using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Augment", menuName = "Stats/Augment")]
public class Augment : ScriptableObject
{
    public string augmentName;
    [TextArea] public string description;

    // Direct Stat Modifiers
    public List<StatModifierData> statModifiers = new List<StatModifierData>();

    // Complex / Dynamic Effects
    public List<AugmentEffect> dynamicEffects = new List<AugmentEffect>();

    public virtual void Apply(CharacterStats characterStats)
    {
        foreach (var modData in statModifiers)
        {
            characterStats.AddModifier(new StatModifier(modData.statType, modData.modType, modData.value, this));
        }

        foreach (var effect in dynamicEffects)
        {
            effect.ApplyEffect(characterStats, this);
        }
    }

    public virtual void Remove(CharacterStats characterStats)
    {
        characterStats.RemoveSource(this);

        foreach (var effect in dynamicEffects)
        {
            effect.RemoveEffect(characterStats, this);
        }
    }
}

[System.Serializable]
public struct StatModifierData
{
    public StatType statType;
    public ModifierType modType;
    public float value;
}
