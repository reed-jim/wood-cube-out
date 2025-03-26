using System;
using PrimeTween;
using UnityEngine;

public class SwipingScreen : MonoBehaviour
{
    [SerializeField] private RectTransform container;
    [SerializeField] private UIScreen iapShopScreen;
    [SerializeField] private UIScreen leaderboardScreen;

    [SerializeField] private Vector2Variable canvasSize;
    [SerializeField] private Vector2Variable canvasSizeOfReferenceDevice;

    [Header("CUSTOMIZE")]
    [SerializeField] private int screenNumber;

    #region PRIVATE FIELD
    private float _slotSize;
    private bool _isSwitchingByTap;
    private bool _isEnabledSwiping;
    #endregion

    #region ACTION
    public static event Action<float> hideOutsideScreenEvent;
    #endregion

    private void Awake()
    {
        //SwipeGesture.swipeGestureEvent += OnSwipe;
        SwipeGesture.stopSwipeGestureEvent += OnStopSwiping;
        UIScreen.moveSwipingScreenEvent += MoveSwipingScreen;

        _isEnabledSwiping = true;
    }

    private void Start()
    {
        hideOutsideScreenEvent?.Invoke(container.localPosition.x);

        SetupUI();
    }

    private void OnDestroy()
    {
        //SwipeGesture.swipeGestureEvent -= OnSwipe;
        SwipeGesture.stopSwipeGestureEvent -= OnStopSwiping;
        UIScreen.moveSwipingScreenEvent -= MoveSwipingScreen;
    }

    private void SetupUI()
    {
        iapShopScreen.SetInitialPosition(new Vector3(-GamePersistentVariable.canvasSizeAfterCanvasScaler.x, 0, 0));
        leaderboardScreen.SetInitialPosition(new Vector3(GamePersistentVariable.canvasSizeAfterCanvasScaler.x, 0, 0));
    }

    private void OnSwipe(Vector2 direction)
    {
        if (_isSwitchingByTap || !_isEnabledSwiping)
        {
            return;
        }

        if (Mathf.Abs(container.localPosition.x + direction.x) <= 0.5f * screenNumber * GamePersistentVariable.canvasSizeAfterCanvasScaler.x)
        {
            container.localPosition += new Vector3(direction.x, 0, 0);
        }
    }

    private void OnStopSwiping()
    {
        if (_isSwitchingByTap || !_isEnabledSwiping)
        {
            return;
        }

        float ratio = (container.localPosition.x % GamePersistentVariable.canvasSizeAfterCanvasScaler.x) / GamePersistentVariable.canvasSizeAfterCanvasScaler.x;

        int factor = (int)(container.localPosition.x / GamePersistentVariable.canvasSizeAfterCanvasScaler.x);

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
        Tween.LocalPositionX(container, factor * GamePersistentVariable.canvasSizeAfterCanvasScaler.x, duration: 0.3f)
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

    private void EnableSwiping(bool isEnable)
    {
        _isEnabledSwiping = isEnable;
    }
}
