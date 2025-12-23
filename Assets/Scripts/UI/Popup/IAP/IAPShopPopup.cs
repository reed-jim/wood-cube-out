using System;
using NamPhuThuy.Common;
using NamPhuThuy.UI;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static GameEnum;

public class IAPShopPopup : BasePopup
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private CanvasGroup popupCanvasGroup;
    [SerializeField] private RectTransform scrollContent;
    [SerializeField] private RectTransform firstPackRT;
    [SerializeField] private RectTransform firstCoinPackRT;

    [Header("CUSTOMIZE")]
    [SerializeField] private float scrollToCoinPackDuration;

    public static event Action<ScreenRoute> switchRouteEvent;
    public static event Action reviveRefusedEvent;
    public static event Action fetchLocalizedPriceIAPEvent;

    protected override void RegisterMoreEvent()
    {
        
        GalleryPackElement.openIAPShopPopupEvent += ShowAndScrollToCoinPack;
        
        ResourceEarnPopup.showResourcesEarnPopupEvent += OnResourcesEarnPopupShown;
    }

    protected override void UnregisterMoreEvent()
    {
        
        GalleryPackElement.openIAPShopPopupEvent -= ShowAndScrollToCoinPack;
        
        ResourceEarnPopup.showResourcesEarnPopupEvent -= OnResourcesEarnPopupShown;
    }

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);

        fetchLocalizedPriceIAPEvent?.Invoke();

        GamePersistentVariable.IAPLocation = "IAP Shop Gameplay";
    }

    protected override void Hide()
    {
        base.Hide();
        /*if (GamePersistentVariable.isReviveState)
        {
            // reviveRefusedEvent?.Invoke();
            SwitchRoute(ScreenRoute.Revive, false);
        }*/
        
        DebugLogger.Log(message: $"route to switch back: {prevRoute}", context: this);
        if (prevRoute != ScreenRoute.None)
        {
            SwitchRoute(prevRoute, false);
            prevRoute = ScreenRoute.None;
        }
    }

    private void ShowAndScrollToCoinPack(ScreenRoute prevRoute)
    {
        scrollRect.vertical = false;

        Show(onCompletedAction: () =>
        {
            float distance = 0.9f * (firstPackRT.localPosition.y - firstCoinPackRT.localPosition.y);

            _tweens.Add(Tween.LocalPositionY(scrollContent, distance, duration: scrollToCoinPackDuration)
            .OnComplete(() =>
            {
                scrollRect.vertical = true;
            }));
        });

        this.prevRoute = prevRoute;
    }


    private void OnResourcesEarnPopupShown(bool isShow)
    {
        float start = 0;
        float end = 1;

        if (isShow)
        {
            start = 1;
            end = 0;
        }

        _tweens.Add(Tween.Custom(start, end, duration: 0.3f, onValueChange: newVal =>
        {
            popupCanvasGroup.alpha = newVal;
        }));
    }
}
