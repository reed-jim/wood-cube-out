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

    public static event Action fetchLocalizedPriceIAPEvent;

    protected override void RegisterMoreEvent()
    {
        BuyBoosterPopup.openIAPShopPopupEvent += ShowAndScrollToCoinPack;
        RevivePopup.openIAPShopPopupEvent += ShowAndScrollToCoinPack;
        LivesShopPopup.openIAPShopPopupEvent += ShowAndScrollToCoinPack;
        LosePopup.openIAPShopPopupEvent += ShowAndScrollToCoinPack;
        ResourceEarnPopup.showResourcesEarnPopupEvent += OnResourcesEarnPopupShown;
        ResourceEarnPopup.removeAdsEvent += RebuildLayout;
        SaferioIAPManager.removeAdsEvent += RebuildLayout;
    }

    protected override void UnregisterMoreEvent()
    {
        BuyBoosterPopup.openIAPShopPopupEvent -= ShowAndScrollToCoinPack;
        RevivePopup.openIAPShopPopupEvent -= ShowAndScrollToCoinPack;
        LivesShopPopup.openIAPShopPopupEvent -= ShowAndScrollToCoinPack;
        LosePopup.openIAPShopPopupEvent -= ShowAndScrollToCoinPack;
        ResourceEarnPopup.showResourcesEarnPopupEvent -= OnResourcesEarnPopupShown;
        ResourceEarnPopup.removeAdsEvent -= RebuildLayout;
        SaferioIAPManager.removeAdsEvent -= RebuildLayout;
    }

    private void RebuildLayout()
    {
        if (scrollContent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollContent);
        }
    }

    protected override void Show(Action onCompletedAction = null)
    {
        if (scrollRect != null)
        {
            scrollRect.StopMovement();
            scrollRect.verticalNormalizedPosition = 1f;
        }

        if (scrollContent != null)
        {
            Vector3 pos = scrollContent.localPosition;
            pos.y = 0f;
            scrollContent.localPosition = pos;
        }

        base.Show(onCompletedAction);

        fetchLocalizedPriceIAPEvent?.Invoke();

        GamePersistentVariable.IAPLocation = "IAP Shop Gameplay";
    }

    private void ShowAndScrollToCoinPack(ScreenRoute prevRoute)
    {
        scrollRect.vertical = false;

        Show(onCompletedAction: () =>
        {
            if (scrollContent != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(scrollContent);
            }

            float distance = 0.9f * (firstPackRT.localPosition.y - firstCoinPackRT.localPosition.y);

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
            if (_prevRoute != ScreenRoute.Waiting)
            {
                SwitchRoute(_prevRoute);
                _prevRoute = ScreenRoute.Waiting;
            }
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
            //popupCanvasGroup.alpha = newVal;
        }));
    }
}
