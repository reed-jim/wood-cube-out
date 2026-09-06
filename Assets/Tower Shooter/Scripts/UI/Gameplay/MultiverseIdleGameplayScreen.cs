using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class MultiverseIdleGameplayScreen : Singleton<MultiverseIdleGameplayScreen>
{
    public Button openAugmentSelectionButton;
    public Button openUpgradeButton;

    [Header("Augment")]
    public TMP_Text buyAugmentCostText;
    public NotificationDot buyAugmentNotification;

    [Header("Resource")]
    public TMP_Text upgradePointText;

    public static event Action<ScreenRoute> switchRouteEvent;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        openAugmentSelectionButton.onClick.AddListener(OpenAugmentSelection);
        openUpgradeButton.onClick.AddListener(OpenUpgrade);
    }

    private void OpenAugmentSelection()
    {
        if (ResourceManager.Ins.GetResourceValue(ResourceType.Coin) >= AugmentManager.Ins.currentBuyAugmentCost)
        {
            switchRouteEvent?.Invoke(ScreenRoute.AugmentSelection);

            HideBuyAugmentNotification();
        }
        else
        {

        }
    }

    private void OpenUpgrade()
    {
        switchRouteEvent?.Invoke(ScreenRoute.Upgrade);
    }

    public void UpdateUpgradePointText(int upgradePoint)
    {
        upgradePointText.text = $"{upgradePoint}";
    }

    public void UpdateBuyAugmentCostText(float cost)
    {
        buyAugmentCostText.text = $"{cost:F2}";

        if (ResourceManager.Ins.GetResourceValue(ResourceType.Coin) >= cost)
        {
            NotifyBuyAugment();
        }
        else
        {
            HideBuyAugmentNotification();
        }
    }

    public void NotifyBuyAugment()
    {
        buyAugmentNotification.Notify();
    }

    public void HideBuyAugmentNotification()
    {
        buyAugmentNotification.Hide();
    }
}
