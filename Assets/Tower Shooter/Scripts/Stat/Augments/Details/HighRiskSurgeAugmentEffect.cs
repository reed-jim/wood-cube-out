using System.Collections.Generic;
using Dreamteck.Utilities;
using UnityEngine;

[CreateAssetMenu(fileName = "HighRiskSurgeAugmentEffect", menuName = "Stats/Effects/HighRiskSurgeAugmentEffect")]
public class HighRiskSurgeAugmentEffect : AugmentEffect
{
    public StatModifierData debuff;
    public float burstPercentToPower;
    public float chanceToBurst;
    public float maxLosePercent;

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
        var tokens = new (string Key, object Value)[4];

        tokens[0] = ($"value0", chanceToBurst);
        tokens[1] = ($"value1", burstPercentToPower);
        tokens[2] = ($"value2", debuff.value);
        tokens[3] = ($"value3", maxLosePercent);

        return tokens;
    }

    private void SetAugmentForRuntime(CharacterStats stats, Augment augment)
    {
        stats.serviceLocator.highRiskSurgeAugmentEffectRuntime.SetAugment(augment);
    }

    private void InitRuntime(CharacterStats stats)
    {
        if (stats.serviceLocator.highRiskSurgeAugmentEffectRuntime == null)
        {
            stats.serviceLocator.highRiskSurgeAugmentEffectRuntime = stats.gameObject.AddComponent<HighRiskSurgeAugmentEffectRuntime>();
        }

        stats.serviceLocator.highRiskSurgeAugmentEffectRuntime.Init(debuff, burstPercentToPower, chanceToBurst, maxLosePercent);
    }

    private void Apply(CharacterStats stats)
    {
        stats.serviceLocator.highRiskSurgeAugmentEffectRuntime.Apply(stats);
    }
}

public class HighRiskSurgeAugmentEffectRuntime : BaseAugmentEffectRuntime
{
    private StatModifier debuffStatModifier;
    private float burstPercentToPower;
    private float chanceToBurst;
    private float maxLosePercent;

    private StatModifierData debuff;

    public void Init(StatModifierData debuff, float burstPercentToPower, float chanceToBurst, float maxLosePercent)
    {
        debuffStatModifier = new StatModifier(debuff.statType, debuff.modType, debuff.value);
        this.burstPercentToPower = burstPercentToPower;
        this.chanceToBurst = chanceToBurst;
        this.maxLosePercent = maxLosePercent;

        this.debuff = debuff;
    }

    public void Apply(CharacterStats stats)
    {
        BaseCharacter node = stats.serviceLocator.character;

        if (UnityEngine.Random.value < chanceToBurst)
        {
            double burstPower = burstPercentToPower * stats.GetStat(StatType.Damage).Value;

            node.GenerateInstantPower(burstPower);

            AugmentManager.Ins.UpdatePowerGeneratedByAugment(augment, burstPower);

            if (debuffStatModifier != null)
            {
                stats.RemoveModifier(debuffStatModifier);

                debuffStatModifier.Value = 0;
            }
        }
        else
        {
            if (debuffStatModifier != null)
            {
                if (Mathf.Abs((float)debuffStatModifier.Value) >= maxLosePercent)
                {
                    return;
                }

                stats.RemoveModifier(debuffStatModifier);

                debuffStatModifier.Value += debuff.value;
            }
            else
            {
                debuffStatModifier = new StatModifier(debuff.statType, debuff.modType, debuff.value);
            }

            stats.AddModifier(debuffStatModifier);
        }
    }
}
