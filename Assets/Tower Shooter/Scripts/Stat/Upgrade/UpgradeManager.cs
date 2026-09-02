using System.Linq;
using UnityEngine;

public class UpgradeManager : Singleton<UpgradeManager>
{
    public BaseCharacter player;

    public void Upgrade(StatType statType)
    {
        if (player.upgrades.Any(item => item.Type == statType))
        {
            StatModifier oldStatModifier = player.upgrades.First(item => item.Type == statType);

            StatModifier newStatModifier = new StatModifier(oldStatModifier.Type, oldStatModifier.ModType, oldStatModifier.Value + 1);

            RemoveUpgrade(oldStatModifier);
            ApplyUpgrade(newStatModifier);
        }
        else
        {
            StatModifier statModifier = new StatModifier(statType, ModifierType.Flat, 1);

            player.upgrades.Add(statModifier);

            ApplyUpgrade(statModifier);
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
