using Unity.Collections;
using Unity.Mathematics;

// Enum matching array indices for O(1) access
public enum StatType
{
    AttackDamage = 0,
    Armor = 1,
    AttackSpeed = 2,
    MoveSpeed = 3,
    Count = 4
}

// Blittable struct stored in NativeArray
public struct StatBlock
{
    public float Base;
    public float FlatAdd;
    public float PercentAdd;
    public float PercentMulti;
    public float FinalValue;

    public readonly float Calculate()
    {
        return ((Base + FlatAdd) * (1.0f + PercentAdd)) * PercentMulti;
    }
}

public enum ModifierType
{
    Flat = 0,
    PercentAdd = 1,
    PercentMulti = 2
}

public struct StatModifier
{
    public StatType Stat;
    public ModifierType Type;
    public float Value;
    public int SourceID; // ID of the Augment/Item for easy removal
}
