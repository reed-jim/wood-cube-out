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

        return $"<color={effectColorHex}>{effectName}:</color> {autoText}";
    }

    /// <summary>
    /// Constructs a readable sentence automatically based on GenericAugmentData configuration.
    /// </summary>
    private string GenerateAutoDescription()
    {
        StringBuilder sb = new StringBuilder();

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
                sb.Append($"Every {config.cycleFrequency} cycles, ");
                break;
            case EffectTrigger.WhenIdle:
                sb.Append("While idle, ");
                break;
            case EffectTrigger.WhenStatCrossesThreshold:
                sb.Append($"When {config.sourceStat} reaches {config.thresholdValue}, ");
                break;
            case EffectTrigger.Permanently:
                sb.Append("Permanently ");
                break;
        }

        // 2. OPERATION & TARGET STAT
        switch (config.operation)
        {
            case EffectOperation.Add:
                sb.Append($"increases {config.targetStat}");
                break;
            case EffectOperation.Multiply:
                sb.Append($"multiplies {config.targetStat}");
                break;
            case EffectOperation.Convert:
                sb.Append($"converts {config.sourceStat} into {config.targetStat}");
                break;
            case EffectOperation.Swap:
                sb.Append($"swaps {config.sourceStat} with {config.targetStat}");
                break;
            case EffectOperation.SquareRoot:
                sb.Append($"applies square root scaling to {config.targetStat}");
                break;
            case EffectOperation.Logarithm:
                sb.Append($"applies logarithmic scaling to {config.targetStat}");
                break;
            case EffectOperation.Copy:
                sb.Append($"copies {config.sourceStat} to {config.targetStat}");
                break;
        }

        // 3. MATH SCALING & VALUES
        if (config.operation != EffectOperation.Swap)
        {
            if (config.baseValue > 0)
            {
                sb.Append($" by {config.baseValue}");
            }

            if (config.scalingFactor != 0 && config.sourceStat != config.targetStat)
            {
                string scalingTypeStr = config.scalingFunction != ScalingFunction.Linear ? $" ({config.scalingFunction})" : "";
                sb.Append($" + {config.scalingFactor * 100f}% of {config.sourceStat}{scalingTypeStr}");
            }
            else if (config.scalingFactor != 0 && config.scalingFunction != ScalingFunction.Linear)
            {
                sb.Append($" scaling {config.scalingFunction}ly by factor {config.scalingFactor}");
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
                sb.Append($" (Requires {config.sourceStat} > {config.comparisonStat})");
                break;
            case ConditionType.StatLessThanOther:
                sb.Append($" (Requires {config.sourceStat} < {config.comparisonStat})");
                break;
            case ConditionType.StatThresholdGate:
                sb.Append($" (Requires {config.sourceStat} >= {config.thresholdValue})");
                break;
        }

        // 6. DURATION & LIFETIME
        switch (config.durationType)
        {
            case EffectDuration.TemporaryBurst:
                sb.Append($" for {config.durationInSeconds}s");
                break;
            case EffectDuration.Stacking:
                sb.Append($" (Stacks up to {config.maxStacks}x)");
                break;
            case EffectDuration.Decaying:
                sb.Append($" (Decays by {config.decayRatePerSecond}/s)");
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
