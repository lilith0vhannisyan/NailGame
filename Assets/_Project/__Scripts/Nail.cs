using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Collections;

public class Nail : MonoBehaviour, IPointerClickHandler
{
    [Header("Floor Landing Animation")]
    [SerializeField] private float flightDuration = 0.6f;
    [SerializeField] private float floorY = -3.5f;
    [SerializeField] private float horizontalSpread = 1.5f;

    [Header("Physics Settings")]
    [SerializeField] private float finalScaleMultiplier = 0.6f;
    [SerializeField] private float lifetimeOnFloor = 2f;
    [SerializeField] private float shrinkDuration = 0.5f;

    private Board parentBoard;
    private bool isRemoved = false;
    private bool isLocked = true;

    void Start()
    {
        parentBoard = GetComponentInParent<Board>();

        if (parentBoard == null)
        {
            Board[] allBoards = FindObjectsByType<Board>(FindObjectsInactive.Include);
            foreach (Board b in allBoards)
            {
                if (b.nails.Contains(this))
                {
                    parentBoard = b;
                    break;
                }
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver()) return;
        TryRemove();
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;
        Collider col = GetComponent<Collider>();
        if (col) col.enabled = !locked;
    }

    public void TryRemove()
    {
        if (isRemoved || isLocked) return;

        if (!GameManager.Instance.HasHammer())
        {
            GameManager.Instance.ShowNoHammerFeedback();
            return;
        }

        GameManager.Instance.UseHammerOn(this);
    }

    public void CompleteRemove()
    {
        if (isRemoved) return;
        isRemoved = true;

        transform.SetParent(null);

        Collider col = GetComponent<Collider>();
        if (col) col.enabled = false;

        if (parentBoard != null)
            parentBoard.OnNailRemoved(this);

        PlayRemoveAnim();
    }

    void PlayRemoveAnim()
    {
        Vector3 startPos = transform.position;

        // Step 1: Move forward toward camera
        Vector3 forwardPos = startPos + new Vector3(
            Random.Range(-0.3f, 0.3f),  // slight random X
            0f,
            -1.5f                        // toward camera
        );

        // Step 2: Floor position
        Vector3 floorPos = new Vector3(
            forwardPos.x,
            floorY,                      // your floor Y value
            forwardPos.z
        );

        Vector3 endScale = transform.localScale * finalScaleMultiplier;

        Sequence seq = DOTween.Sequence();

        // Move forward quickly
        seq.Append(transform.DOMove(forwardPos, 0.2f).SetEase(Ease.OutQuad));

        // Fall down to floor + shrink + rotate to lie flat
        seq.Append(transform.DOMove(floorPos, 0.5f).SetEase(Ease.InQuad));
        seq.Join(transform.DOScale(endScale, 0.5f).SetEase(Ease.InQuad));
        seq.Join(transform.DORotate(
            new Vector3(-90f, Random.Range(-45f, 45f), 0f),
            0.5f,
            RotateMode.Fast
        ));

        // Lie on floor for a moment
        seq.AppendInterval(lifetimeOnFloor);

        // Shrink to zero and destroy
        seq.Append(transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InQuad));
        seq.OnComplete(() => Destroy(gameObject));
    }

}