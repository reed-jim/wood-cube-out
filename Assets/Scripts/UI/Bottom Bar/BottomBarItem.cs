using System;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class BottomBarItem : MonoBehaviour
{
    [SerializeField] private RectTransform container;
    [SerializeField] private RectTransform iconRT;
    [SerializeField] private RectTransform textRT;

    [SerializeField] private Button selectButton;

    [Header("CUSTOMIZE")]
    [SerializeField] private ScreenRoute route;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private Vector2Variable canvasSize;

    #region ACTION
    public static event Action<float> moveBottomBarHighlightEvent;
    public static event Action<ScreenRoute> switchRouteEvent;
    #endregion

    #region PRIVATE FIELD
    private float _slotSize;
    private Vector3 _iconInitialPosition;
    #endregion

    private void Awake()
    {
        BottomBar.setHighlightPositionEvent += OnSwipe;

        selectButton.onClick.AddListener(Select);

        _slotSize = canvasSize.Value.x / 5f;

        _iconInitialPosition = iconRT.localPosition;
    }

    private void OnDestroy()
    {
        BottomBar.setHighlightPositionEvent -= OnSwipe;
    }

    private void OnSwipe(float highlightPosition)
    {
        float ratio = 0;

        if (highlightPosition * container.localPosition.x < 0)
        {

        }
        else
        {
            ratio = Mathf.Abs(highlightPosition - container.localPosition.x) / _slotSize;

            if (ratio <= 1)
            {
                ratio = 1 - ratio;
            }
            else
            {
                ratio = 0;
            }
        }

        if (ratio <= 1)
        {
            iconRT.localPosition = new Vector3(_iconInitialPosition.x, _iconInitialPosition.y + ratio * 0.6f * _slotSize, 0);
            textRT.localScale = ratio * Vector3.one;
        }

        // if (ratio == 1)
        // {
        //     switchRouteEvent?.Invoke(route);
        // }
    }

    private void Select()
    {
        switchRouteEvent?.Invoke(route);
        moveBottomBarHighlightEvent?.Invoke(container.localPosition.x);
    }
}
