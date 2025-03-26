using System;
using System.Collections.Generic;
using Saferio.Util.SaferioTween;
using UnityEngine;
using UnityEngine.EventSystems;

public class SwipeGesture : MonoBehaviour
{
    [Header("CUSTOMIZE")]
    [SerializeField] private float threshold;

    private Vector2 _moveDirection;

    private Vector2 _lastPosition;
    private bool _isLastPositionExist;
    private bool _isDisabled;
    private bool _isSwipingLastFrame;
    private float acceleratedMultiplier;
    private float _notSwipingDuration;

    #region ACTION
    public static event Action stopSwipeGestureEvent;
    #endregion

    void Awake()
    {
        
        PinchGesture.enableSwipingEvent += EnableSwiping;

        acceleratedMultiplier = 1;
    }

    void OnDestroy()
    {
        PinchGesture.enableSwipingEvent -= EnableSwiping;
    }

    private void Update()
    {
        if (Input.GetMouseButton(0) && Input.touchCount == 1 && !_isDisabled)
        {
            // if (IsClickOnUI())
            // {
            //     return;
            // }

            Vector2 mousePosition = Input.mousePosition;

            if (!_isLastPositionExist)
            {
                _lastPosition = Input.mousePosition;

                _isLastPositionExist = true;
            }
            else
            {
                _moveDirection = mousePosition - _lastPosition;

                // _moveDirection *= acceleratedMultiplier;

                if (_moveDirection.magnitude < threshold)
                {
                    return;
                }
                

                _lastPosition = mousePosition;

                _isSwipingLastFrame = true;

                // acceleratedMultiplier += 0.01f;
            }
        }
        else
        {
            if (_isSwipingLastFrame)
            {
                stopSwipeGestureEvent?.Invoke();

                _isSwipingLastFrame = false;
            }
            _isLastPositionExist = false;
        }
    }

    private void EnableSwiping(bool isEnable)
    {
        _isDisabled = !isEnable;
    }

    private void TemporaryDisableSwipe(float value)
    {
        _isDisabled = true;

        SaferioTween.DelayAsync(1f, onCompletedAction: () =>
        {
            _isDisabled = false;
        });
    }

    private bool IsClickOnUI()
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var raycastResults = new List<RaycastResult>();

        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

        if (raycastResults.Count > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
