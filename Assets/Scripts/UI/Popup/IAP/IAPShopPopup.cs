using PrimeTween;
using UnityEngine;

public class IAPShopPopup : BasePopup
{
    [SerializeField] private RectTransform scrollContent;
    [SerializeField] private RectTransform firstPackRT;
    [SerializeField] private RectTransform firstCoinPackRT;

    [Header("CUSTOMIZE")]
    [SerializeField] private float scrollToCoinPackDuration;

    protected override void RegisterMoreEvent()
    {
        BuyBoosterPopup.openIAPShopPopupEvent += ShowAndScrollToCoinPack;
    }

    protected override void UnregisterMoreEvent()
    {
        BuyBoosterPopup.openIAPShopPopupEvent -= ShowAndScrollToCoinPack;
    }

    private void ShowAndScrollToCoinPack()
    {
        Show(onCompletedAction: () =>
        {
            float distance = firstPackRT.localPosition.y - firstCoinPackRT.localPosition.y;

            _tweens.Add(Tween.LocalPositionY(scrollContent, distance, duration: scrollToCoinPackDuration));
        });
    }
}
