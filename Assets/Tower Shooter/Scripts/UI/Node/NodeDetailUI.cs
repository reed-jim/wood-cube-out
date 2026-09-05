using System;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodeDetailUI : MonoBehaviour
{
    public RectTransform container;

    public Button closeButton;

    public SerializableDictionary<StatType, UIElementsOfStat> UIElementsOfStats;

    public Button unlockButton;

    public BaseCharacter node;

    [Header("Animation")]
    public ScaleTransition scaleTransition;

    void Awake()
    {
        closeButton.onClick.AddListener(Close);
        unlockButton.onClick.AddListener(Unlock);
    }

    public void Init(BaseCharacter node)
    {
        this.node = node;

        container.position = node.serviceLocator.nodeUI.container.transform.position;

        Show();

        foreach (var statType in UIElementsOfStats.Keys)
        {
            UpdateStatText(node, statType);

            UIElementsOfStats[statType].upgradeButton.onClick.RemoveAllListeners();
            UIElementsOfStats[statType].upgradeButton.onClick.AddListener(() => UpgradeStat(statType));
        }
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
        if (ResourceManager.Ins.GetResourceValue(ResourceType.UpgradePoint) > 0)
        {
            ResourceManager.Ins.ChangeResource(ResourceType.UpgradePoint, -1);

            node.nodeData.isUnlocked = true;

            node.Init();

            Close();
        }
        else
        {

        }
    }

    public void Show()
    {
        scaleTransition.Show(onCompletedAction: () =>
        {

        });
    }

    public void Close()
    {
        scaleTransition.Hide();
    }
}

[Serializable]
public class UIElementsOfStat
{
    public TMP_Text statText;
    public TMP_Text costText;
    public Button upgradeButton;
}
