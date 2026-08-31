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

    public virtual void Apply(GameObject target)
    {
        var stats = target.GetComponent<CharacterStats>();
        if (stats == null) return;

        foreach (var modData in statModifiers)
        {
            stats.AddModifier(new StatModifier(modData.statType, modData.modType, modData.value, this));
        }

        foreach (var effect in dynamicEffects)
        {
            effect.ApplyEffect(target, this);
        }
    }

    public virtual void Remove(GameObject target)
    {
        var stats = target.GetComponent<CharacterStats>();
        if (stats == null) return;

        stats.RemoveSource(this);

        foreach (var effect in dynamicEffects)
        {
            effect.RemoveEffect(target, this);
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
