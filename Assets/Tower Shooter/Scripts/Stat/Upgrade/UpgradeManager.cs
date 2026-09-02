using System.Linq;
using UnityEngine;

public class UpgradeManager : Singleton<UpgradeManager>
{
    public BaseCharacter player;

    public StatUpgradeData Upgrade(StatType statType)
    {
        int upgradeTime = 0;

        StatUpgradeData statUpgradeData = GetStatUpgradeData(statType);

        if (player.upgrades.Any(item => item.Type == statType))
        {
            StatModifier oldStatModifier = player.upgrades.First(item => item.Type == statType);

            upgradeTime = oldStatModifier.UpgradeTime + 1;

            StatModifier newStatModifier = new StatModifier(oldStatModifier.Type, oldStatModifier.ModType, statUpgradeData.percentAdd, upgradeTime);

            RemoveUpgrade(oldStatModifier);
            ApplyUpgrade(newStatModifier);
        }
        else
        {
            upgradeTime = 1;

            StatModifier statModifier = new StatModifier(statType, ModifierType.PercentAdd, statUpgradeData.percentAdd, upgradeTime);

            ApplyUpgrade(statModifier);
        }

        return statUpgradeData;
    }

    public void ApplyUpgrade(StatModifier statModifier)
    {
        player.stats.AddModifier(statModifier);

        player.upgrades.Add(statModifier);
    }

    public void RemoveUpgrade(StatModifier statModifier)
    {
        player.stats.RemoveModifier(statModifier);

        player.upgrades.Remove(statModifier);
    }

    public StatUpgradeData GetStatUpgradeData(StatType statType)
    {
        int upgradeTime = 0;
        float percentAdd = 0;
        float currentValue = 0;
        float newValue = 0;
        float nextCoinRequired = 0;

        float baseStatValue = player.stats.GetStat(statType).BaseValue;
        float currentStatValue = player.stats.GetStat(statType).Value;

        if (player.upgrades.Any(item => item.Type == statType))
        {
            StatModifier oldStatModifier = player.upgrades.First(item => item.Type == statType);

            upgradeTime = oldStatModifier.UpgradeTime + 1;
            currentValue = currentStatValue;
            percentAdd = CurveManager.Ins.GetStatPercentAdd(upgradeTime);
            newValue = baseStatValue * (1 + percentAdd);
            nextCoinRequired = StatModifier.GetCoinRequired(upgradeTime);
        }
        else
        {
            upgradeTime = 1;
            currentValue = baseStatValue;
            percentAdd = CurveManager.Ins.GetStatPercentAdd(upgradeTime);
            newValue = baseStatValue * (1 + percentAdd);
            nextCoinRequired = StatModifier.GetCoinRequired(upgradeTime);
        }

        StatUpgradeData statUpgradeData = new StatUpgradeData
        {
            percentAdd = percentAdd,
            currentValue = currentValue,
            nextValue = newValue,
            coinRequired = nextCoinRequired
        };

        return statUpgradeData;
    }
}

public struct StatUpgradeData
{
    public float percentAdd;
    public float currentValue;
    public float nextValue;
    public float coinRequired;
}
