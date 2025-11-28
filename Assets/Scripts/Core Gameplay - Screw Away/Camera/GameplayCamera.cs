using System;
using System.Collections;
using NamPhuThuy;
using NamPhuThuy.Common;
using NamPhuThuy.Data;
using NamPhuThuy.UI;
using PrimeTween;
using UnityEngine;
using UnityEngine.Video;

public class GameplayCamera : MonoBehaviour
{
    [SerializeField] private Camera gameplayCamera;


    [Header("Background")]
    [SerializeField] private Transform background;

    private float _initialOrthographicSize;
    private float _targetOrthographicSize;

    private float TargetOrthographicSize
    {
        set
        {
            _targetOrthographicSize = value;
            gameplayCamera.orthographicSize = _targetOrthographicSize;
            updateCameraZoomRatioEvent?.Invoke(gameplayCamera.orthographicSize / _initialOrthographicSize);
        }
    }


    private Vector3 _initialBackgroundScale;
    private Vector3 _targetBackgroundScale;

    public static event Action<float> updateCameraZoomRatioEvent;
    public static event Action<bool> enablePinchingEvent;

    #region Callbacks

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
        /*gameplayCamera.orthographicSize = Mathf.Lerp(gameplayCamera.orthographicSize, _targetOrthographicSize, 1 / 12f);

        // background.localScale = Vector3.Lerp(background.localScale, _targetBackgroundScale, 1 / 12f);

        updateCameraZoomRatioEvent?.Invoke(gameplayCamera.orthographicSize / _initialOrthographicSize);*/
    }


    #endregion

    private void OnLevelStart()
    {
        Reset();
    }

    private void Reset()
    {
        TargetOrthographicSize = _initialOrthographicSize;
        _targetBackgroundScale = _initialBackgroundScale;
    }

    private void Zoom(float orthographicSize)
    {
        TargetOrthographicSize = orthographicSize;
        _targetBackgroundScale = _initialBackgroundScale * orthographicSize / _initialOrthographicSize;
    }

    private void SetInitialOrthographicSize(float orthographicSize)
    {
        float _prevOrthographicSize = gameplayCamera.orthographicSize;

        enablePinchingEvent?.Invoke(false);

        Tween.Custom(_prevOrthographicSize, orthographicSize, duration: 0.3f, ease: Ease.InOutSine, onValueChange: newVal =>
        {
            TargetOrthographicSize = newVal;
            _targetBackgroundScale = _initialBackgroundScale * orthographicSize / _initialOrthographicSize;
        })
        .OnComplete(() =>
        {
            enablePinchingEvent?.Invoke(true);
        });
    }

    private void Shake()
    {
        Tween.ShakeCamera(gameplayCamera, 1, duration: 0.3f);
    }
}
