using System;
using UnityEngine;

public class PinchGesture : MonoBehaviour
{
    public Camera cameraToZoom;
    public float zoomSpeed = 0.1f;
    public float minZoom = 10f;
    public float maxZoom = 100f;

    private bool _isDisabled;

    #region ACTION
    public static event Action<float> pinchGestureEvent;
    #endregion

    private void Awake()
    {
        LevelLoader.startLevelEvent += OnLevelStarted;
        BasePopup.enableSwipingScreenEvent += EnablePinching;
        LevelTutorial.enablePinchingEvent += EnablePinching;

        minZoom = 0.85f * cameraToZoom.orthographicSize;
        maxZoom = 1.15f * cameraToZoom.orthographicSize;


    }

    void OnDestroy()
    {
        LevelLoader.startLevelEvent -= OnLevelStarted;
        BasePopup.enableSwipingScreenEvent -= EnablePinching;
        LevelTutorial.enablePinchingEvent -= EnablePinching;
    }

    private void Update()
    {
        if (_isDisabled)
        {
            return;
        }

        if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
            Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;

            float prevTouchDeltaMag = (touch1PrevPos - touch2PrevPos).magnitude;
            float touchDeltaMag = (touch1.position - touch2.position).magnitude;

            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            pinchGestureEvent?.Invoke(Mathf.Clamp(cameraToZoom.orthographicSize + deltaMagnitudeDiff * zoomSpeed, minZoom, maxZoom));
        }
    }

    private void OnLevelStarted()
    {
        pinchGestureEvent?.Invoke(minZoom);
    }

    private void EnablePinching(bool isEnable)
    {
        _isDisabled = !isEnable;
    }
}
