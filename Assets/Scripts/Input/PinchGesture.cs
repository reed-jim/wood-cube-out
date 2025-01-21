using System;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class PinchGesture : MonoBehaviour
{
    public Camera cameraToZoom;

    [SerializeField] private Slider zoomSlider;

    public float zoomSpeed = 0.1f;
    public float minZoom = 10f;
    public float maxZoom = 100f;
    public float delayToEnableSwipeGesture;

    private List<Tween> _tweens;
    private bool _isDisabled;

    #region ACTION
    public static event Action<float> pinchGestureEvent;
    public static event Action<bool> enableSwipingEvent;
    #endregion

    private void Awake()
    {
        LevelLoader.startLevelEvent += OnLevelStarted;
        BasePopup.enableSwipingScreenEvent += EnablePinching;
        LevelTutorial.enablePinchingEvent += EnablePinching;

        zoomSlider.onValueChanged.AddListener(OnZoomSliderValueChanged);

        _tweens = new List<Tween>();

        minZoom = 0.85f * cameraToZoom.orthographicSize;
        maxZoom = 1.15f * cameraToZoom.orthographicSize;
    }

    void OnDestroy()
    {
        LevelLoader.startLevelEvent -= OnLevelStarted;
        BasePopup.enableSwipingScreenEvent -= EnablePinching;
        LevelTutorial.enablePinchingEvent -= EnablePinching;

        CommonUtil.StopAllTweens(_tweens);
    }

    private void OnZoomSliderValueChanged(float value)
    {
        pinchGestureEvent?.Invoke(maxZoom - value * (maxZoom - minZoom));

        enableSwipingEvent?.Invoke(false);

        CommonUtil.StopAllTweens(_tweens);

        _tweens.Add(Tween.Delay(delayToEnableSwipeGesture).OnComplete(() =>
        {
            enableSwipingEvent?.Invoke(true);
        }));
    }

    private void Update()
    {
        // if (_isDisabled)
        // {
        //     return;
        // }

        // if (Input.touchCount == 2)
        // {
        //     Touch touch1 = Input.GetTouch(0);
        //     Touch touch2 = Input.GetTouch(1);

        //     Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
        //     Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;

        //     float prevTouchDeltaMag = (touch1PrevPos - touch2PrevPos).magnitude;
        //     float touchDeltaMag = (touch1.position - touch2.position).magnitude;

        //     float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

        //     pinchGestureEvent?.Invoke(Mathf.Clamp(cameraToZoom.orthographicSize + deltaMagnitudeDiff * zoomSpeed, minZoom, maxZoom));
        // }
    }

    private void OnLevelStarted()
    {
        zoomSlider.value = 0;

        // pinchGestureEvent?.Invoke(maxZoom);
    }

    private void EnablePinching(bool isEnable)
    {
        _isDisabled = !isEnable;
    }
}
