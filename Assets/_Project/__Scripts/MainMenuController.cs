using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainButtons;
    [SerializeField] private GameObject levelsButtons;
    [SerializeField] private GameObject settingsButtons;

    [Header("Sound Buttons")]
    [SerializeField] private GameObject soundOnButton;     // shown when music is ON
    [SerializeField] private GameObject soundOffButton;    // shown when music is OFF
    [SerializeField] private AudioSource musicSource;      // optional

    private const string MUSIC_KEY = "MusicOn";
    private const string UNLOCKED_KEY = "UnlockedLevel";

    void Start()
    {
        ShowMainMenu();
        ApplyMusicSetting();
    }

    // ===== PANEL SWITCHING =====
    public void ShowMainMenu()
    {
        if (mainButtons) mainButtons.SetActive(true);
        if (levelsButtons) levelsButtons.SetActive(false);
        if (settingsButtons) settingsButtons.SetActive(false);
    }

    public void ShowLevels()
    {
        if (mainButtons) mainButtons.SetActive(false);
        if (levelsButtons) levelsButtons.SetActive(true);
        if (settingsButtons) settingsButtons.SetActive(false);
    }

    public void ShowSettings()
    {
        if (mainButtons) mainButtons.SetActive(false);
        if (levelsButtons) levelsButtons.SetActive(false);
        if (settingsButtons) settingsButtons.SetActive(true);
        ApplyMusicSetting();   // refresh visible button
    }

    // ===== PLAY =====
    public void OnPlay()
    {
        int unlocked = PlayerPrefs.GetInt(UNLOCKED_KEY, 1);
        SceneManager.LoadScene("Level" + unlocked);
    }

    public void LoadLevel(int levelNumber)
    {
        int unlocked = PlayerPrefs.GetInt(UNLOCKED_KEY, 1);
        if (levelNumber > unlocked)
        {
            Debug.Log("Level " + levelNumber + " is locked");
            return;
        }
        SceneManager.LoadScene("Level" + levelNumber);
    }

    public void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ===== SOUND TOGGLE =====
    // Called when player clicks SoundOnButton (currently ON, wants to turn OFF)
    public void TurnSoundOff()
    {
        PlayerPrefs.SetInt(MUSIC_KEY, 0);
        PlayerPrefs.Save();
        ApplyMusicSetting();
    }

    // Called when player clicks SoundOffButton (currently OFF, wants to turn ON)
    public void TurnSoundOn()
    {
        PlayerPrefs.SetInt(MUSIC_KEY, 1);
        PlayerPrefs.Save();
        ApplyMusicSetting();
    }

    void ApplyMusicSetting()
    {
        bool on = PlayerPrefs.GetInt(MUSIC_KEY, 1) == 1;
        if (musicSource) musicSource.mute = !on;
        if (soundOnButton) soundOnButton.SetActive(on);
        if (soundOffButton) soundOffButton.SetActive(!on);
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("Progress reset");
    }
}