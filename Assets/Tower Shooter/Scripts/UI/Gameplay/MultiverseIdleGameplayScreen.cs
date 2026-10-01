using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class MultiverseIdleGameplayScreen : Singleton<MultiverseIdleGameplayScreen>
{
    public Button openAugmentSelectionButton;
    public Button augmentOverviewButton;
    public Button selectThemeButton;
    public Button settingButton;
    public Button leadershipButton;
    public Button ingameEventButton;

    [Header("Node")]
    public RectTransform nodeContainer;

    [Header("Augment")]
    public TMP_Text buyAugmentCostText;
    public NotificationDot buyAugmentNotification;
    public ThemeableUI augmentSelectionContainerThemeable;
    public ThemeableUI augmentSelectionIconThemeable;
    public ThemeableUI augmentSelectionTextThemeable;

    [Header("Select Augment UI")]
    public SelectAugmentUI selectAugmentUI;
    private int selectAugmentUISiblingIndex;
    private int nodeContainerSiblingIndex;

    [Header("Filter")]
    public Button normalFilterButton;
    public Button augmentFilterButton;
    public ThemeableUI normalFilterThemeable;
    public ThemeableUI augmentFilterThemeable;

    [Header("Task")]
    public NotificationDot taskNotification;

    public static event Action<ScreenRoute> switchRouteEvent;
    public static event Action openLeadershipUIEvent;
    public static event Action openIngameEventScreenEvent;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        BaseTask.taskCompleteEvent += OnTaskCompleted;
        ResourceManager.changeResourceValueEvent += OnResourceValueChanged;

        openAugmentSelectionButton.onClick.AddListener(OpenAugmentSelection);
        augmentOverviewButton.onClick.AddListener(OpenAugmentOverview);
        selectThemeButton.onClick.AddListener(OpenSelectThemePopup);
        settingButton.onClick.AddListener(OpenSettingPopup);
        // leadershipButton.onClick.AddListener(OpenLeadershipPopup);
        // ingameEventButton.onClick.AddListener(OpenIngameEventPopup);

        normalFilterButton.onClick.AddListener(EnableNormalFilter);
        augmentFilterButton.onClick.AddListener(EnableAugmentFilter);
    }

    void OnDestroy()
    {
        BaseTask.taskCompleteEvent -= OnTaskCompleted;
        ResourceManager.changeResourceValueEvent -= OnResourceValueChanged;
    }

    private void OpenAugmentSelection()
    {
        switchRouteEvent?.Invoke(ScreenRoute.AugmentSelection);

        // if (ResourceManager.Ins.GetResourceValue(ResourceType.AugmentPoint) > 0)
        // {
        //     switchRouteEvent?.Invoke(ScreenRoute.AugmentSelection);

        //     HideBuyAugmentNotification();

        //     ResourceManager.Ins.ChangeResource(ResourceType.AugmentPoint, -1);
        // }
        // else
        // {

        // }
    }

    private void OpenUpgrade()
    {
        switchRouteEvent?.Invoke(ScreenRoute.Upgrade);
    }

    private void OpenAugmentOverview()
    {
        switchRouteEvent?.Invoke(ScreenRoute.AugmentOverview);
    }

    private void OpenSelectThemePopup()
    {
        switchRouteEvent?.Invoke(ScreenRoute.SelectTheme);
    }

    private void OpenSettingPopup()
    {
        switchRouteEvent?.Invoke(ScreenRoute.Setting);
    }

    private void OpenLeadershipPopup()
    {
        openLeadershipUIEvent?.Invoke();
    }

    private void OpenIngameEventPopup()
    {
        openIngameEventScreenEvent?.Invoke();
    }

    public void UpdateUpgradePointText(int upgradePoint)
    {

    }

    public void UpdateBuyAugmentCostText(float cost)
    {
        // buyAugmentCostText.text = $"Augment ({cost:F2})";

        // if (ResourceManager.Ins.GetResourceValue(ResourceType.Coin) >= cost)
        // {
        //     NotifyBuyAugment();
        // }
        // else
        // {
        //     HideBuyAugmentNotification();
        // }
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

        selectAugmentUI.transform.SetAsLastSibling();

        selectAugmentUI.Show(isSelectNode);
    }

    public void HideSelectAugmentUI()
    {
        selectAugmentUI.transform.SetSiblingIndex(selectAugmentUISiblingIndex);
        nodeContainer.transform.SetSiblingIndex(nodeContainerSiblingIndex);

        selectAugmentUI.Hide();
    }

    public void SetSelectAugmentUIAsParent(Transform child)
    {
        child.SetParent(selectAugmentUI.transform);
    }

    private void HandleAugmentPointChanged(int point)
    {
        if (point > 0)
        {
            augmentSelectionContainerThemeable.ChangeRole(ThemeableUI.ElementRole.Primary);
            augmentSelectionIconThemeable.ChangeRole(ThemeableUI.ElementRole.OnPrimary);
            augmentSelectionTextThemeable.ChangeRole(ThemeableUI.ElementRole.OnPrimary);

            if (!buyAugmentNotification.gameObject.activeSelf)
            {
                NotifyBuyAugment();
            }
        }
        else
        {
            augmentSelectionContainerThemeable.ChangeRole(ThemeableUI.ElementRole.Secondary);
            augmentSelectionIconThemeable.ChangeRole(ThemeableUI.ElementRole.OnSecondary);
            augmentSelectionTextThemeable.ChangeRole(ThemeableUI.ElementRole.OnSecondary);

            HideBuyAugmentNotification();
        }

        if (point > 1)
        {
            buyAugmentCostText.text = $"{point} Augments left";
        }
        else
        {
            buyAugmentCostText.text = $"{point} Augment left";
        }
    }
    #endregion

    #region Filter
    public void EnableAugmentFilter()
    {
        FilterManager.Ins.EnableAugmentFilter();

        FilterManager.filterMode = GameFilterMode.Augment;

        normalFilterThemeable.ChangeRole(ThemeableUI.ElementRole.OnSurface);
        augmentFilterThemeable.ChangeRole(ThemeableUI.ElementRole.Primary);
    }

    public void EnableNormalFilter()
    {
        if (FilterManager.filterMode == GameFilterMode.Augment)
        {
            FilterManager.Ins.DisableAugmentFilter();

            FilterManager.filterMode = GameFilterMode.Normal;

            normalFilterThemeable.ChangeRole(ThemeableUI.ElementRole.Primary);
            augmentFilterThemeable.ChangeRole(ThemeableUI.ElementRole.OnSurface);
        }
    }
    #endregion

    #region Task
    private void OnTaskCompleted()
    {
        NotifyTask();
    }

    public void NotifyTask()
    {
        taskNotification.Notify();
    }

    public void HideTaskNotification()
    {
        taskNotification.Hide();
    }
    #endregion

    private void OnResourceValueChanged(ResourceType resourceType, float value)
    {
        if (resourceType == ResourceType.AugmentPoint)
        {
            HandleAugmentPointChanged((int)value);
        }
    }
}
