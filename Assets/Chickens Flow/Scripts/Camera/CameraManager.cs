using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private const float ReferenceWidth = 1080f;
    private const float ReferenceHeight = 1920f;

    [SerializeField] private Camera mainCamera;

    // Set this to whatever orthographic size you use at 1080x1920.
    [SerializeField] private float orthographicSizeToAdd = 2f;

    private void Awake()
    {
        AdjustCamera();
    }

    private void AdjustCamera()
    {
        float referenceAspect = ReferenceHeight / ReferenceWidth;
        float currentAspect = (float)Screen.height / Screen.width;

        if (currentAspect < referenceAspect)
        {
            mainCamera.orthographicSize += orthographicSizeToAdd;
        }
    }

    private void OnRectTransformDimensionsChange()
    {
        if (mainCamera != null)
            AdjustCamera();
    }
}
