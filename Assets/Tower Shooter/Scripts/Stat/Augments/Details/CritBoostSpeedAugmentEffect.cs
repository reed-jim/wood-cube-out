using System.Collections.Generic;
using Dreamteck.Utilities;
using UnityEngine;

[CreateAssetMenu(fileName = "CritBoostSpeedAugmentEffect", menuName = "Stats/Effects/CritBoostSpeedAugmentEffect")]
public class CritBoostSpeedAugmentEffect : AugmentEffect
{
    public StatModifierData speedBoost;
    public int numCycleBoost;

    public override void ApplyEffect(CharacterStats target, object source)
    {
        InitRuntime(target);
    }

    public override void RemoveEffect(CharacterStats target, object source)
    {

    }

    public override void ApplyEffectOnCycleCompleted(CharacterStats target)
    {
        Apply(target);
    }

    public override void SetAugment(CharacterStats target, Augment augment)
    {
        SetAugmentForRuntime(target, augment);
    }

    protected override (string Key, object Value)[] GetDescriptionTokens()
    {
        var tokens = new (string Key, object Value)[2];

        tokens[0] = ($"value0", speedBoost.value);
        tokens[1] = ($"value1", numCycleBoost);

        return tokens;
    }

    private void SetAugmentForRuntime(CharacterStats stats, Augment augment)
    {
        stats.serviceLocator.alternativeAugmentEffectRuntime.SetAugment(augment);
    }

    private void InitRuntime(CharacterStats stats)
    {
        if (stats.serviceLocator.critBoostSpeedAugmentEffectRuntime == null)
        {
            stats.serviceLocator.critBoostSpeedAugmentEffectRuntime = stats.gameObject.AddComponent<CritBoostSpeedAugmentEffectRuntime>();
        }

        stats.serviceLocator.critBoostSpeedAugmentEffectRuntime.Init(speedBoost, numCycleBoost);
    }

    private void Apply(CharacterStats stats)
    {
        if (stats.serviceLocator.critBoostSpeedAugmentEffectRuntime == null)
        {
            stats.serviceLocator.critBoostSpeedAugmentEffectRuntime = stats.gameObject.AddComponent<CritBoostSpeedAugmentEffectRuntime>();
        }

        stats.serviceLocator.critBoostSpeedAugmentEffectRuntime.Apply(stats);
    }
}

public class CritBoostSpeedAugmentEffectRuntime : BaseAugmentEffectRuntime
{
    public StatModifier speedStatModifier;
    public int numCycleBoost;
    public int remaningCycle;

    private double bonusPowerEachCycle;

    public void Init(StatModifierData speedBoost, int numCycleBoost)
    {
        this.speedStatModifier = new StatModifier(speedBoost.statType, speedBoost.modType, speedBoost.value);
        this.numCycleBoost = numCycleBoost;
    }

    public void Apply(CharacterStats stats)
    {
        if (remaningCycle > 0)
        {
            remaningCycle--;

            if (remaningCycle == 0)
            {
                stats.RemoveModifier(speedStatModifier);
            }

            AugmentManager.Ins.UpdatePowerGeneratedByAugment(augment, bonusPowerEachCycle);

            return;
        }

        BaseCharacter node = stats.serviceLocator.character;

        if (node.isJustCrit)
        {
            double powerValue = stats.GetStat(StatType.Damage).Value;
            double oldSpeedValue = stats.GetStat(StatType.AttackSpeed).Value;
            double newSpeedValue;

            newSpeedValue = stats.AddModifier(speedStatModifier, isApplyAugmentEffect: false);

            bonusPowerEachCycle = ((newSpeedValue - oldSpeedValue) - 1) * powerValue;

            remaningCycle = numCycleBoost;
        }
    }
}
