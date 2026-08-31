using UnityEngine;

public enum ModifierType
{
    Flat,
    PercentAdd,
    PercentMultiply
}

public enum ModifierSource
{
    Character,
    Item,
    Augment,
    Buff,
    Debuff,
    Skill,
    Aura
}

public class StatModifier
{
    public StatType Stat;
    public ModifierType Type;
    public float Value;

    public ModifierSource Source;
    public object SourceObject;
}
