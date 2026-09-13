using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class MultiverseIdleGameplayScreen : Singleton<MultiverseIdleGameplayScreen>
{
    public Button openAugmentSelectionButton;
    public Button openUpgradeButton;
    public Button augmentOverviewButton;

    [Header("Node")]
    public RectTransform nodeContainer;

    [Header("Augment")]
    public TMP_Text buyAugmentCostText;
    public NotificationDot buyAugmentNotification;

    [Header("Resource")]
    public TMP_Text upgradePointText;

    [Header("Select Augment UI")]
    public SelectAugmentUI selectAugmentUI;
    private int selectAugmentUISiblingIndex;
    private int nodeContainerSiblingIndex;

    public static event Action<ScreenRoute> switchRouteEvent;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        openAugmentSelectionButton.onClick.AddListener(OpenAugmentSelection);
        openUpgradeButton.onClick.AddListener(OpenUpgrade);
        augmentOverviewButton.onClick.AddListener(OpenAugmentOverview);
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

    private void OpenAugmentOverview()
    {
        switchRouteEvent?.Invoke(ScreenRoute.AugmentOverview);
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

    #region SelectAugmentUI
    public void ShowSelectAugmentUI()
    {
        selectAugmentUISiblingIndex = selectAugmentUI.transform.GetSiblingIndex();
        nodeContainerSiblingIndex = nodeContainer.transform.GetSiblingIndex();

        selectAugmentUI.transform.SetAsLastSibling();
        nodeContainer.transform.SetAsLastSibling();

        selectAugmentUI.Show();
    }
    #endregion
}
