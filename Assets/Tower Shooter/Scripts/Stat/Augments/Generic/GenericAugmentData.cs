using System;
using UnityEngine;

[Serializable]
public class GenericAugmentData
{
    [Header("Trigger & Scope")]
    public EffectTrigger trigger = EffectTrigger.OnCycleComplete;
    public EffectScope scope = EffectScope.Self;
    public int cycleFrequency = 1; // Used for EveryXCycles

    [Header("Stats Logic")]
    public StatType sourceStat = StatType.Damage;
    public StatType targetStat = StatType.Damage;
    public EffectOperation operation = EffectOperation.Add;

    [Header("Mathematical Scaling")]
    public AugmentValueSource valueSource = AugmentValueSource.SourceStat; // Default to existing behavior
    public ScalingFunction scalingFunction = ScalingFunction.Linear;
    public float baseValue = 0f;
    public float scalingFactor = 1f;

    [Header("Conditions")]
    public ConditionType condition = ConditionType.AlwaysActive;
    public StatType comparisonStat = StatType.Damage;
    public float thresholdValue = 0f;

    [Header("Duration & Lifetime")]
    public EffectDuration durationType = EffectDuration.Permanent;
    public float durationInSeconds = 5f;
    public float decayRatePerSecond = 1f;
    public int maxStacks = 10;
}
