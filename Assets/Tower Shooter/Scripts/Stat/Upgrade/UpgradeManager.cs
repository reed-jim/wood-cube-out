using System;
using System.Linq;
using UnityEngine;

public class UpgradeManager : Singleton<UpgradeManager>
{
    public BaseCharacter player;

    [Header("Customize")]
    public float percentAddPerUpgrade;

    public static event Action<StatType> upgradeStatEvent;

    public StatUpgradeData Upgrade(BaseCharacter node, StatType statType)
    {
        int upgradeTime = 0;

        StatUpgradeData statUpgradeData = GetStatUpgradeData(node, statType);

        if (node.upgrades.Any(item => item.Type == statType))
        {
            StatModifier oldStatModifier = node.upgrades.First(item => item.Type == statType);

            upgradeTime = oldStatModifier.UpgradeTime + 1;

            StatModifier newStatModifier =
                new StatModifier(
                    oldStatModifier.Type, oldStatModifier.ModType, oldStatModifier.Value + statUpgradeData.deltaValue, upgradeTime, oldStatModifier.statModifierSource);

            RemoveUpgrade(node, oldStatModifier);
            ApplyUpgrade(node, newStatModifier);
        }
        else
        {
            upgradeTime = 1;

            StatModifier statModifier = new StatModifier(statType, ModifierType.Flat, statUpgradeData.deltaValue, upgradeTime, StatModifierSource.Upgrade);

            ApplyUpgrade(node, statModifier);
        }

        upgradeStatEvent?.Invoke(statType);

        return statUpgradeData;
    }

    public void ApplyUpgrade(BaseCharacter node, StatModifier statModifier)
    {
        node.stats.AddModifier(statModifier);

        node.upgrades.Add(statModifier);

        float power = node.stats.GetStat(StatType.Damage).Value;
        float speed = node.stats.GetStat(StatType.AttackSpeed).Value;
    }

    public void RemoveUpgrade(BaseCharacter node, StatModifier statModifier)
    {
        node.stats.RemoveModifier(statModifier);

        node.upgrades.Remove(statModifier);
    }

    public StatUpgradeData GetStatUpgradeData(BaseCharacter node, StatType statType)
    {
        int upgradeTime = 0;
        float currentValue = 0;
        float newValue = 0;
        float deltaValue = 0;
        float nextCoinRequired = 0;

        Stat stat = node.stats.GetStat(statType);

        float baseStatValue = stat.BaseValue;
        float currentStatValue = stat.Value;

        if (node.upgrades.Any(item => item.Type == statType))
        {
            StatModifier oldStatModifier = node.upgrades.First(item => item.Type == statType);

            upgradeTime = oldStatModifier.UpgradeTime + 1;
        }
        else
        {
            upgradeTime = 1;
        }

        currentValue = currentStatValue;

        // newValue = CurveManager.Ins.GetNextStatValue(statType, upgradeTime);
        // newValue = CommonUtil.RoundToDecimalPlaces(newValue, 3);
        // nextCoinRequired = CurveManager.Ins.GetUpgradeNodeCost(upgradeTime);
        // deltaValue = newValue - currentValue;

        deltaValue = CurveManager.Ins.GetNextStatValue(statType, upgradeTime);
        newValue = currentStatValue + deltaValue;
        nextCoinRequired = CurveManager.Ins.GetUpgradeNodeCost(upgradeTime);

        StatUpgradeData statUpgradeData = new StatUpgradeData
        {
            currentValue = currentValue,
            nextValue = newValue,
            deltaValue = deltaValue,
            coinRequired = nextCoinRequired
        };

        return statUpgradeData;
    }
}

public struct StatUpgradeData
{
    public float currentValue;
    public float nextValue;
    public float deltaValue;
    public float coinRequired;
}
