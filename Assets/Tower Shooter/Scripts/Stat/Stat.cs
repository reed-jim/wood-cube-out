using System;
using System.Collections.Generic;
using UnityEngine;

public enum StatType
{
    Health,
    Damage,
    AttackSpeed,
    Armor,
    MagicResist,
    MoveSpeed,
    CritChance,
    CritDamage,
    AbilityHaste
}

public class Stat
{
    public float BaseValue { get; private set; }

    private readonly List<StatModifier> modifiers = new();

    public Stat(float baseValue)
    {
        BaseValue = baseValue;
    }

    public void AddModifier(StatModifier modifier)
    {
        modifiers.Add(modifier);
    }

    public void RemoveModifier(StatModifier modifier)
    {
        modifiers.Remove(modifier);
    }

    public float Calculate(List<StatModifier> dynamicModifiers)
    {
        float flat = 0f;
        float percentAdd = 0f;
        float multiplier = 1f;

        foreach (var modifier in modifiers)
        {
            ApplyModifier(
                modifier,
                ref flat,
                ref percentAdd,
                ref multiplier
            );
        }

        foreach (var modifier in dynamicModifiers)
        {
            ApplyModifier(
                modifier,
                ref flat,
                ref percentAdd,
                ref multiplier
            );
        }

        return (BaseValue + flat)
             * (1f + percentAdd)
             * multiplier;
    }

    private void ApplyModifier(
        StatModifier modifier,
        ref float flat,
        ref float percentAdd,
        ref float multiplier)
    {
        switch (modifier.Type)
        {
            case ModifierType.Flat:
                flat += modifier.Value;
                break;

            case ModifierType.PercentAdd:
                percentAdd += modifier.Value;
                break;

            case ModifierType.PercentMultiply:
                multiplier *= modifier.Value;
                break;
        }
    }
}

[Serializable]
public struct StatValue
{
    public StatType type;
    public float value;
}
