using System;
using PrimeTween;
using UnityEngine;

public class SwipingScreen : MonoBehaviour
{
    [SerializeField] private RectTransform container;

    [SerializeField] private Vector2Variable canvasSize;

    [Header("CUSTOMIZE")]
    [SerializeField] private int screenNumber;

    #region PRIVATE FIELD
    private float _slotSize;
    private bool _isSwitchingByTap;
    #endregion

    #region ACTION
    public static event Action<float> hideOutsideScreenEvent;
    #endregion

    private void Awake()
    {
        SwipeGesture.swipeGestureEvent += OnSwipe;
        SwipeGesture.stopSwipeGestureEvent += OnStopSwiping;
        UIScreen.moveSwipingScreenEvent += MoveSwipingScreen;
    }

    private void Start()
    {
        hideOutsideScreenEvent?.Invoke(container.localPosition.x);
    }

    private void OnDestroy()
    {
        SwipeGesture.swipeGestureEvent -= OnSwipe;
        SwipeGesture.stopSwipeGestureEvent -= OnStopSwiping;
        UIScreen.moveSwipingScreenEvent -= MoveSwipingScreen;
    }

    private void OnSwipe(Vector2 direction)
    {
        if (_isSwitchingByTap)
        {
            return;
        }

        if (Mathf.Abs(container.localPosition.x + direction.x) <= 0.5f * screenNumber * canvasSize.Value.x)
        {
            container.localPosition += new Vector3(direction.x, 0, 0);
        }
    }

    private void OnStopSwiping()
    {
        if (_isSwitchingByTap)
        {
            return;
        }

        float ratio = (container.localPosition.x % canvasSize.Value.x) / canvasSize.Value.x;

        int factor = (int)(container.localPosition.x / canvasSize.Value.x);

        if (Mathf.Abs(ratio) >= 0.5f)
        {
            if (ratio > 0)
            {
                factor++;
            }
            else
            {
                factor--;
            }
        }

        Tween.StopAll(container);
        Tween.LocalPositionX(container, factor * canvasSize.Value.x, duration: 0.3f)
        .OnComplete(() =>
        {
            hideOutsideScreenEvent?.Invoke(container.localPosition.x);
        });
    }

    private void MoveSwipingScreen(float positionX)
    {
        Tween.StopAll(container);
        Tween.LocalPositionX(container, -positionX, duration: 0.3f)
        .OnComplete(() =>
        {
            hideOutsideScreenEvent?.Invoke(container.localPosition.x);

            _isSwitchingByTap = false;
        });

        _isSwitchingByTap = true;
    }
}
