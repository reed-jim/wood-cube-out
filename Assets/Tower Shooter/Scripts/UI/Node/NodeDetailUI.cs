using System;
using System.Collections.Generic;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodeDetailUI : BasePopup
{
    public TMP_Text nameText;
    public TMP_Text noAugmentText;

    public SerializableDictionary<StatType, UIElementsOfStat> UIElementsOfStats;

    public Button unlockButton;

    public BaseCharacter node;

    [Header("Current Augments")]
    public RectTransform currentAugmentContainer;
    public List<NodeDetailAugmentItemUI> nodeDetailAugmentItemUIs = new List<NodeDetailAugmentItemUI>();

    [Header("Pending Augments")]
    public RectTransform pendingAugmentContainer;
    public List<PendingAugmentItemUI> pendingAugmentItemUIs = new List<PendingAugmentItemUI>();

    protected override void MoreActionInAwake()
    {
        base.MoreActionInAwake();

        unlockButton.onClick.AddListener(Unlock);
    }

    protected override void RegisterMoreEvent()
    {
        base.RegisterMoreEvent();

        NodeManager.showNodeDetailEvent += ShowNodeDetail;
    }

    protected override void UnregisterMoreEvent()
    {
        base.UnregisterMoreEvent();

        NodeManager.showNodeDetailEvent -= ShowNodeDetail;
    }

    public void ShowNodeDetail(BaseCharacter node)
    {
        this.node = node;

        // container.position = node.serviceLocator.nodeUI.container.transform.position;

        Show();

        nameText.text = node.name;

        foreach (var statType in UIElementsOfStats.Keys)
        {
            UpdateStatText(node, statType);

            UIElementsOfStats[statType].upgradeButton.onClick.RemoveAllListeners();
            UIElementsOfStats[statType].upgradeButton.onClick.AddListener(() => UpgradeStat(statType));
        }

        InitCurrentAugments();
        InitPendingAugments();
    }

    public void UpgradeStat(StatType statType)
    {
        UpgradeManager.Ins.Upgrade(node, statType);

        if (statType == StatType.Damage)
        {
            node.serviceLocator.nodeUI.UpdatePower();
        }

        UpdateStatText(node, statType);
    }

    public void UpdateStatText(BaseCharacter node, StatType statType)
    {
        // StatUpgradeData upgradeData = UpgradeManager.Ins.GetStatUpgradeData(node, statType);

        // UIElementsOfStats[statType].statText.text = $"{upgradeData.currentValue:F2} --> <color=#008502>{upgradeData.nextValue:F2}</color>";
        // UIElementsOfStats[statType].costText.text = $"{(int)upgradeData.coinRequired}";

        Stat stat = node.serviceLocator.stats.GetStat(statType);

        float baseValue = stat.BaseValue;
        float currentValue = stat.Value;

        Color color = ThemeManager.PrimaryText;

        if (currentValue > baseValue)
        {
            color = Color.green;
        }
        else if (currentValue < baseValue)
        {
            color = Color.red;
        }

        UIElementsOfStats[statType].statText.text = $"{statType}: <color={ColorUtil.ColorToHex(color)}>{node.serviceLocator.stats.GetStat(statType).Value}";
    }

    public void Unlock()
    {
        ResourceManager.Ins.ChangeResource(ResourceType.UpgradePoint, -1);

        node.Unlock();
    }

    #region Current Augments
    public void InitCurrentAugments()
    {
        if (node.ownedAugments.Count == 0)
        {
            noAugmentText.gameObject.SetActive(true);

            return;
        }
        else
        {
            noAugmentText.gameObject.SetActive(false);
        }

        for (int i = 0; i < nodeDetailAugmentItemUIs.Count; i++)
        {
            NodeDetailAugmentItemUI nodeDetailAugmentItemUI = nodeDetailAugmentItemUIs[i];

            ObjectPoolingEverything.ReturnToPool(GameConstants.nodeDetailAugmentItemUI, nodeDetailAugmentItemUI.gameObject);
        }

        nodeDetailAugmentItemUIs.Clear();

        for (int i = 0; i < node.ownedAugments.Count; i++)
        {
            NodeDetailAugmentItemUI nodeDetailAugmentItemUI = ObjectPoolingEverything.GetFromPool<NodeDetailAugmentItemUI>(GameConstants.nodeDetailAugmentItemUI);

            nodeDetailAugmentItemUI.transform.SetParent(currentAugmentContainer);
            nodeDetailAugmentItemUI.transform.localScale = Vector3.one;

            nodeDetailAugmentItemUI.Init(node.ownedAugments[i], expectedWidth: 720);

            nodeDetailAugmentItemUIs.Add(nodeDetailAugmentItemUI);
        }
    }
    #endregion

    #region Pending Augments
    public void InitPendingAugments()
    {
        if (AugmentManager.Ins.pendingAugments.Count == 0)
        {
            pendingAugmentContainer.gameObject.SetActive(false);

            return;
        }
        else
        {
            pendingAugmentContainer.gameObject.SetActive(true);
        }

        for (int i = 0; i < pendingAugmentItemUIs.Count; i++)
        {
            PendingAugmentItemUI pendingAugmentItemUI = pendingAugmentItemUIs[i];

            ObjectPoolingEverything.ReturnToPool(GameConstants.pendingAugmentItemUI, pendingAugmentItemUI.gameObject);
        }

        pendingAugmentItemUIs.Clear();

        for (int i = 0; i < AugmentManager.Ins.pendingAugments.Count; i++)
        {
            PendingAugmentItemUI pendingAugmentItemUI = ObjectPoolingEverything.GetFromPool<PendingAugmentItemUI>(GameConstants.pendingAugmentItemUI);

            pendingAugmentItemUI.transform.SetParent(pendingAugmentContainer);
            pendingAugmentItemUI.transform.localScale = Vector3.one;

            pendingAugmentItemUI.Init(AugmentManager.Ins.pendingAugments[i], node, onApply: Hide);

            pendingAugmentItemUIs.Add(pendingAugmentItemUI);
        }
    }
    #endregion
}

[Serializable]
public class UIElementsOfStat
{
    public TMP_Text statText;
    public TMP_Text costText;
    public Button upgradeButton;
}
