using UnityEngine;
using DG.Tweening;

public class KeyAnimator : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Transform keyEntryPoint;

    [Header("Animation")]
    [SerializeField] private float forwardAmount = 0.5f;
    [SerializeField] private float moveSpeed = 0.8f;

    public void MoveToNextWaypoint() { }

    public void EnterChestAndOpen(Chest chest)
    {
        if (keyEntryPoint == null)
        {
            chest.OpenChest();
            return;
        }

        Vector3 forwardPos = transform.position + new Vector3(0, 0, -forwardAmount);
        Vector3 targetRotation = new Vector3(357.916321f, 94.5781479f, 178.429108f);

        Sequence seq = DOTween.Sequence();

        // Come forward
        seq.Append(transform.DOMove(forwardPos, 0.3f).SetEase(Ease.OutQuad));

        // Fly to entry point
        seq.Append(transform.DOMove(keyEntryPoint.position, moveSpeed).SetEase(Ease.InOutSine));

        // Rotate to exact rotation during flight
        seq.Join(transform.DORotate(targetRotation, moveSpeed, RotateMode.Fast).SetEase(Ease.InOutSine));

        // Open chest after arriving — NO shrink
        seq.OnComplete(() => chest.OpenChest());
    }
}