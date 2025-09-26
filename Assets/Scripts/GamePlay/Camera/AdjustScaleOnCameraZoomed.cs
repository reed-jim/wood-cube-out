using NamPhuThuy;
using UnityEngine;

public class AdjustScaleOnCameraZoomed : MonoBehaviour
{
    void Awake()
    {
        GameplayCamera.updateCameraZoomRatioEvent += OnCameraZoomed;
    }

    void OnDestroy()
    {
        GameplayCamera.updateCameraZoomRatioEvent -= OnCameraZoomed;
    }

    private void OnCameraZoomed(float ratio)
    {
        transform.localScale = ratio * Vector3.one / Mathf.Cos(30 * Mathf.Deg2Rad);
    }
}
