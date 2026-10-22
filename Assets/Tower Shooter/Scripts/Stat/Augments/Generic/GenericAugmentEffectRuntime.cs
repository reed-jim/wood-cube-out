using System;
using System.Collections.Generic;
using UnityEngine;

public class GenericAugmentEffectRuntime : BaseAugmentEffectRuntime
{
    private GenericAugmentData config;
    private CharacterStats targetStats;
    private BaseCharacter node;

    private int triggerTime = 0;
    private int cycleCounter = 0;
    private int currentStacks = 0;
    private float activeTimer = 0f;
    private double accumulatedValue = 0f;

    // Active modifier stored so we can calculate stat deltas accurately
    private StatModifier activeModifier;

    public static event Action<BaseCharacter> updateNodeStatEvent;

    public void Init(GenericAugmentData configData)
    {
        config = configData;
        targetStats = GetComponent<CharacterStats>();
        if (targetStats != null && targetStats.serviceLocator != null)
        {
            node = targetStats.serviceLocator.character;
        }
    }

    public override void SetAugment(Augment augment)
    {
        base.SetAugment(augment);

        if (config.trigger == EffectTrigger.Once)
        {
            ExecuteEffect();
        }
    }

    private void Update()
    {
        if (config == null) return;

        if (config.trigger == EffectTrigger.WhenStatCrossesThreshold)
        {
            double statVal = GetStatValue(targetStats, config.sourceStat);
            if (statVal >= config.thresholdValue)
            {
                ExecuteEffect();
            }
        }

        HandleLifetime();
    }

    public void OnTriggerRaised(EffectTrigger raisedTrigger)
    {
        // if (config == null || config.trigger != raisedTrigger) return;

        if (raisedTrigger != config.trigger)
        {
            if (raisedTrigger == EffectTrigger.OnCycleComplete && config.trigger == EffectTrigger.EveryXCycles)
            {

            }
            else
            {
                return;
            }
        }

        if (raisedTrigger == EffectTrigger.OnCrit && node != null && !node.isJustCrit) return;

        if (config.trigger == EffectTrigger.EveryXCycles)
        {
            cycleCounter++;
            if (cycleCounter < config.cycleFrequency) return;
            cycleCounter = 0;
        }

        ExecuteEffect();
    }

    private void ExecuteEffect()
    {
        if (!EvaluateConditions()) return;

        List<CharacterStats> targets = GetTargetStatsList();

        foreach (var tStat in targets)
        {
            double calculatedVal;

            if (config.valueSource == AugmentValueSource.FlatValue)
            {
                // Use the flat base value directly (multiplied by augment efficiency)
                calculatedVal =
                    AugmentManager.Ins.GetAugmentEfficiency(augment) * ScalingMath.Evaluate(config.baseValue, triggerTime, config.scalingFunction, config.scalingFactor);
            }
            else
            {
                // Original logic: evaluate using the source stat
                double sourceVal = config.sourceStatValueRatio * GetStatValue(targetStats, config.sourceStat);
                calculatedVal =
                    AugmentManager.Ins.GetAugmentEfficiency(augment) * ScalingMath.Evaluate(config.baseValue, sourceVal, config.scalingFunction, config.scalingFactor);
            }

            ApplyOperationToTarget(tStat, calculatedVal);

            PlayTextVisualEffects(tStat, calculatedVal);

            tStat.serviceLocator.augmentEffectController.PlayEffect(0);
        }

        triggerTime++;
    }

    private void ApplyOperationToTarget(CharacterStats target, double calculatedVal)
    {
        double oldStatValue = GetStatValue(target, config.targetStat);
        double currentTargetVal = oldStatValue;

        // now we have value applied the scaling function --> do operation between the value with target stat's current value
        double resultVal = ScalingMath.ApplyOperation(config.operation, currentTargetVal, calculatedVal);
        double valueForFlatAddModifier = resultVal - currentTargetVal;

        if (activeModifier != null)
        {
            valueForFlatAddModifier += activeModifier.Value;
        }

        if (config.operation == EffectOperation.Swap)
        {
            double tempVal = GetStatValue(target, config.sourceStat);
            SetStatValue(target, config.sourceStat, currentTargetVal);
            SetStatValue(target, config.targetStat, tempVal);

            // Track utility delta from the swap
            double newStatValue = GetStatValue(target, config.targetStat);
            TrackUtilityDelta(target, oldStatValue, newStatValue);
            return;
        }

        if (config.durationType == EffectDuration.Stacking)
        {
            if (currentStacks < config.maxStacks)
            {
                currentStacks++;
                accumulatedValue += valueForFlatAddModifier;
                SetStatValue(target, config.targetStat, accumulatedValue);
                TrackUtilityDelta(target, oldStatValue, accumulatedValue);
            }
        }
        else if (config.durationType == EffectDuration.TemporaryBurst)
        {
            activeTimer = config.durationInSeconds;
            SetStatValue(target, config.targetStat, valueForFlatAddModifier);
            TrackUtilityDelta(target, oldStatValue, valueForFlatAddModifier);
        }
        else
        {
            SetStatValue(target, config.targetStat, valueForFlatAddModifier);
            double newStatValue = GetStatValue(target, config.targetStat);
            TrackUtilityDelta(target, oldStatValue, newStatValue);
        }
    }

    /// <summary>
    /// Calculates utility/power generated by this augment and logs it with AugmentManager.
    /// </summary>
    private void TrackUtilityDelta(CharacterStats target, double oldVal, double newVal)
    {
        if (augment == null) return;

        double delta = newVal - oldVal;
        if (Mathf.Approximately((float)delta, 0f)) return;

        double calculatedUtility = 0f;

        switch (config.targetStat)
        {
            case StatType.Damage:
                // Direct power addition/multiplication directly creates power utility[cite: 3, 6, 8, 9]
                calculatedUtility = delta;
                break;

            case StatType.AttackSpeed:
                // Speed boost utility: Converts speed increase into effective power throughput per cycle[cite: 7]
                double basePower = target.GetStat(StatType.Damage).Value;
                calculatedUtility = delta * basePower;
                break;

            case StatType.CritChance:
                // Crit Chance utility: (Crit Chance Gain) * (Crit Multiplier - 1) * Base Power
                double critMulti = target.GetStat(StatType.CritMultiplier).Value;
                double power = target.GetStat(StatType.Damage).Value;
                calculatedUtility = delta * (critMulti - 1f) * power;
                break;

            case StatType.CritMultiplier:
                // Crit Multiplier utility: (Crit Multi Gain) * (Crit Chance) * Base Power
                double critChance = target.GetStat(StatType.CritChance).Value;
                double pwr = target.GetStat(StatType.Damage).Value;
                calculatedUtility = delta * critChance * pwr;
                break;
        }

        if (calculatedUtility != 0f)
        {
            AugmentManager.Ins.UpdatePowerGeneratedByAugment(augment, calculatedUtility);
        }
    }

    private bool EvaluateConditions()
    {
        if (config.condition == ConditionType.AlwaysActive) return true;

        double statA = GetStatValue(targetStats, config.sourceStat);
        double statB = GetStatValue(targetStats, config.comparisonStat);

        switch (config.condition)
        {
            case ConditionType.StatGreaterThanOther: return statA > statB;
            case ConditionType.StatLessThanOther: return statA < statB;
            case ConditionType.StatGreaterThan: return statB > config.thresholdValue;
            case ConditionType.StatLessThan: return statB < config.thresholdValue;
            default: return true;
        }
    }

    private List<CharacterStats> GetTargetStatsList()
    {
        List<CharacterStats> list = new List<CharacterStats>();

        switch (config.scope)
        {
            case EffectScope.Self:
                if (targetStats != null) list.Add(targetStats);
                break;

            case EffectScope.AdjacentNodes:
                if (node != null)
                {
                    List<BaseCharacter> neighbors = NodeManager.Ins.GetNeighborNodes(node);
                    foreach (var neighbor in neighbors)
                    {
                        if (neighbor.nodeData.isUnlocked)
                        {
                            list.Add(neighbor.serviceLocator.stats);
                        }
                    }
                }
                break;

            case EffectScope.Global:
                if (NodeManager.Ins != null)
                {
                    foreach (var activeNode in NodeManager.Ins.GetAllUnlockedNodes())
                    {
                        list.Add(activeNode.serviceLocator.stats);
                    }
                }
                break;
        }

        return list;
    }

    private void HandleLifetime()
    {
        if (config.durationType == EffectDuration.TemporaryBurst && activeTimer > 0)
        {
            activeTimer -= Time.deltaTime;
            if (activeTimer <= 0)
            {
                RevertEffect();
            }
        }
        else if (config.durationType == EffectDuration.Decaying && accumulatedValue > 0)
        {
            double preDecay = accumulatedValue;
            accumulatedValue -= config.decayRatePerSecond * Time.deltaTime;
            accumulatedValue = Math.Max(0, accumulatedValue);
            SetStatValue(targetStats, config.targetStat, accumulatedValue);

            // Log negative delta when decaying
            TrackUtilityDelta(targetStats, preDecay, accumulatedValue);
        }
    }

    private double GetStatValue(CharacterStats stats, StatType type)
    {
        if (stats == null) return 0f;

        switch (type)
        {
            case StatType.Damage: return stats.GetStat(StatType.Damage).Value;
            case StatType.AttackSpeed: return stats.GetStat(StatType.AttackSpeed).Value;
            case StatType.CritChance: return stats.GetStat(StatType.CritChance).Value;
            case StatType.CritMultiplier: return stats.GetStat(StatType.CritMultiplier).Value;
            default: return 0f;
        }
    }

    private void SetStatValue(CharacterStats stats, StatType type, double newValue)
    {
        if (stats == null) return;

        StatType internalStatType = StatType.Damage;
        switch (type)
        {
            case StatType.Damage: internalStatType = StatType.Damage; break;
            case StatType.AttackSpeed: internalStatType = StatType.AttackSpeed; break;
            case StatType.CritChance: internalStatType = StatType.CritChance; break;
            case StatType.CritMultiplier: internalStatType = StatType.CritMultiplier; break;
        }

        if (activeModifier != null)
        {
            stats.RemoveModifier(activeModifier);
        }

        activeModifier = new StatModifier(internalStatType, ModifierType.Flat, newValue);
        stats.AddModifier(activeModifier, isApplyAugmentEffect: false);

        updateNodeStatEvent?.Invoke(stats.serviceLocator.character);
    }

    private void RevertEffect()
    {
        if (activeModifier != null && targetStats != null)
        {
            double preVal = GetStatValue(targetStats, config.targetStat);
            targetStats.RemoveModifier(activeModifier);
            double postVal = GetStatValue(targetStats, config.targetStat);

            TrackUtilityDelta(targetStats, preVal, postVal);
            activeModifier = null;
        }
    }

    #region Visual
    private void PlayVisualEffects(CharacterStats targetStats, float calculatedVal)
    {
        NodeUI nodeUI = targetStats.serviceLocator.nodeUI;

        string augmentIconId = AugmentScoringUtility.GetAugmentIconId(augment);

        nodeUI.PlayEffectOnAugmentActivate(augment, augmentIconId);
    }

    private void PlayTextVisualEffects(CharacterStats targetStats, double calculatedVal)
    {
        Vector3 targetPosition = targetStats.transform.position;

        // 1. Floating Text Popups
        string popupText = GetEffectPopupText(calculatedVal);
        Color textColor = AugmentManager.Ins.GetStatColor(config.targetStat);

        DamageText damageText = ObjectPoolingEverything.GetFromPool<DamageText>(GameConstants.damageText);

        damageText.transform.SetParent(targetStats.transform);

        damageText.ShowAugmentEffect(
            config,
            calculatedVal,
            targetStats.transform.position,
            targetStats.transform,
            isMoveHalfway: true
        );

        // Call your Floating Text / Damage Number Manager here
        // FloatingTextManager.Ins.Show(popupText, targetPosition, textColor);

        // 2. Play Beam or Pulse FX for Scope / Propagation
        if (config.scope == EffectScope.AdjacentNodes || config.scope == EffectScope.Global)
        {
            // LineRenderer or Particle burst connecting source node -> target node
            // VFXManager.Ins.PlayLinkEffect(transform.position, targetPosition);
        }

        // 3. Optional Node Flash
        var spriteRenderer = targetStats.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // StartCoroutine(FlashNodeRoutine(spriteRenderer, textColor));
        }
    }

    private string GetEffectPopupText(double calculatedVal)
    {
        switch (config.operation)
        {
            case EffectOperation.Swap:
                return "SWAP!";
            case EffectOperation.Multiply:
                return $"x{calculatedVal:F1} {config.targetStat}!";
            default:
                return $"+{calculatedVal:F0} {config.targetStat}";
        }
    }
    #endregion
}
