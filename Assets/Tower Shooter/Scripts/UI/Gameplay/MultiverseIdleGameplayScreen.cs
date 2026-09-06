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

    public void UpdateBuyAugmentCostText(int cost)
    {
        buyAugmentCostText.text = $"{cost}";
    }
}
