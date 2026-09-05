using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "New Augment", menuName = "Stats/Augment")]
public class Augment : ScriptableObject
{
    public string augmentName;
    [TextArea(2, 4)]
    [SerializeField] private string customNotesTemplate; // Optional extra flavor/lore text

    // Direct Stat Modifiers
    public List<StatModifierData> statModifiers = new List<StatModifierData>();

    // Complex / Dynamic Effects
    public List<AugmentEffect> dynamicEffects = new List<AugmentEffect>();

    public bool isNeedCharacterToApply;

    public virtual void Apply(CharacterStats characterStats)
    {
        if (isNeedCharacterToApply && !IsCharacterSuitable(characterStats.serviceLocator.character))
        {
            return;
        }

        foreach (var modData in statModifiers)
        {
            characterStats.AddModifier(new StatModifier(modData.statType, modData.modType, modData.value, 0, this));
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

    public virtual bool IsCharacterSuitable(BaseCharacter character)
    {
        return true;
    }

    public string BuildFullDescription(StatColorConfigSO config)
    {
        StringBuilder sb = new StringBuilder();

        // 1. Process basic stat modifiers automatically
        foreach (var mod in statModifiers)
        {
            string hex = config.GetHex(mod.statType);
            string valHex = config.GetValueHex();

            string sign = mod.value >= 0 ? "+" : "";
            string valStr = mod.modType == ModifierType.Flat
                ? $"{sign}{mod.value}"
                : $"{sign}{mod.value * 100f}%";

            sb.AppendLine($"<color=#{valHex}>{valStr}</color> <color=#{hex}>{mod.statType}</color>");
        }

        // 2. Process dynamic effects descriptions
        // foreach (var effect in dynamicEffects)
        // {
        //     string effectDesc = effect.GetFormattedDescription(config);
        //     if (!string.IsNullOrEmpty(effectDesc))
        //     {
        //         sb.AppendLine(effectDesc);
        //     }
        // }

        // 3. Process optional extra flavor text
        if (!string.IsNullOrEmpty(customNotesTemplate))
        {
            if (sb.Length > 0) sb.AppendLine();
            sb.Append(StatTextParser.ParseTemplate(customNotesTemplate, config));
        }

        return sb.ToString().TrimEnd();
    }
}

[System.Serializable]
public struct StatModifierData
{
    public StatType statType;
    public ModifierType modType;
    public float value;
}
