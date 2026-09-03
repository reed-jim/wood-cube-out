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

    void Awake()
    {
        closeButton.onClick.AddListener(Close);
    }

    public void Init(BaseCharacter node)
    {
        this.node = node;

        container.SetParent(node.transform);
        container.localPosition = new Vector3(0, -container.sizeDelta.y);

        SaferioTween.ShowWithPunch(container, 1);

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
        StatUpgradeData upgradeData = UpgradeManager.Ins.GetStatUpgradeData(node, statType);

        UIElementsOfStats[statType].statText.text = $"{upgradeData.currentValue:F2} --> <color=#008502>{upgradeData.nextValue:F2}</color>";

        UIElementsOfStats[statType].costText.text = $"{(int)upgradeData.coinRequired}";
    }

    public void Unlock()
    {

    }

    public void Close()
    {
        SaferioTween.ScaleDownWithPunch(container, 1);
    }
}

[Serializable]
public class UIElementsOfStat
{
    public TMP_Text statText;
    public TMP_Text costText;
    public Button upgradeButton;
}
