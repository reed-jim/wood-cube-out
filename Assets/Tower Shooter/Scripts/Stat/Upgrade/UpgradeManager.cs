using System.Linq;
using UnityEngine;

public class UpgradeManager : Singleton<UpgradeManager>
{
    public BaseCharacter player;

    public void Upgrade(StatType statType, out int nextCoinRequired)
    {
        if (player.upgrades.Any(item => item.Type == statType))
        {
            StatModifier oldStatModifier = player.upgrades.First(item => item.Type == statType);

            StatModifier newStatModifier = new StatModifier(oldStatModifier.Type, oldStatModifier.ModType, oldStatModifier.Value + 1, oldStatModifier.UpgradeTime + 1);

            RemoveUpgrade(oldStatModifier);
            ApplyUpgrade(newStatModifier);

            nextCoinRequired = newStatModifier.UpgradeTime;
        }
        else
        {
            StatModifier statModifier = new StatModifier(statType, ModifierType.Flat, 1, 1);

            player.upgrades.Add(statModifier);

            ApplyUpgrade(statModifier);

            nextCoinRequired = statModifier.UpgradeTime;
        }
    }

    public void ApplyUpgrade(StatModifier statModifier)
    {
        player.stats.AddModifier(statModifier);
    }

    public void RemoveUpgrade(StatModifier statModifier)
    {
        player.stats.RemoveModifier(statModifier);
    }
}
