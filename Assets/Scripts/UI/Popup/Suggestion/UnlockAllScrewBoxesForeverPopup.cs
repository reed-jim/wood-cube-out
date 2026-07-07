using System;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnlockAllScrewBoxesForeverPopup : BasePopup
{
    [SerializeField] private TMP_Text localizedPriceText;
    [SerializeField] private RectTransform[] resourceGroups;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button noThanksButton;

    [Header("CUSTOMIZE")]
    [SerializeField] private IAPPackageData packageData;

    public static event Action<string> buyIAPEvent;
    public static event Action requestFetchIAPPrice;

    protected override void MoreActionInAwake()
    {

    }

    protected override void RegisterMoreEvent()
    {
        SaferioIAPManager.updatePriceEvent += FetchLocalizedPrice;
        ResourceEarnPopup.showResourcesEarnPopupEvent += OnResourceEarnPopupShow;

        buyButton.onClick.AddListener(Buy);
        noThanksButton.onClick.AddListener(Hide);
    }

    protected override void UnregisterMoreEvent()
    {
        SaferioIAPManager.updatePriceEvent -= FetchLocalizedPrice;
        ResourceEarnPopup.showResourcesEarnPopupEvent -= OnResourceEarnPopupShow;
    }

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);

        requestFetchIAPPrice?.Invoke();
    }

    private void Buy()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Hide();

            ShowNotification(GameConstants.NO_INTERNET, GameConstants.PLEASE_CHECK_YOUR_INTERNET_CONNECTION);
        }
        else
        {
            GamePersistentVariable.IAPLocation = "Unlock All Screw Boxes Forever";

            GamePersistentVariable.lastPurchasedPackPosition = buyButton.transform.position;
            buyIAPEvent?.Invoke(packageData.ProductId);

            buyButton.interactable = false;

            _tweens.Add(Tween.Delay(1f).OnComplete(() =>
            {
                buyButton.interactable = true;
            }));
        }
    }

    private void FetchLocalizedPrice(string productId, string localizedPrice)
    {
        if (productId == packageData.ProductId)
        {
            localizedPriceText.text = $"{localizedPrice}";
        }
    }

    private void OnResourceEarnPopupShow(bool isShow)
    {
        if (isShow)
        {
            Hide();
        }
    }
}
