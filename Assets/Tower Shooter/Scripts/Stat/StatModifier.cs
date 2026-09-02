using System;
using UnityEngine;

public enum StatType
{
    Health,
    MaxHealth,
    Damage,         // Primary offense stat
    AttackSpeed,    // Attacks per second
    Armor,          // Damage mitigation
    MoveSpeed,      // Base movement speed
    AbilityHaste,   // Cooldown reduction calculation
    CritChance      // 0.0 to 1.0 (0% to 100%)
}

public enum ModifierType
{
    Flat = 100,
    PercentAdd = 200,   // Added together: (Base * (1 + SumOfPercentAdd))
    PercentMult = 300   // Applied sequentially: Value * Mult1 * Mult2
}

[Serializable]
public class StatModifier
{
    public readonly StatType Type;
    public readonly ModifierType ModType;
    public readonly float Value;
    public readonly int UpgradeTime;
    public readonly object Source; // Identifies origin (e.g., AugmentSO, ItemInstance)

    public StatModifier(StatType type, ModifierType modType, float value, int upgradeTime = 0, object source = null)
    {
        Type = type;
        ModType = modType;
        Value = value;
        UpgradeTime = upgradeTime;
        Source = source;
    }

    public static float GetUpgradedValue(int upgradeTime)
    {
        return 0.1f * (1 + Mathf.Log(upgradeTime));
    }

    public static float GetCoinRequired(int upgradeTime)
    {
        return 1 + Mathf.Pow(upgradeTime, 2);
    }
}
