using UnityEngine;

public class CameraScaler : MonoBehaviour
{
    [SerializeField] private float referenceAspect = 9f / 16f; // portrait reference
    [SerializeField] private float referenceFOV = 60f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        AdjustCamera();
    }

    void AdjustCamera()
    {
        float currentAspect = (float)Screen.width / Screen.height;
        float aspectRatio = referenceAspect / currentAspect;
        cam.fieldOfView = referenceFOV * aspectRatio;
    }
}