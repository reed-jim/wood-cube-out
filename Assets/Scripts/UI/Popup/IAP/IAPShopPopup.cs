using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static GameEnum;

public class IAPShopPopup : BasePopup
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform scrollContent;
    [SerializeField] private RectTransform firstPackRT;
    [SerializeField] private RectTransform firstCoinPackRT;

    [Header("CUSTOMIZE")]
    [SerializeField] private float scrollToCoinPackDuration;

    #region PRIVATE FIELD
    private ScreenRoute _prevRoute;
    #endregion

    public static event Action<ScreenRoute> switchRouteEvent;

    protected override void RegisterMoreEvent()
    {
        BuyBoosterPopup.openIAPShopPopupEvent += ShowAndScrollToCoinPack;
        RevivePopup.openIAPShopPopupEvent += ShowAndScrollToCoinPack;
    }

    protected override void UnregisterMoreEvent()
    {
        BuyBoosterPopup.openIAPShopPopupEvent -= ShowAndScrollToCoinPack;
        RevivePopup.openIAPShopPopupEvent -= ShowAndScrollToCoinPack;
    }

    private void ShowAndScrollToCoinPack(ScreenRoute prevRoute)
    {
        scrollRect.vertical = false;

        Show(onCompletedAction: () =>
        {
            float distance = 1.06f * (firstPackRT.localPosition.y - firstCoinPackRT.localPosition.y);

            _tweens.Add(Tween.LocalPositionY(scrollContent, distance, duration: scrollToCoinPackDuration)
            .OnComplete(() =>
            {
                scrollRect.vertical = true;
            }));
        });

        _prevRoute = prevRoute;
    }

    protected override void Hide()
    {
        base.Hide(onCompletedAction: () =>
        {
            switchRouteEvent?.Invoke(_prevRoute);
        });
    }
}
