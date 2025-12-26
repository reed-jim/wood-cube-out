using NamPhuThuy.GamePlay;
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
        LevelLoader.startLevelEvent += OnLevelStarted;
    }

    void OnDestroy()
    {
        GameplayCamera.updateCameraZoomRatioEvent -= OnCameraZoomed;
        LevelLoader.startLevelEvent -= OnLevelStarted;
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

            ScrewBox[] spoolBoxes = TransformUtil.GetComponentsArrayFromAllChildren<ScrewBox>(transform);

            foreach (var spoolBox in spoolBoxes)
            {
                spoolBox.SetSize(worldSize);
            }

            _isSetSizeForBoxesOnLevel = true;
        }
    }
}
