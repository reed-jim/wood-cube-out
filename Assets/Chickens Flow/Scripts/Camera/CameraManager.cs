using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float extraOrthographicSize = 1f;

    private float originalSize;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        originalSize = mainCamera.orthographicSize;

        Rect safeArea = Screen.safeArea;

        // Bottom safe area inset exists
        bool hasBottomSafeArea = safeArea.yMin > 0f;

        if (hasBottomSafeArea)
        {
            mainCamera.orthographicSize = originalSize + extraOrthographicSize;
        }
    }
}
