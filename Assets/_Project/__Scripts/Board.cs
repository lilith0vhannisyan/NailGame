using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class Board : MonoBehaviour
{
    [Header("Setup")]
    public List<Nail> nails = new List<Nail>();
    public bool isInitiallyActive = false;
    public List<Board> prerequisiteBoards = new List<Board>();

    [Header("Animation")]
    [SerializeField] private float fallDuration = 1f;
    [SerializeField] private float zoomInAmount = 1.2f;
    [SerializeField] private float fallDistance = 15f;

    private bool isActive = false;
    private bool isRemoved = false;

    void Start()
    {
        if (nails.Count == 0)
            nails.AddRange(GetComponentsInChildren<Nail>());

        SetNailsLocked(true);

        if (isInitiallyActive)
            ActivateBoard();

        GameManager.Instance.RegisterBoard(this);
    }

    public void TryActivate()
    {
        if (isActive || isRemoved) return;

        foreach (Board prereq in prerequisiteBoards)
        {
            if (prereq == null) continue;
            if (!prereq.IsRemoved()) return;
        }

        ActivateBoard();
    }

    void ActivateBoard()
    {
        isActive = true;
        SetNailsLocked(false);
        transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 5, 0.5f);
    }

    void SetNailsLocked(bool locked)
    {
        foreach (Nail n in nails)
            if (n != null) n.SetLocked(locked);
    }

    public bool IsActive() => isActive;
    public bool IsRemoved() => isRemoved;

    public void OnNailRemoved(Nail nail)
    {
        nails.Remove(nail);
        if (nails.Count == 0)
            RemoveBoard();
    }

    void RemoveBoard()
    {
        if (isRemoved) return;
        isRemoved = true;

        Collider col = GetComponent<Collider>();
        if (col) col.enabled = false;

        // Notify GameManager immediately
        GameManager.Instance.OnBoardRemoved(this);

        // Play fall animation AFTER win check
        Sequence seq = DOTween.Sequence();
        Vector3 zoomPos = transform.position + (Camera.main.transform.position - transform.position).normalized * zoomInAmount;
        seq.Append(transform.DOMove(zoomPos, fallDuration * 0.3f).SetEase(Ease.OutQuad));
        seq.Append(transform.DOMoveY(transform.position.y - fallDistance, fallDuration * 0.7f).SetEase(Ease.InQuad));
        seq.Join(transform.DORotate(new Vector3(Random.Range(-60, 60), 0, Random.Range(-90, 90)), fallDuration * 0.7f, RotateMode.LocalAxisAdd));
        seq.Join(transform.DOScale(Vector3.zero, fallDuration * 0.7f).SetEase(Ease.InQuad));
        seq.OnComplete(() => Destroy(gameObject));
    }
}