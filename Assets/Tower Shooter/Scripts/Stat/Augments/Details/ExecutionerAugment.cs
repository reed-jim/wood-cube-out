using UnityEngine;

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Augments/Executioner")]
public class ExecutionerAugment : Augment, IDynamicStatModifier
{
    [SerializeField]
    private float healthThreshold = 0.3f;

    [SerializeField]
    private float damageBonus = 0.3f;

    public void CollectModifiers(
        CharacterStats stats,
        StatType statType,
        List<StatModifier> modifiers)
    {
        if (statType != StatType.Damage)
            return;

        GameObject target = stats.CurrentTarget;

        if (target == null)
            return;

        Health targetHealth = target.GetComponent<Health>();

        if (targetHealth == null)
            return;

        float healthPercent =
            targetHealth.CurrentHealth / targetHealth.MaxHealth;

        if (healthPercent <= healthThreshold)
        {
            modifiers.Add(new StatModifier
            {
                Stat = StatType.Damage,
                Type = ModifierType.PercentAdd,
                Value = damageBonus,
                Source = ModifierSource.Augment,
                SourceObject = this
            });
        }
    }
}

