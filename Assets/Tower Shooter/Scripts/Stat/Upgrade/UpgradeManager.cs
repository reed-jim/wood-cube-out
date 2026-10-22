using System;
using System.Linq;
using UnityEngine;

public class UpgradeManager : Singleton<UpgradeManager>
{
    public static event Action<StatType> upgradeStatEvent;

    public StatUpgradeData Upgrade(BaseCharacter node, StatType statType)
    {
        StatUpgradeData statUpgradeData = GetStatUpgradeData(node, statType);

        return Upgrade(node, statType, statUpgradeData);
    }

    public StatUpgradeData Upgrade(BaseCharacter node, StatType statType, StatUpgradeData statUpgradeData)
    {
        int upgradeTime = 0;

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

        node.serviceLocator.statUpgradeEffectController.TryTriggerUpgrade();

        return statUpgradeData;
    }

    public void ApplyUpgrade(BaseCharacter node, StatModifier statModifier)
    {
        node.stats.AddModifier(statModifier);

        node.upgrades.Add(statModifier);

        double power = node.stats.GetStat(StatType.Damage).Value;
        double speed = node.stats.GetStat(StatType.AttackSpeed).Value;
    }

    public void RemoveUpgrade(BaseCharacter node, StatModifier statModifier)
    {
        node.stats.RemoveModifier(statModifier);

        node.upgrades.Remove(statModifier);
    }

    public StatUpgradeData GetStatUpgradeData(BaseCharacter node, StatType statType)
    {
        int upgradeTime = 0;
        double currentValue = 0;
        double newValue = 0;
        double deltaValue = 0;
        double nextCoinRequired = 0;

        Stat stat = node.stats.GetStat(statType);

        double baseStatValue = stat.BaseValue;
        double currentStatValue = stat.Value;

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

    public StatUpgradeData GetStatUpgradeDataMax(BaseCharacter node, StatType statType, double currentCoin)
    {
        Stat stat = node.stats.GetStat(statType);
        double currentValue = stat.Value;

        int currentUpgradeTime = 0;
        if (node.upgrades.Any(item => item.Type == statType))
        {
            StatModifier oldStatModifier = node.upgrades.First(item => item.Type == statType);
            currentUpgradeTime = oldStatModifier.UpgradeTime;
        }

        int startUpgradeTime = currentUpgradeTime + 1;

        // Check if player can even afford the first upgrade
        double firstCost = CurveManager.Ins.GetUpgradeNodeCost(startUpgradeTime);
        if (currentCoin < firstCost)
        {
            return new StatUpgradeData
            {
                currentValue = currentValue,
                nextValue = currentValue,
                deltaValue = 0,
                coinRequired = 0
            };
        }

        // Binary search to find the maximum levels affordable without a heavy linear loop
        int low = 1;
        int high = 100000; // Adjust this upper bound safely based on your game's max level limits
        int maxLevels = 0;
        double totalCost = 0;
        double totalDelta = 0;

        // Fast binary search to find the highest valid level count player can afford
        while (low <= high)
        {
            int mid = low + (high - low) / 2;
            double costSum = 0;
            bool affordable = true;

            // Calculate total cost for 'mid' levels from startUpgradeTime
            // (Note: If CurveManager has a cumulative cost formula, you can replace this inner loop with a direct math formula)
            for (int i = 0; i < mid; i++)
            {
                costSum += CurveManager.Ins.GetUpgradeNodeCost(startUpgradeTime + i);
                if (costSum > currentCoin)
                {
                    affordable = false;
                    break;
                }
            }

            if (affordable)
            {
                maxLevels = mid;
                low = mid + 1; // Try buying more
            }
            else
            {
                high = mid - 1; // Too expensive, try fewer
            }
        }

        // Calculate exact total cost and total stat delta for the verified maxLevels
        for (int i = 0; i < maxLevels; i++)
        {
            totalCost += CurveManager.Ins.GetUpgradeNodeCost(startUpgradeTime + i);
            totalDelta += CurveManager.Ins.GetNextStatValue(statType, startUpgradeTime + i);
        }

        double newValue = currentValue + totalDelta;

        return new StatUpgradeData
        {
            currentValue = currentValue,
            nextValue = newValue,
            deltaValue = totalDelta,
            coinRequired = (float)totalCost
        };
    }
}

public struct StatUpgradeData
{
    public double currentValue;
    public double nextValue;
    public double deltaValue;
    public double coinRequired;
}
