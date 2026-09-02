using System.Linq;
using UnityEngine;

public class UpgradeManager : Singleton<UpgradeManager>
{
    public BaseCharacter player;

    public void Upgrade(StatType statType, out float nextCoinRequired)
    {
        int upgradeTime = 0;
        float newValue = 0;

        if (player.upgrades.Any(item => item.Type == statType))
        {
            StatModifier oldStatModifier = player.upgrades.First(item => item.Type == statType);

            upgradeTime = oldStatModifier.UpgradeTime + 1;
            newValue = StatModifier.GetUpgradedValue(upgradeTime);

            StatModifier newStatModifier = new StatModifier(oldStatModifier.Type, oldStatModifier.ModType, newValue, upgradeTime);

            RemoveUpgrade(oldStatModifier);
            ApplyUpgrade(newStatModifier);

            nextCoinRequired = StatModifier.GetCoinRequired(upgradeTime);
        }
        else
        {
            upgradeTime = 1;
            newValue = StatModifier.GetUpgradedValue(upgradeTime);

            StatModifier statModifier = new StatModifier(statType, ModifierType.PercentAdd, newValue, upgradeTime);

            ApplyUpgrade(statModifier);

            nextCoinRequired = StatModifier.GetCoinRequired(upgradeTime);
        }
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
}
