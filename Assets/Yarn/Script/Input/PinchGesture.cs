using System;
using System.Collections.Generic;
using NamPhuThuy.Common;
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
    public float changeThreshold;
    public float delayToEnableSwipeGesture;

    private List<Tween> _tweens;
    private Tween _autoZoomTween;
    private bool _isDisabled;
    private float _zoomPercent;
    private float _lastZoomPercent;
    private bool _isZoomByPinching;

    #region EVENTS
    public static event Action<float> pinchGestureEvent;
    public static event Action<bool> enableSwipingEvent;
    public event Action<bool> isPinchingEvent; 
    #endregion

    #region MonoBehaviour Callbacks

    private void Awake()
    {
        YarnLevelLoader.startLevelEvent += OnLevelStarted;
        YarnLevelLoader.setTargetOrthographicSizeEvent += SetNewCameraOrthographicSize;
        MultiPhaseLevelManager.setTargetOrthographicSizeEvent += SetNewCameraOrthographicSize;
        GameplayCamera.enablePinchingEvent += EnablePinching;
        BasePopup.enableSwipingScreenEvent += EnablePinching;
        LevelTutorial.enablePinchingEvent += EnablePinching;

        zoomSlider.onValueChanged.AddListener(OnZoomSliderValueChanged);
        
        // Add event triggers for pointer down/up
        
        UnityEngine.EventSystems.EventTrigger trigger = zoomSlider.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null)
        {
            trigger = zoomSlider.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        }

        UnityEngine.EventSystems.EventTrigger.Entry pointerDownEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerDownEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
        pointerDownEntry.callback.AddListener((data) => { OnSliderPointerDown(); });
        trigger.triggers.Add(pointerDownEntry);

        UnityEngine.EventSystems.EventTrigger.Entry pointerUpEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerUpEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
        pointerUpEntry.callback.AddListener((data) => { OnSliderPointerUp(); });
        trigger.triggers.Add(pointerUpEntry);
        
        _tweens = new List<Tween>();
    }

    void OnDestroy()
    {
        YarnLevelLoader.startLevelEvent -= OnLevelStarted;
        YarnLevelLoader.setTargetOrthographicSizeEvent -= SetNewCameraOrthographicSize;
        MultiPhaseLevelManager.setTargetOrthographicSizeEvent -= SetNewCameraOrthographicSize;
        GameplayCamera.enablePinchingEvent -= EnablePinching;
        BasePopup.enableSwipingScreenEvent -= EnablePinching;
        LevelTutorial.enablePinchingEvent -= EnablePinching;

        CommonUtil.StopAllTweens(_tweens);
    }

    private void Update()
    {
        if (_isDisabled)
        {
            return;
        }

        if (Input.touchCount == 2)
        {
            /*if (!_isZoomByPinching)
            {
                isPinchingEvent?.Invoke(true);
            }*/
            
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
            Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;

            float prevTouchDeltaMag = (touch1PrevPos - touch2PrevPos).magnitude;
            float touchDeltaMag = (touch1.position - touch2.position).magnitude;

            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            float targetOrthographicSize = Mathf.Clamp(cameraToZoom.orthographicSize + deltaMagnitudeDiff * zoomSpeed, minZoom, maxZoom);

            // pinchGestureEvent?.Invoke(targetOrthographicSize);

            _zoomPercent = (maxZoom - targetOrthographicSize) / (maxZoom - minZoom);

            zoomSlider.interactable = false;

            _isZoomByPinching = true;

            // if (Mathf.Abs(_zoomPercent - _lastZoomPercent) < changeThreshold)
            // {
            //     return;
            // }

            // zoomSlider.value = _zoomPercent;
            // updateZoomSliderEvent?.Invoke(targetOrthographicSize / maxZoom);
        }
        
        if (_isZoomByPinching)
        {
            if (!Mathf.Approximately(zoomSlider.value, _zoomPercent))
            {
                zoomSlider.value = Mathf.Lerp(zoomSlider.value, _zoomPercent, 1f / 4);
            }
            else
            {
                zoomSlider.interactable = true;

                _isZoomByPinching = false;
                isPinchingEvent?.Invoke(false); 
            }
        }
    }


    #endregion
    private void OnLevelStarted()
    {
        _isDisabled = false;

        zoomSlider.value = 0.0f;
    }

    #region Public Methods

    public void ToggleSlider(bool isActive)
    {
        // DebugLogger.Log(message:$"isActive: {isActive}", context:this);
        zoomSlider.gameObject.SetActive(isActive);
    }

    #endregion

    #region Slider Interaction Methods

    private void OnZoomSliderValueChanged(float value)
    {
        if (_isDisabled)
        {
            return;
        }

        pinchGestureEvent?.Invoke(maxZoom - value * (maxZoom - minZoom));

        /*if (!_isZoomByPinching)
        {
            DebugLogger.Log(message:$"active pinch disable swipe");
            enableSwipingEvent?.Invoke(false);
            isPinchingEvent?.Invoke(true); 
        }

        CommonUtil.StopAllTweens(_tweens);

        _tweens.Add(Tween.Delay(delayToEnableSwipeGesture).OnComplete(() =>
        {
            DebugLogger.Log(message:$"active pinch enable swipe");
            enableSwipingEvent?.Invoke(true);
            isPinchingEvent?.Invoke(false); 
        }));*/

        _lastZoomPercent = value;
    }
    
    private void OnSliderPointerDown()
    {
        if (_isDisabled) return;
    
        isPinchingEvent?.Invoke(true);
        enableSwipingEvent?.Invoke(false);
    }

    private void OnSliderPointerUp()
    {
        if (_isDisabled) return;

        CommonUtil.StopAllTweens(_tweens);

        _tweens.Add(Tween.Delay(delayToEnableSwipeGesture).OnComplete(() =>
        {
            enableSwipingEvent?.Invoke(true);
            isPinchingEvent?.Invoke(false);
        }));
    }

    #endregion

    private void SetNewCameraOrthographicSize(float orthographicSize)
    {
        float unit = 0.5f * orthographicSize;

        if (_autoZoomTween.isAlive)
        {
            _autoZoomTween.Stop();
        }

        _autoZoomTween = Tween.Custom(zoomSlider.value, 0.5f, duration: 0.15f, onValueChange: newVal =>
        {
            zoomSlider.value = newVal;

            maxZoom = orthographicSize + 1.0f * zoomSlider.value * unit;
            minZoom = orthographicSize - 1.0f * (1 - zoomSlider.value) * unit;
        });
    }

    private void EnablePinching(bool isEnable)
    {
        _isDisabled = !isEnable;
    }
}
