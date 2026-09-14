using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum AugmentQuality
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
    Mythic
}

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

    [Header("Customize")]
    public AugmentQuality augmentQuality;
    public SpaceObjectType spaceObjectType;
    public bool isNeedCharacterToApply = true;
    public bool isApplyImmediatelyAfterPick = true;
    public bool isForNodeGroup;

    public virtual void Pick()
    {

    }

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
            effect.SetAugment(characterStats, this);
        }

        if (!characterStats.serviceLocator.character.ownedAugments.Contains(this))
        {
            characterStats.serviceLocator.character.ownedAugments.Add(this);
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
            // string valHex;

            bool isPositive = mod.value >= 0;

            string sign = isPositive ? "+" : "";

            // if (isPositive)
            // {
            //     valHex = ColorUtil.ColorToHex(DynamicThemeManager.Ins.CurrentTheme.success);
            // }
            // else
            // {
            //     valHex = ColorUtil.ColorToHex(DynamicThemeManager.Ins.CurrentTheme.error);
            // }

            string valStr = mod.modType == ModifierType.Flat
                ? $"{sign}{mod.value}"
                : $"{sign}{mod.value * 100f}%";

            // sb.AppendLine($"<color=#{valHex}>{valStr}</color> <color=#{hex}>{mod.statType}</color>");
            sb.AppendLine($"<color=#{hex}>{valStr} {mod.statType}</color>");
        }

        // 2. Process dynamic effects descriptions
        foreach (var effect in dynamicEffects)
        {
            string effectDesc = effect.GetFormattedDescription(config);
            if (!string.IsNullOrEmpty(effectDesc))
            {
                sb.AppendLine(effectDesc);
            }
        }

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
