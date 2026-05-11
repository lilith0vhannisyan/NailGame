using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class HammerBox : MonoBehaviour, IPointerClickHandler
{
    [Header("Setup")]
    public int hammerReward = 3;
    public List<Board> prerequisiteBoards = new List<Board>();

    [Header("Visuals")]
    [SerializeField] private GameObject flyingHammerPrefab;
    [SerializeField] private float flightDuration = 0.7f;
    [SerializeField] private float arcHeight = 2f;
    [SerializeField] private float delayBetweenHammers = 0.1f;

    private bool isActive = false;
    private bool isCollected = false;

    public bool IsCollected() => isCollected;

    void Start()
    {
        SetLocked(true);
        GameManager.Instance.RegisterHammerBox(this);

        Debug.Log("HAMMERBOX START: layer=" + LayerMask.LayerToName(gameObject.layer) +
                  " prereqs=" + prerequisiteBoards.Count +
                  " collider=" + (GetComponent<Collider>() != null));

        if (prerequisiteBoards.Count == 0)
            Activate();
    }

    public void TryActivate()
    {
        if (isActive || isCollected) return;

        foreach (Board prereq in prerequisiteBoards)
        {
            if (prereq != null && !prereq.IsRemoved())
                return;
        }

        Activate();
    }

    void Activate()
    {
        isActive = true;
        SetLocked(false);
        transform.DOPunchScale(Vector3.one * 0.15f, 0.4f, 5, 0.5f);
    }

    void SetLocked(bool locked)
    {
        Collider col = GetComponent<Collider>();
        if (col) col.enabled = !locked;
    }

    public bool IsActiveAndAvailable()
    {
        return isActive && !isCollected;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("HAMMERBOX CLICKED! isActive=" + isActive + " isCollected=" + isCollected);
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver()) return;
        if (!isActive || isCollected) return;
        Collect();
    }

    void Collect()
    {
        isCollected = true;
        Collider col = GetComponent<Collider>();
        if (col) col.enabled = false;

        StartCoroutine(SpawnFlyingHammers());

        Sequence boxSeq = DOTween.Sequence();
        boxSeq.Append(transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack));
        boxSeq.Join(transform.DORotate(new Vector3(0, 360, 0), 0.5f, RotateMode.LocalAxisAdd));
        boxSeq.OnComplete(() => Destroy(gameObject));
    }

    IEnumerator SpawnFlyingHammers()
    {
        Vector3 targetPos = GameManager.Instance.GetHammerBoxWorldPosition();

        for (int i = 0; i < hammerReward; i++)
        {
            SpawnOneFlyingHammer(targetPos);
            yield return new WaitForSeconds(delayBetweenHammers);
        }
    }

    void SpawnOneFlyingHammer(Vector3 targetPos)
    {
        if (flyingHammerPrefab == null)
        {
            GameManager.Instance.AddHammers(1);
            return;
        }

        Vector3 startPos = transform.position + new Vector3(
            Random.Range(-0.2f, 0.2f),
            Random.Range(-0.2f, 0.2f),
            0
        );

        GameObject flyer = Instantiate(flyingHammerPrefab, startPos, Quaternion.identity);
        Vector3 originalScale = flyer.transform.localScale;

        Vector3 midPoint = (startPos + targetPos) / 2f + Vector3.up * arcHeight;
        Vector3[] path = new Vector3[] { startPos, midPoint, targetPos };

        Sequence seq = DOTween.Sequence();
        seq.Append(flyer.transform.DOPath(path, flightDuration, PathType.CatmullRom).SetEase(Ease.InOutSine));
        seq.Join(flyer.transform.DORotate(new Vector3(0, 0, 720), flightDuration, RotateMode.FastBeyond360));
        seq.Join(flyer.transform.DOScale(originalScale * 0.5f, flightDuration));
        seq.OnComplete(() =>
        {
            GameManager.Instance.AddHammers(1);
            Destroy(flyer);
        });
    }
}