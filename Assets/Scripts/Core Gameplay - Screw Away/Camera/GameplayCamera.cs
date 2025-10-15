using System;
using PrimeTween;
using UnityEngine;

public class GameplayCamera : MonoBehaviour
{
    [SerializeField] private Camera gameplayCamera;
    [SerializeField] private Transform background;

    private float _initialOrthographicSize;
    private float _targetOrthographicSize;
    private Vector3 _initialBackgroundScale;
    private Vector3 _targetBackgroundScale;

    public static event Action<float> updateCameraZoomRatioEvent;

    private void Awake()
    {
        YarnLevelLoader.startLevelEvent += OnLevelStart;
        YarnLevelLoader.setTargetOrthographicSizeEvent += SetInitialOrthographicSize;
        MultiPhaseLevelManager.setTargetOrthographicSizeEvent += SetInitialOrthographicSize;
        PinchGesture.pinchGestureEvent += Zoom;
        BasicObjectPart.shakeCameraEvent += Shake;
        MultiPhaseLevelManager.zoomCameraEvent += Zoom;
        MultiPhaseLevelManager.resetCameraEvent += Reset;

        _initialOrthographicSize = gameplayCamera.orthographicSize;
        _targetOrthographicSize = gameplayCamera.orthographicSize;

        _initialBackgroundScale = background.localScale;
        _targetBackgroundScale = background.localScale;

        GamePersistentVariable.screenSizeWorld = CommonUtil.GetScreenSizeWorld(gameplayCamera);
    }

    private void OnDestroy()
    {
        YarnLevelLoader.startLevelEvent -= OnLevelStart;
        YarnLevelLoader.setTargetOrthographicSizeEvent -= SetInitialOrthographicSize;
        MultiPhaseLevelManager.setTargetOrthographicSizeEvent -= SetInitialOrthographicSize;
        PinchGesture.pinchGestureEvent -= Zoom;
        BasicObjectPart.shakeCameraEvent -= Shake;
        MultiPhaseLevelManager.zoomCameraEvent -= Zoom;
        MultiPhaseLevelManager.resetCameraEvent -= Reset;
    }

    private void Update()
    {
        gameplayCamera.orthographicSize = Mathf.Lerp(gameplayCamera.orthographicSize, _targetOrthographicSize, 0.333f);

        background.localScale = Vector3.Lerp(background.localScale, _targetBackgroundScale, 0.333f);

        updateCameraZoomRatioEvent?.Invoke(gameplayCamera.orthographicSize / _initialOrthographicSize);
    }

    private void OnLevelStart()
    {
        Reset();
    }

    private void Reset()
    {
        // _targetOrthographicSize = _initialOrthographicSize;
        // _targetBackgroundScale = _initialBackgroundScale;
    }

    private void Zoom(float orthographicSize)
    {
        _targetOrthographicSize = orthographicSize;
        _targetBackgroundScale = _initialBackgroundScale * orthographicSize / _initialOrthographicSize;
    }

    private void SetInitialOrthographicSize(float orthographicSize)
    {
        _targetOrthographicSize = orthographicSize;
        _targetBackgroundScale = _initialBackgroundScale * orthographicSize / _initialOrthographicSize;
    }

    private void Shake()
    {
        Tween.ShakeCamera(gameplayCamera, 1, duration: 0.3f);
    }
}
