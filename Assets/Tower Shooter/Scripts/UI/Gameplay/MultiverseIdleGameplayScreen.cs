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

    [Header("Filter")]
    public Button normalFilterButton;
    public Button enableAugmentFilterButton;

    public static event Action<ScreenRoute> switchRouteEvent;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        openAugmentSelectionButton.onClick.AddListener(OpenAugmentSelection);
        openUpgradeButton.onClick.AddListener(OpenUpgrade);
        augmentOverviewButton.onClick.AddListener(OpenAugmentOverview);

        normalFilterButton.onClick.AddListener(EnableNormalFilter);
        enableAugmentFilterButton.onClick.AddListener(EnableAugmentFilter);
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
        buyAugmentCostText.text = $"Augment ({cost:F2})";

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
    public void ShowSelectAugmentUI(bool isSelectNode)
    {
        selectAugmentUISiblingIndex = selectAugmentUI.transform.GetSiblingIndex();
        nodeContainerSiblingIndex = nodeContainer.transform.GetSiblingIndex();

        selectAugmentUI.transform.SetAsLastSibling();
        nodeContainer.transform.SetAsLastSibling();

        selectAugmentUI.Show(isSelectNode);
    }

    public void HideSelectAugmentUI()
    {
        selectAugmentUI.transform.SetSiblingIndex(selectAugmentUISiblingIndex);
        nodeContainer.transform.SetSiblingIndex(nodeContainerSiblingIndex);

        selectAugmentUI.Hide();
    }
    #endregion

    #region Filter
    public void EnableAugmentFilter()
    {
        FilterManager.Ins.EnableAugmentFilter();

        FilterManager.filterMode = GameFilterMode.Augment;
    }

    public void EnableNormalFilter()
    {
        if (FilterManager.filterMode == GameFilterMode.Augment)
        {
            FilterManager.Ins.DisableAugmentFilter();

            FilterManager.filterMode = GameFilterMode.Normal;
        }
    }
    #endregion
}
