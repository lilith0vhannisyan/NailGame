using UnityEngine;
using DG.Tweening;

public class HammerController : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Transform homePosition;
    [SerializeField] private float flightDuration = 0.3f;

    [Header("Strike Settings")]
    [SerializeField] private float strikeForwardOffset = 0.5f;
    [SerializeField] private float strikeHeightAbove = 0.3f;
    [SerializeField] private float strikeScaleMultiplier = 1.5f;
    [SerializeField] private float windUpAngle = -45f;
    [SerializeField] private float strikeAngle = 60f;
    [SerializeField] private float windUpDuration = 0.15f;
    [SerializeField] private float strikeDuration = 0.1f;
    [SerializeField] private float impactPause = 0.05f;

    [Header("Swing Axis")]
    [Tooltip("Which axis the hammer swings on. X is typical. Try Z if hammer swings sideways instead of down.")]
    [SerializeField] private Vector3 swingAxis = new Vector3(1, 0, 0);   // X axis by default

    private Vector3 homePos;
    private Quaternion homeRot;
    private Vector3 homeScale;
    private bool isBusy = false;
    private bool isVisible = true;

    void Start()
    {
        if (homePosition != null)
        {
            homePos = homePosition.position;
            homeRot = homePosition.rotation;
        }
        else
        {
            homePos = transform.position;
            homeRot = transform.rotation;
        }
        homeScale = transform.localScale;
    }

    public void StrikeNail(Nail target, System.Action onHit)
    {
        if (isBusy || target == null || !isVisible)
        {
            onHit?.Invoke();
            return;
        }
        isBusy = true;

        Vector3 nailPos = target.transform.position;

        // Position above and in front of nail (toward camera)
        Vector3 abovePos = nailPos + new Vector3(0, strikeHeightAbove + 0.4f, -strikeForwardOffset);
        Vector3 strikePos = nailPos + new Vector3(0, strikeHeightAbove, -strikeForwardOffset * 0.5f);

        // Rotations relative to home rotation
        Quaternion windUpRot = homeRot * Quaternion.Euler(swingAxis * windUpAngle);
        Quaternion impactRot = homeRot * Quaternion.Euler(swingAxis * strikeAngle);

        Vector3 biggerScale = homeScale * strikeScaleMultiplier;

        Sequence seq = DOTween.Sequence();

        // PHASE 1: Fly to above-nail, grow, wind-up
        seq.Append(transform.DOMove(abovePos, flightDuration).SetEase(Ease.OutQuad));
        seq.Join(transform.DOScale(biggerScale, flightDuration).SetEase(Ease.OutQuad));
        seq.Join(transform.DORotateQuaternion(windUpRot, flightDuration).SetEase(Ease.OutQuad));

        // PHASE 2: Hold wind-up briefly
        seq.AppendInterval(windUpDuration);

        // PHASE 3: STRIKE - swing down and forward
        seq.Append(transform.DOMove(strikePos, strikeDuration).SetEase(Ease.InQuad));
        seq.Join(transform.DORotateQuaternion(impactRot, strikeDuration).SetEase(Ease.InQuad));

        // IMPACT
        seq.AppendCallback(() => onHit?.Invoke());
        seq.AppendInterval(impactPause);

        // PHASE 4: Return home — position, scale, rotation
        seq.Append(transform.DOMove(homePos, flightDuration).SetEase(Ease.OutQuad));
        seq.Join(transform.DOScale(homeScale, flightDuration));
        seq.Join(transform.DORotateQuaternion(homeRot, flightDuration));

        seq.OnComplete(() => { isBusy = false; });
    }

    public void HideAndDestroy()
    {
        if (!isVisible) return;
        isVisible = false;
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InBack));
        seq.Join(transform.DORotate(new Vector3(0, 720, 0), 0.4f, RotateMode.LocalAxisAdd));
        seq.OnComplete(() => gameObject.SetActive(false));
    }

    public void ShowAgain()
    {
        if (isVisible) return;
        isVisible = true;
        gameObject.SetActive(true);
        transform.position = homePos;
        transform.rotation = homeRot;
        transform.localScale = Vector3.zero;
        transform.DOScale(homeScale, 0.4f).SetEase(Ease.OutBack);
    }

    public bool IsVisible() => isVisible;
}