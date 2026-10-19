using UnityEngine;

public class RecycleModule : MonoBehaviour
{
    public CharacterServiceLocator serviceLocator;

    public void Recycle(double lastPowerGenerated)
    {
        double recyclePower = serviceLocator.character.nodeData.recycleModuleRate * lastPowerGenerated;

        CharacterStats characterStats = serviceLocator.stats;

        StatModifier oldStatModifier = characterStats.GetModifierFromSource(StatType.Damage, StatModifierSource.RecycleModule);

        StatModifier statModifier = new StatModifier(StatType.Damage, ModifierType.Flat, (float)recyclePower, statModifierSource: StatModifierSource.RecycleModule);

        if (oldStatModifier != null)
        {
            statModifier.Value += oldStatModifier.Value;

            characterStats.RemoveModifier(oldStatModifier);
        }

        characterStats.AddModifier(statModifier);

        PowerManager.Ins.GetTotalPowerOutputPerSecond();
    }
}
