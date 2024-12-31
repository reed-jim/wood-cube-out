using System;
using PrimeTween;
using UnityEngine;

public class BottomBar : MonoBehaviour
{
    [SerializeField] private RectTransform highlight;

    [SerializeField] private Vector2Variable canvasSize;

    [Header("CUSTOMIZE")]
    [SerializeField] private int screenNumber;

    #region PRIVATE FIELD
    private float _slotSize;
    private bool _isSwitchingByTap;
    #endregion

    #region ACTION
    public static event Action<float> setHighlightPositionEvent;
    #endregion

    private void Awake()
    {
        SwipeGesture.swipeGestureEvent += OnSwipe;
        SwipeGesture.stopSwipeGestureEvent += OnStopSwiping;
        BottomBarItem.moveBottomBarHighlightEvent += MoveBottomBarHighlight;

        _slotSize = canvasSize.Value.x / screenNumber;
    }

    private void Start()
    {
        setHighlightPositionEvent?.Invoke(highlight.localPosition.x);
    }

    private void OnDestroy()
    {
        SwipeGesture.swipeGestureEvent -= OnSwipe;
        SwipeGesture.stopSwipeGestureEvent -= OnStopSwiping;
        BottomBarItem.moveBottomBarHighlightEvent -= MoveBottomBarHighlight;
    }

    private void OnSwipe(Vector2 direction)
    {
        if (_isSwitchingByTap)
        {
            return;
        }

        if (Mathf.Abs(highlight.localPosition.x - direction.x / screenNumber) <= 0.5f * (canvasSize.Value.x - highlight.sizeDelta.x))
        {
            highlight.localPosition -= new Vector3(direction.x / screenNumber, 0, 0);

            setHighlightPositionEvent?.Invoke(highlight.localPosition.x);
        }
    }

    private void OnStopSwiping()
    {
        if (_isSwitchingByTap)
        {
            return;
        }

        float unitValue = canvasSize.Value.x / screenNumber;

        float ratio = (highlight.localPosition.x % unitValue) / unitValue;

        int factor = (int)(highlight.localPosition.x / unitValue);

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

        Tween.StopAll(highlight);
        Tween.Custom(highlight.localPosition.x, factor * unitValue, duration: 0.3f, onValueChange: newVal =>
        {
            highlight.localPosition = new Vector3(newVal, highlight.localPosition.y, 0);

            setHighlightPositionEvent?.Invoke(highlight.localPosition.x);
        });
    }

    private void MoveBottomBarHighlight(float positionX)
    {
        float duration = 0.3f * Mathf.Abs(positionX - highlight.localPosition.x) / _slotSize;

        duration = 0.3f;

        Tween.Custom(highlight.localPosition.x, positionX, duration: duration, onValueChange: newVal =>
        {
            highlight.localPosition = new Vector3(newVal, highlight.localPosition.y, 0);

            setHighlightPositionEvent?.Invoke(highlight.localPosition.x);
        })
        .OnComplete(() =>
        {
            _isSwitchingByTap = false;
        });

        _isSwitchingByTap = true;
    }
}
