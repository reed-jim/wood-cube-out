using System;
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

    #region PRIVATE FIELD
    private ScreenRoute _prevRoute;
    #endregion

    public static event Action<ScreenRoute> switchRouteEvent;
    public static event Action fetchLocalizedPriceIAPEvent;

    protected override void RegisterMoreEvent()
    {
        BuyBoosterPopup.openIAPShopPopupEvent += ShowAndScrollToCoinPack;
        RevivePopup.openIAPShopPopupEvent += ShowAndScrollToCoinPack;
        LivesShopPopup.openIAPShopPopupEvent += ShowAndScrollToCoinPack;
        ResourceEarnPopup.showResourcesEarnPopupEvent += OnResourcesEarnPopupShown;
    }

    protected override void UnregisterMoreEvent()
    {
        BuyBoosterPopup.openIAPShopPopupEvent -= ShowAndScrollToCoinPack;
        RevivePopup.openIAPShopPopupEvent -= ShowAndScrollToCoinPack;
        LivesShopPopup.openIAPShopPopupEvent -= ShowAndScrollToCoinPack;
        ResourceEarnPopup.showResourcesEarnPopupEvent -= OnResourcesEarnPopupShown;
    }

    protected override void Show(Action onCompletedAction = null)
    {
        scrollRect.vertical = false;

        base.Show(onCompletedAction: () =>
        {
            ScrollToTop();
        });

        fetchLocalizedPriceIAPEvent?.Invoke();

        GamePersistentVariable.IAPLocation = "IAP Shop Gameplay";
    }

    private void ShowAndScrollToCoinPack(ScreenRoute prevRoute)
    {
        Show();

        // scrollRect.vertical = false;

        // Show(onCompletedAction: () =>
        // {
        //     Tween.Custom(scrollRect.verticalNormalizedPosition, 1, duration: 1f, onValueChange: newVal =>
        //     {
        //         scrollRect.verticalNormalizedPosition = newVal;
        //     })
        //     .OnComplete(() =>
        //     {
        //         scrollRect.vertical = true;
        //     });

        //     // float distance = 0.9f * (firstPackRT.localPosition.y - firstCoinPackRT.localPosition.y);

        //     // _tweens.Add(Tween.LocalPositionY(scrollContent, distance, duration: scrollToCoinPackDuration)
        //     // .OnComplete(() =>
        //     // {
        //     //     scrollRect.vertical = true;
        //     // }));
        // });

        _prevRoute = prevRoute;
    }

    private void ScrollToTop()
    {
        scrollRect.vertical = false;

        Tween.Custom(scrollRect.verticalNormalizedPosition, 1, duration: 0.6f, onValueChange: newVal =>
        {
            scrollRect.verticalNormalizedPosition = newVal;
        })
        .OnComplete(() =>
        {
            scrollRect.vertical = true;
        });
    }

    protected override void Hide()
    {
        base.Hide(onCompletedAction: () =>
        {
            switchRouteEvent?.Invoke(_prevRoute);
            // switchRouteChannel.Invoke(_prevRoute, nameof(_prevRoute));
        });
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
