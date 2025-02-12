using UnityEngine;

public class ScrewBoxContainer : MonoBehaviour
{
    public static ScrewBoxContainer Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);

            return;
        }
        else
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
        }

        GameplayCamera.updateCameraZoomRatioEvent += OnCameraZoomed;
    }

    void OnDestroy()
    {
        GameplayCamera.updateCameraZoomRatioEvent -= OnCameraZoomed;
    }

    private void OnCameraZoomed(float ratio)
    {
        transform.localScale = ratio * Vector3.one;
    }
}
