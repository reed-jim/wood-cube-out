using UnityEngine;

public class ScrewBoxContainer : MonoBehaviour
{
    public static ScrewBoxContainer Instance;

    private bool _isSetSizeForBoxesOnLevel;

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
        YarnLevelLoader.startLevelEvent += OnLevelStarted;
    }

    void OnDestroy()
    {
        GameplayCamera.updateCameraZoomRatioEvent -= OnCameraZoomed;
        YarnLevelLoader.startLevelEvent -= OnLevelStarted;
    }

    private void OnLevelStarted()
    {
        _isSetSizeForBoxesOnLevel = false;
    }

    private void OnCameraZoomed(float ratio)
    {
        transform.localScale = ratio * Vector3.one;

        if (!_isSetSizeForBoxesOnLevel)
        {
            Vector2 worldSize = CommonUtil.GetScreenSizeWorld(Camera.main);

            SpoolBox[] spoolBoxes = TransformUtil.GetComponentsArrayFromAllChildren<SpoolBox>(transform);

            foreach (var spoolBox in spoolBoxes)
            {
                spoolBox.SetSpoolSize(worldSize);
            }

            _isSetSizeForBoxesOnLevel = true;
        }
    }
}
