using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "GenericAugmentEffect", menuName = "Stats/Effects/GenericAugmentEffect")]
public class GenericAugmentEffect : AugmentEffect
{
    public GenericAugmentData config;

    public override void ApplyEffect(CharacterStats target, object source)
    {
        EnsureRuntime(target).Init(config);
    }

    public override void RemoveEffect(CharacterStats target, object source)
    {
        var runtime = target.GetComponent<GenericAugmentEffectRuntime>();
        if (runtime != null)
        {
            Destroy(runtime);
        }
    }

    public override void ApplyEffectOnCycleCompleted(CharacterStats target)
    {
        var runtime = EnsureRuntime(target);
        runtime.OnTriggerRaised(EffectTrigger.OnCycleComplete);
    }

    public override void SetAugment(CharacterStats target, Augment augment)
    {
        EnsureRuntime(target).SetAugment(augment);
    }

    public override void SetAugment(NodeGroup nodeGroup, Augment augment)
    {
        foreach (var node in nodeGroup.nodes)
        {
            if (node.serviceLocator != null && node.serviceLocator.stats != null)
            {
                EnsureRuntime(node.serviceLocator.stats).SetAugment(augment);
            }
        }
    }

    private GenericAugmentEffectRuntime EnsureRuntime(CharacterStats target)
    {
        var runtime = target.GetComponent<GenericAugmentEffectRuntime>();
        if (runtime == null)
        {
            runtime = target.gameObject.AddComponent<GenericAugmentEffectRuntime>();
            runtime.Init(config);
        }
        return runtime;
    }

    /// <summary>
    /// Overrides GetFormattedDescription to fallback to auto-generated text if no template is filled.
    /// </summary>
    public override string GetFormattedDescription(StatColorConfigSO configSO)
    {
        // If template exists, use the standard parser logic
        if (!string.IsNullOrEmpty(descriptionTemplate))
        {
            return base.GetFormattedDescription(configSO);
        }

        // Auto-Generate description if descriptionTemplate is empty
        if (config == null) return string.Empty;

        string autoText = GenerateAutoDescription();
        string effectColorHex = ColorUtil.ColorToHex(effectColor);

        return $"<color={effectColorHex}>{GenerateAutoEffectName(config)}:</color> {autoText}";
    }

    public override string GetFormattedDescriptionTooltip(StatColorConfigSO configSO)
    {
        // If template exists, use the standard parser logic
        if (!string.IsNullOrEmpty(descriptionTemplate))
        {
            return base.GetFormattedDescription(configSO);
        }

        // Auto-Generate description if descriptionTemplate is empty
        if (config == null) return string.Empty;

        string autoText = GenerateAutoDescriptionTooltip();
        string effectColorHex = ColorUtil.ColorToHex(effectColor);

        return $"<color={effectColorHex}>{GenerateAutoEffectName(config)}:</color> {autoText}";
    }

    public string GenerateAutoEffectName(GenericAugmentData config)
    {
        if (config == null) return "Unknown Effect";

        // 1. Prefix derived from trigger context or scope
        string prefix = config.scope switch
        {
            EffectScope.Global => "Global",
            EffectScope.AdjacentNodes => "Aura",
            _ => config.trigger switch
            {
                EffectTrigger.OnCrit => "Critical",
                EffectTrigger.WhenIdle => "Idle",
                EffectTrigger.EveryXCycles => "Cycling",
                EffectTrigger.WhenStatCrossesThreshold => "Surge",
                EffectTrigger.Permanently => "Enduring",
                _ => config.durationType switch
                {
                    EffectDuration.TemporaryBurst => "Burst",
                    EffectDuration.Stacking => "Stacking",
                    EffectDuration.Decaying => "Fading",
                    _ => ""
                }
            }
        };

        // 2. Action verb or adjective based on the mathematical operation
        string action = config.operation switch
        {
            EffectOperation.Add => "Boost",
            EffectOperation.Multiply => "Amplifier",
            EffectOperation.Convert => "Transmute",
            EffectOperation.Swap => "Exchange",
            EffectOperation.SquareRoot => "Focus",
            EffectOperation.Logarithm => "Scale",
            EffectOperation.Copy => "Echo",
            _ => "Effect"
        };

        // 3. Main noun identifying target stat
        string statName = config.targetStat switch
        {
            StatType.Damage => "Power",
            StatType.AttackSpeed => "Haste",
            StatType.CritChance => "Precision",
            StatType.CritMultiplier => "Ferocity",
            _ => config.targetStat.ToString()
        };

        // Build the name up to 3 words max
        if (string.IsNullOrEmpty(prefix))
        {
            return $"{statName} {action}";
        }

        return $"{prefix} {statName} {action}";
    }

    /// <summary>
    /// Constructs a readable sentence automatically based on GenericAugmentData configuration.
    /// </summary>
    private string GenerateAutoDescription()
    {
        StringBuilder sb = new StringBuilder();

        StatType sourceStat = config.sourceStat;
        StatType targetStat = config.targetStat;
        StatType comparisonStat = config.comparisonStat;

        string primaryHex = DynamicThemeManager.Ins.PrimaryHex;
        string sourceStatColorHex = AugmentManager.Ins.GetStatColorHex(sourceStat);
        string targetStatColorHex = AugmentManager.Ins.GetStatColorHex(targetStat);
        string comparisonStatColorHex = AugmentManager.Ins.GetStatColorHex(comparisonStat);

        string sourceStatWithColor = $"<color={sourceStatColorHex}>{sourceStat}</color>";
        string targetStatWithColor = $"<color={targetStatColorHex}>{targetStat}</color>";
        string comparisonStatWithColor = $"<color={comparisonStatColorHex}>{comparisonStat}</color>";

        string baseValueWithColor = $"<color={targetStatColorHex}>{config.baseValue}</color>";
        string thresholdValueWithColor = $"<color={comparisonStatColorHex}>{config.thresholdValue}</color>";

        string cycleFrequencyWithColor = $"<color={primaryHex}>{config.cycleFrequency}</color>";
        string durationInSecondsWithColor = $"<color={primaryHex}>{config.durationInSeconds}</color>";
        string maxStacksWithColor = $"<color={primaryHex}>{config.maxStacks}</color>";
        string decayRatePerSecondWithColor = $"<color={primaryHex}>{config.decayRatePerSecond}</color>";

        string scalingFactorWithColor = $"<color={sourceStatColorHex}>{config.scalingFactor}</color>";
        string scalingFactorPercentWithColor = $"<color={sourceStatColorHex}>{config.scalingFactor * 100f}%</color>";
        string scalingFunctionWithColor = $"<color={primaryHex}>{config.scalingFunction}</color>";
        string secondsSuffixWithColor = $"<color={primaryHex}>s</color>";
        string perSecondSuffixWithColor = $"<color={primaryHex}>/s</color>";

        // 1. TRIGGER
        switch (config.trigger)
        {
            case EffectTrigger.OnCycleComplete:
                sb.Append("On cycle complete, ");
                break;
            case EffectTrigger.OnCrit:
                sb.Append("On landing a critical hit, ");
                break;
            case EffectTrigger.EveryXCycles:
                sb.Append($"Every {cycleFrequencyWithColor} cycles, ");
                break;
            case EffectTrigger.WhenIdle:
                sb.Append("While idle, ");
                break;
            case EffectTrigger.WhenStatCrossesThreshold:
                sb.Append($"When {sourceStatWithColor} reaches {thresholdValueWithColor}, ");
                break;
            case EffectTrigger.Permanently:
                sb.Append("Permanently ");
                break;
        }

        // 2. OPERATION & TARGET STAT
        switch (config.operation)
        {
            case EffectOperation.Add:
                sb.Append($"increases {targetStatWithColor}");
                break;
            case EffectOperation.Multiply:
                sb.Append($"multiplies {targetStatWithColor}");
                break;
            case EffectOperation.Convert:
                sb.Append($"converts {sourceStatWithColor} into {targetStatWithColor}");
                break;
            case EffectOperation.Swap:
                sb.Append($"swaps {sourceStatWithColor} with {targetStatWithColor}");
                break;
            case EffectOperation.SquareRoot:
                sb.Append($"applies square root scaling to {targetStatWithColor}");
                break;
            case EffectOperation.Logarithm:
                sb.Append($"applies logarithmic scaling to {targetStatWithColor}");
                break;
            case EffectOperation.Copy:
                sb.Append($"copies {sourceStatWithColor} to {targetStatWithColor}");
                break;
        }

        // 3. MATH SCALING & VALUES
        if (config.operation != EffectOperation.Swap)
        {
            if (config.baseValue > 0)
            {
                sb.Append($" by {baseValueWithColor}");
            }

            if (config.scalingFactor != 0 && config.sourceStat != config.targetStat)
            {
                string scalingTypeStr = config.scalingFunction != ScalingFunction.Linear ? $" ({scalingFunctionWithColor})" : "";
                sb.Append($" + {scalingFactorPercentWithColor} of {sourceStatWithColor}{scalingTypeStr}");
            }
            else if (config.scalingFactor != 0 && config.scalingFunction != ScalingFunction.Linear)
            {
                sb.Append($" scaling {scalingFunctionWithColor}ly by factor {scalingFactorWithColor}");
            }
        }

        // 4. SCOPE
        switch (config.scope)
        {
            case EffectScope.AdjacentNodes:
                sb.Append(" for adjacent nodes");
                break;
            case EffectScope.Global:
                sb.Append(" globally for all nodes");
                break;
        }

        // 5. CONDITIONALS
        switch (config.condition)
        {
            case ConditionType.StatGreaterThanOther:
                sb.Append($" (Requires {sourceStatWithColor} > {comparisonStatWithColor})");
                break;
            case ConditionType.StatLessThanOther:
                sb.Append($" (Requires {sourceStatWithColor} < {comparisonStatWithColor})");
                break;
            case ConditionType.StatThresholdGate:
                sb.Append($" (Requires {sourceStatWithColor} >= {thresholdValueWithColor})");
                break;
        }

        // 6. DURATION & LIFETIME
        switch (config.durationType)
        {
            case EffectDuration.TemporaryBurst:
                sb.Append($" for {durationInSecondsWithColor}{secondsSuffixWithColor}");
                break;
            case EffectDuration.Stacking:
                sb.Append($" (Stacks up to {maxStacksWithColor}x)");
                break;
            case EffectDuration.Decaying:
                sb.Append($" (Decays by {decayRatePerSecondWithColor}{perSecondSuffixWithColor})");
                break;
        }

        sb.Append(".");

        // Capitalize first letter of sentence
        if (sb.Length > 0)
        {
            sb[0] = char.ToUpper(sb[0]);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Constructs a concise description focusing solely on the stat value change between levels (e.g., +10 -> +15).
    /// </summary>
    public string GenerateAutoDescriptionForUpLevelAugment(AugmentLevel level)
    {
        if (config == null) return string.Empty;

        // Calculate tier multipliers for the previous level and current level
        float prevTierMultiplier = CurveManager.Ins.GetAugmentEfficiency(level - 1);
        float tierMultiplier = CurveManager.Ins.GetAugmentEfficiency(level);

        // Compute values for both levels
        float prevScaledBaseValue = config.baseValue * prevTierMultiplier;
        float scaledBaseValue = config.baseValue * tierMultiplier;

        float prevScaledScalingFactor = config.scalingFactor * prevTierMultiplier;
        float scaledScalingFactor = config.scalingFactor * tierMultiplier;

        StringBuilder sb = new StringBuilder();

        StatType targetStat = config.targetStat;
        string targetStatColorHex = AugmentManager.Ins.GetStatColorHex(targetStat);
        string targetStatWithColor = $"<color={targetStatColorHex}>{targetStat}</color>";

        // Focus strictly on the primary stat value transition
        if (config.baseValue != 0)
        {
            string signPrev = prevScaledBaseValue >= 0 ? "+" : "";
            string signNext = scaledBaseValue >= 0 ? "+" : "";
            sb.Append($"{signPrev}{prevScaledBaseValue:F1} -> {signNext}{scaledBaseValue:F1} {targetStatWithColor}");
        }
        else if (config.scalingFactor != 0)
        {
            string signPrev = prevScaledScalingFactor >= 0 ? "+" : "";
            string signNext = scaledScalingFactor >= 0 ? "+" : "";
            sb.Append($"{signPrev}{prevScaledScalingFactor * 100f:F0}% -> {signNext}{scaledScalingFactor * 100f:F0}% {targetStatWithColor}");
        }
        else
        {
            sb.Append($"{targetStatWithColor} Upgraded");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Constructs a detailed multi-line breakdown automatically based on GenericAugmentData configuration 
    /// tailored for wide, short tooltip layouts.
    /// </summary>
    private string GenerateAutoDescriptionTooltip()
    {
        StringBuilder sb = new StringBuilder();

        StatType sourceStat = config.sourceStat;
        StatType targetStat = config.targetStat;
        StatType comparisonStat = config.comparisonStat;

        string primaryHex = DynamicThemeManager.Ins.PrimaryHex;
        string sourceStatColorHex = AugmentManager.Ins.GetStatColorHex(sourceStat);
        string targetStatColorHex = AugmentManager.Ins.GetStatColorHex(targetStat);
        string comparisonStatColorHex = AugmentManager.Ins.GetStatColorHex(comparisonStat);

        string sourceStatWithColor = $"<color={sourceStatColorHex}>{sourceStat}</color>";
        string targetStatWithColor = $"<color={targetStatColorHex}>{targetStat}</color>";
        string comparisonStatWithColor = $"<color={comparisonStatColorHex}>{comparisonStat}</color>";

        string baseValueWithColor = $"<color={targetStatColorHex}>{config.baseValue}</color>";
        string thresholdValueWithColor = $"<color={comparisonStatColorHex}>{config.thresholdValue}</color>";

        string cycleFrequencyWithColor = $"<color={primaryHex}>{config.cycleFrequency}</color>";
        string durationInSecondsWithColor = $"<color={primaryHex}>{config.durationInSeconds}</color>";
        string maxStacksWithColor = $"<color={primaryHex}>{config.maxStacks}</color>";
        string decayRatePerSecondWithColor = $"<color={primaryHex}>{config.decayRatePerSecond}</color>";

        string scalingFactorWithColor = $"<color={sourceStatColorHex}>{config.scalingFactor}</color>";
        string scalingFactorPercentWithColor = $"<color={sourceStatColorHex}>{config.scalingFactor * 100f}%</color>";
        string scalingFunctionWithColor = $"<color={primaryHex}>{config.scalingFunction}</color>";

        // --- SECTION 1: TRIGGER & SCOPE ---
        sb.Append($"<color={primaryHex}><b>Trigger:</b></color> ");
        switch (config.trigger)
        {
            case EffectTrigger.OnCycleComplete:
                sb.Append("On cycle complete");
                break;
            case EffectTrigger.OnCrit:
                sb.Append("On landing a critical hit");
                break;
            case EffectTrigger.EveryXCycles:
                sb.Append($"Every {cycleFrequencyWithColor} cycles");
                break;
            case EffectTrigger.WhenIdle:
                sb.Append("While idle");
                break;
            case EffectTrigger.WhenStatCrossesThreshold:
                sb.Append($"When {sourceStatWithColor} reaches {thresholdValueWithColor}");
                break;
            case EffectTrigger.Permanently:
                sb.Append("Permanent passive");
                break;
            default:
                sb.Append(config.trigger.ToString());
                break;
        }

        // Append Scope context if applicable
        switch (config.scope)
        {
            case EffectScope.AdjacentNodes:
                sb.Append(" <color=#888888>|</color> Scope: <color=#FFFFFF>Adjacent Nodes (Aura)</color>");
                break;
            case EffectScope.Global:
                sb.Append(" <color=#888888>|</color> Scope: <color=#FFFFFF>Global (All Nodes)</color>");
                break;
            default:
                sb.Append(" <color=#888888>|</color> Scope: <color=#FFFFFF>Self</color>");
                break;
        }

        sb.AppendLine();

        // --- SECTION 2: EFFECT & OPERATION ---
        sb.Append($"<color={primaryHex}><b>Effect:</b></color> ");
        switch (config.operation)
        {
            case EffectOperation.Add:
                sb.Append($"Increases {targetStatWithColor}");
                break;
            case EffectOperation.Multiply:
                sb.Append($"Multiplies {targetStatWithColor}");
                break;
            case EffectOperation.Convert:
                sb.Append($"Converts {sourceStatWithColor} into {targetStatWithColor}");
                break;
            case EffectOperation.Swap:
                sb.Append($"Swaps {sourceStatWithColor} with {targetStatWithColor}");
                break;
            case EffectOperation.SquareRoot:
                sb.Append($"Square-root scales {targetStatWithColor}");
                break;
            case EffectOperation.Logarithm:
                sb.Append($"Logarithm scales {targetStatWithColor}");
                break;
            case EffectOperation.Copy:
                sb.Append($"Copies {sourceStatWithColor} to {targetStatWithColor}");
                break;
        }

        // Values & Scaling math details
        if (config.operation != EffectOperation.Swap)
        {
            if (config.baseValue > 0)
            {
                sb.Append($" by {baseValueWithColor}");
            }

            if (config.scalingFactor != 0 && config.sourceStat != config.targetStat)
            {
                string scalingTypeStr = config.scalingFunction != ScalingFunction.Linear ? $" [{scalingFunctionWithColor}]" : "";
                sb.Append($" (+{scalingFactorPercentWithColor} of {sourceStatWithColor}{scalingTypeStr})");
            }
            else if (config.scalingFactor != 0 && config.scalingFunction != ScalingFunction.Linear)
            {
                sb.Append($" (Scaled {scalingFunctionWithColor}ly by {scalingFactorWithColor})");
            }
        }

        sb.AppendLine();

        // --- SECTION 3: CONDITIONS & LIFETIME (Optional details) ---
        bool hasCondition = config.condition != ConditionType.AlwaysActive;
        bool hasLifetime = config.durationType != EffectDuration.Permanent;

        if (hasCondition || hasLifetime)
        {
            sb.Append($"<color={primaryHex}><b>Conditions:</b></color> ");

            if (hasCondition)
            {
                switch (config.condition)
                {
                    case ConditionType.StatGreaterThanOther:
                        sb.Append($"Requires {sourceStatWithColor} > {comparisonStatWithColor} ");
                        break;
                    case ConditionType.StatLessThanOther:
                        sb.Append($"Requires {sourceStatWithColor} < {comparisonStatWithColor} ");
                        break;
                    case ConditionType.StatThresholdGate:
                        sb.Append($"Requires {sourceStatWithColor} $\\ge$ {thresholdValueWithColor} ");
                        break;
                }
            }

            if (hasLifetime)
            {
                if (hasCondition) sb.Append("<color=#888888>| </color>");
                switch (config.durationType)
                {
                    case EffectDuration.TemporaryBurst:
                        sb.Append($"Duration: <color={primaryHex}>{config.durationInSeconds}s</color>");
                        break;
                    case EffectDuration.Stacking:
                        sb.Append($"Max Stacks: {maxStacksWithColor}x");
                        break;
                    case EffectDuration.Decaying:
                        sb.Append($"Decay: <color={primaryHex}>-{config.decayRatePerSecond}/s</color>");
                        break;
                }
            }
        }

        return sb.ToString().TrimEnd();
    }
    protected override (string Key, object Value)[] GetDescriptionTokens()
    {
        if (config == null) return System.Array.Empty<(string, object)>();

        return new (string Key, object Value)[]
        {
            ("trigger", config.trigger),
            ("scope", config.scope),
            ("cycleFrequency", config.cycleFrequency),
            ("sourceStat", config.sourceStat),
            ("targetStat", config.targetStat),
            ("operation", config.operation),
            ("scalingFunction", config.scalingFunction),
            ("baseValue", config.baseValue),
            ("scalingFactor", config.scalingFactor),
            ("scalingFactorPercent", config.scalingFactor * 100f),
            ("baseValuePercent", config.baseValue * 100f),
            ("condition", config.condition),
            ("comparisonStat", config.comparisonStat),
            ("thresholdValue", config.thresholdValue),
            ("durationType", config.durationType),
            ("durationInSeconds", config.durationInSeconds),
            ("decayRatePerSecond", config.decayRatePerSecond),
            ("maxStacks", config.maxStacks)
        };
    }
}
