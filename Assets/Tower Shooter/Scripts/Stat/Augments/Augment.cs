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
    public int id;
    public string augmentName;
    [TextArea(2, 4)]
    public string customNotesTemplate; // Optional extra flavor/lore text

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

    public bool isUnlocked;

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
            float value = AugmentManager.Ins.GetAugmentEfficiency(this) * modData.value;

            characterStats.AddModifier(new StatModifier(modData.statType, modData.modType, value, 0));
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

            bool isFlat = mod.modType == ModifierType.Flat && !StatUtil.IsPercentStat(mod.statType);

            string valStr = isFlat
                ? $"{sign}{mod.value}"
                : $"{sign}{mod.value * 100f}%";

            // sb.AppendLine($"<color=#{valHex}>{valStr}</color> <color=#{hex}>{mod.statType}</color>");
            sb.AppendLine($"<size=130%><color=#{hex}>{valStr} {mod.statType}</color></size>");
        }

        if (dynamicEffects.Count > 0)
        {
            sb.AppendLine();
        }

        // 2. Process dynamic effects descriptions
        int index = 0;

        foreach (var effect in dynamicEffects)
        {
            string effectDesc = effect.GetFormattedDescription(config);
            if (!string.IsNullOrEmpty(effectDesc))
            {
                if (index > 0)
                {
                    sb.AppendLine();
                }

                sb.AppendLine(effectDesc);
            }

            index++;
        }

        // 3. Process optional extra flavor text
        // if (!string.IsNullOrEmpty(customNotesTemplate))
        // {
        //     if (sb.Length > 0) sb.AppendLine();
        //     sb.Append($"\"{StatTextParser.ParseTemplate(customNotesTemplate, config)}\"");
        // }

        return sb.ToString().TrimEnd();
    }

    public string BuildFullDescriptionTooltip(StatColorConfigSO config)
    {
        StringBuilder sb = new StringBuilder();

        // 1. Process basic stat modifiers automatically
        foreach (var mod in statModifiers)
        {
            string hex = config.GetHex(mod.statType);

            bool isPositive = mod.value >= 0;

            string sign = isPositive ? "+" : "";

            bool isFlat = mod.modType == ModifierType.Flat && !StatUtil.IsPercentStat(mod.statType);

            string valStr = isFlat
                ? $"{sign}{mod.value}"
                : $"{sign}{mod.value * 100f}%";

            sb.AppendLine($"<color=#{hex}>{valStr} {mod.statType}</color>");
        }

        if (dynamicEffects.Count > 0)
        {
            sb.AppendLine();
        }

        // 2. Process dynamic effects descriptions
        int index = 0;

        foreach (var effect in dynamicEffects)
        {
            string effectDesc = effect.GetFormattedDescription(config);
            if (!string.IsNullOrEmpty(effectDesc))
            {
                if (index > 0)
                {
                    sb.AppendLine();
                }

                sb.AppendLine(effectDesc);
            }

            index++;
        }

        return sb.ToString().TrimEnd();
    }

    public string BuildFullDescriptionForUpLevelAugment(StatColorConfigSO config, AugmentLevel level)
    {
        StringBuilder sb = new StringBuilder();

        string primaryHex = DynamicThemeManager.Ins.PrimaryHex;

        sb.AppendLine($"Lv. <color={primaryHex}>{level}</color>");

        sb.AppendLine();

        // 1. Process basic stat modifiers automatically
        GetStatModifiersDescriptionForUpLevelAugment(statModifiers, level, config, sb);

        if (dynamicEffects.Count > 0)
        {
            sb.AppendLine();
        }

        // 2. Process dynamic effects descriptions
        int index = 0;

        foreach (var effect in dynamicEffects)
        {
            string effectDesc = effect.GetFormattedDescription(config);
            if (!string.IsNullOrEmpty(effectDesc))
            {
                if (index > 0)
                {
                    sb.AppendLine();
                }

                sb.AppendLine(effectDesc);
            }

            index++;
        }

        // 3. Process optional extra flavor text
        // if (!string.IsNullOrEmpty(customNotesTemplate))
        // {
        //     if (sb.Length > 0) sb.AppendLine();
        //     sb.Append($"\"{StatTextParser.ParseTemplate(customNotesTemplate, config)}\"");
        // }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Processes basic stat modifiers automatically, scaling values based on the augment level.
    /// </summary>
    public string GetStatModifiersDescriptionForUpLevelAugment(List<StatModifierData> statModifiers, AugmentLevel level, StatColorConfigSO config, StringBuilder sb)
    {
        if (statModifiers == null || statModifiers.Count == 0) return string.Empty;

        // Calculate tier multiplier based on augment level progression
        float currentLevelMultiplier = CurveManager.Ins.GetAugmentEfficiency((int)level);
        float nextLevelMultiplier = CurveManager.Ins.GetAugmentEfficiency((int)level + 1);

        foreach (var mod in statModifiers)
        {
            string hex = config.GetHex(mod.statType);

            // Scale modifier value by the tier multiplier
            float currentValue = currentLevelMultiplier * mod.value;
            float nextValue = nextLevelMultiplier * mod.value;
            bool isPositive = nextValue >= 0;
            // string sign = isPositive ? "+" : "";

            bool isFlat = mod.modType == ModifierType.Flat && !StatUtil.IsPercentStat(mod.statType);

            string currentValStr = isFlat
                ? $"{currentValue:F1}"
                : $"{currentValue * 100f:F0}%";
            string nextValStr = isFlat
                ? $"{nextValue:F1}"
                : $"{nextValue * 100f:F0}%";

            sb.AppendLine($"{currentValStr} > <color=#{hex}>{nextValStr} {mod.statType}</color>");
        }

        return sb.ToString();
    }
}

[System.Serializable]
public struct StatModifierData
{
    public StatType statType;
    public ModifierType modType;
    public float value;
}
