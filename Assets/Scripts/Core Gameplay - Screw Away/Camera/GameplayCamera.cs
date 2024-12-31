using PrimeTween;
using UnityEngine;

public class GameplayCamera : MonoBehaviour
{
    [SerializeField] private Camera gameplayCamera;

    private float _initialOrthographicSize;
    private float _targetOrthographicSize;

    private void Awake()
    {
        LevelLoader.startLevelEvent += OnLevelStart;
        PinchGesture.pinchGestureEvent += Zoom;
        BasicObjectPart.shakeCameraEvent += Shake;
        MultiPhaseLevelManager.zoomCameraEvent += Zoom;

        _initialOrthographicSize = gameplayCamera.orthographicSize;
        _targetOrthographicSize = gameplayCamera.orthographicSize;
    }

    private void OnDestroy()
    {
        LevelLoader.startLevelEvent -= OnLevelStart;
        PinchGesture.pinchGestureEvent -= Zoom;
        BasicObjectPart.shakeCameraEvent -= Shake;
        MultiPhaseLevelManager.zoomCameraEvent -= Zoom;
    }

    private void Update()
    {
        gameplayCamera.orthographicSize = Mathf.Lerp(gameplayCamera.orthographicSize, _targetOrthographicSize, 0.333f);
    }

    private void OnLevelStart()
    {
        _targetOrthographicSize = _initialOrthographicSize;
    }

    private void Zoom(float orthographicSize)
    {
        _targetOrthographicSize = orthographicSize;
    }

    private void Shake()
    {
        Tween.ShakeCamera(gameplayCamera, 1, duration: 0.3f);
    }
}
