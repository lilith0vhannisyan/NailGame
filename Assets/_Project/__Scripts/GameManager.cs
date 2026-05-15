using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Main Hammer Box Reference")]
    [SerializeField] private Transform mainHammerBoxTransform;

    [Header("Level Settings")]
    [SerializeField] private int startingHammers = 5;

    [Header("References")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject treasureChest;
    [SerializeField] private HammerController hammerController;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseSound;

    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI movesText;
    [SerializeField] private TextMeshProUGUI hammerCountText;
    [SerializeField] private Transform hammerBoxIcon;
    [SerializeField] private GameObject noHammerWarning;

    private List<Board> allBoards = new List<Board>();
    private List<HammerBox> hammerBoxes = new List<HammerBox>();
    private int currentHammers;
    private int movesUsed = 0;
    private bool gameWon = false;
    private bool gameLost = false;
    private bool isPaused = false;
    private Chest chest;
    private float loseCheckTimer = 0f;
    private bool isLosingSequenceRunning = false;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Cap framerate — saves battery, reduces heat
        Application.targetFrameRate = 30;
        QualitySettings.vSyncCount = 0;
    }

    void Start()
    {
        Time.timeScale = 1f;
        currentHammers = startingHammers;

        if (winPanel) winPanel.SetActive(false);
        if (losePanel) losePanel.SetActive(false);
        if (pauseMenu) pauseMenu.SetActive(false);
        if (noHammerWarning) noHammerWarning.SetActive(false);

        UpdateHUD();

        if (currentHammers <= 0 && hammerController != null)
            hammerController.HideAndDestroy();
    }

    void Update()
    {
        if (gameWon || gameLost || isPaused) return;
        if (currentHammers > 0) return;

        loseCheckTimer += Time.deltaTime;
        if (loseCheckTimer >= 0.5f)
        {
            loseCheckTimer = 0f;
            CheckLoseCondition();
        }
    }

    void UpdateHUD()
    {
        if (levelText) levelText.text = "Level " + SceneManager.GetActiveScene().buildIndex;
        if (movesText) movesText.text = "Moves: " + movesUsed;
        if (hammerCountText) hammerCountText.text = currentHammers.ToString();
    }

    public void RegisterBoard(Board b)
    {
        if (!allBoards.Contains(b)) allBoards.Add(b);
    }

    public void RegisterHammerBox(HammerBox box)
    {
        if (!hammerBoxes.Contains(box)) hammerBoxes.Add(box);
    }

    public void RegisterChest(Chest c)
    {
        chest = c;
    }

    public List<Nail> GetAllNails()
    {
        return new List<Nail>(FindObjectsByType<Nail>(FindObjectsInactive.Exclude));
    }

    // ===== HAMMERS =====
    public bool HasHammer() => currentHammers > 0;

    public void AddHammers(int amount)
    {
        bool wasZero = currentHammers <= 0;
        currentHammers += amount;
        UpdateHUD();

        // Cancel any running lose sequence
        isLosingSequenceRunning = false;    // ← ADD THIS

        Transform bounceTarget = (hammerBoxIcon != null) ? hammerBoxIcon : mainHammerBoxTransform;
        if (bounceTarget != null)
            bounceTarget.DOPunchScale(Vector3.one * 0.3f, 0.4f, 5, 0.5f);

        if (wasZero && currentHammers > 0 && hammerController != null && !hammerController.IsVisible())
            hammerController.ShowAgain();
    }

    public Vector3 GetHammerBoxWorldPosition()
    {
        if (mainHammerBoxTransform != null)
            return mainHammerBoxTransform.position;
        return Vector3.zero;
    }

    public void UseHammerOn(Nail target)
    {
        if (currentHammers <= 0) return;

        currentHammers--;
        UpdateHUD();
        movesUsed++;
        if (movesText) movesText.text = "Moves: " + movesUsed;

        if (hammerController != null)
        {
            hammerController.StrikeNail(target, () => {
                target.CompleteRemove();
                CheckBoardsAndBoxesActivation();
                CheckHammerVisibility();
                CheckLoseCondition();
            });
        }
        else
        {
            target.CompleteRemove();
            CheckBoardsAndBoxesActivation();
            CheckHammerVisibility();
            CheckLoseCondition();
        }
    }

    void CheckHammerVisibility()
    {
        if (currentHammers <= 0 && hammerController != null && hammerController.IsVisible())
            hammerController.HideAndDestroy();
    }

    public void ShowNoHammerFeedback()
    {
        if (hammerBoxIcon)
            hammerBoxIcon.DOShakePosition(0.3f, 0.2f, 20, 90);
        if (noHammerWarning)
        {
            noHammerWarning.SetActive(true);
            CancelInvoke(nameof(HideNoHammer));
            Invoke(nameof(HideNoHammer), 1.2f);
        }
    }
    void HideNoHammer() { if (noHammerWarning) noHammerWarning.SetActive(false); }

    // ===== BOARD REMOVAL =====
    public void OnBoardRemoved(Board b)
    {
        allBoards.Remove(b);
        CheckBoardsAndBoxesActivation();
        CheckWinCondition();

    }

    void CheckBoardsAndBoxesActivation()
    {
        foreach (Board b in allBoards)
            if (b != null) b.TryActivate();

        foreach (HammerBox box in hammerBoxes)
            if (box != null) box.TryActivate();
    }

    // ===== WIN =====
    public void CheckWinCondition()
    {
        if (gameWon || gameLost) return;

        if (chest != null)
        {
            if (chest.IsUnlocked())
                StartCoroutine(WinSequence());
        }
        else
        {
            if (allBoards.Count == 0)
                StartCoroutine(WinSequence());
        }
    }

    // ===== LOSE =====
    void CheckLoseCondition()
    {
        if (gameWon || gameLost) return;
        if (currentHammers > 0) return;
        if (isLosingSequenceRunning) return;  

        foreach (HammerBox box in hammerBoxes)
            if (box != null && box.IsActiveAndAvailable())
                return;

        isLosingSequenceRunning = true;
        StartCoroutine(LoseSequence());
    }


    IEnumerator WinSequence()
    {
        if (gameWon) yield break;
        gameWon = true;

        int currentLevel = SceneManager.GetActiveScene().buildIndex;
        int unlocked = PlayerPrefs.GetInt("UnlockedLevel", 1);
        if (currentLevel + 1 > unlocked)
            PlayerPrefs.SetInt("UnlockedLevel", currentLevel + 1);
        PlayerPrefs.Save();

        yield return new WaitForSeconds(0.5f);

        // Trigger key → chest sequence
        if (chest != null)
            chest.PlayOpenSequence();
        else
        {
            if (treasureChest != null)
                treasureChest.transform.DOPunchScale(Vector3.one * 0.35f, 0.6f, 5, 0.5f);
            yield return new WaitForSeconds(1f);
            if (winPanel)
            {
                if (audioSource != null && winSound != null)
                    audioSource.PlayOneShot(winSound);
                winPanel.SetActive(true);
            }
                

        }
    }

    IEnumerator LoseSequence()
    {
        yield return new WaitForSeconds(1.5f); 

        // After waiting, check everything again
        if (gameWon)
        {
            isLosingSequenceRunning = false;
            yield break;
        }

        if (currentHammers > 0)
        {
            isLosingSequenceRunning = false;
            yield break;
        }

        foreach (HammerBox box in hammerBoxes)
        {
            if (box != null && box.IsActiveAndAvailable())
            {
                isLosingSequenceRunning = false;
                yield break;
            }
        }

        gameLost = true;
        if (losePanel)
        {
            audioSource.PlayOneShot(loseSound);
            losePanel.SetActive(true);
        }
    }
    public void OnChestOpened()
    {
        StartCoroutine(ShowWinAfterChest());
    }
    IEnumerator ShowWinAfterChest()
    {
        yield return new WaitForSeconds(1.5f);
        if (winPanel)
        {
            winPanel.SetActive(true);
            if (audioSource && winSound)
            {
                audioSource.PlayOneShot(winSound);
            }
        }


    }
    public bool IsGameOver() => gameWon || gameLost || isPaused;

    // ===== PAUSE =====
    public void OnPause()
    {
        if (gameWon || gameLost) return;
        isPaused = true;
        Time.timeScale = 0f;
        if (pauseMenu) pauseMenu.SetActive(true);
    }

    public void OnContinue()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pauseMenu) pauseMenu.SetActive(false);
    }

    // ===== SCENE BUTTONS =====
    public void OnRestart()
    {
        Time.timeScale = 1f;
        DOTween.KillAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnNextLevel()
    {
        Time.timeScale = 1f;
        DOTween.KillAll();
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(next);
        else
            SceneManager.LoadScene("MainMenu");
    }

    public void OnMainMenu()
    {
        Time.timeScale = 1f;
        DOTween.KillAll();
        SceneManager.LoadScene("MainMenu");
    }
}