using System.Collections.Generic;
using Dreamteck.Utilities;
using UnityEngine;

[CreateAssetMenu(fileName = "SumAugmentEffect", menuName = "Stats/Effects/SumAugmentEffect")]
public class SumAugmentEffect : AugmentEffect
{
    public float sumEfficiency;

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
        var tokens = new (string Key, object Value)[1];

        tokens[0] = ($"value0", sumEfficiency);

        return tokens;
    }

    private void SetAugmentForRuntime(CharacterStats stats, Augment augment)
    {
        stats.serviceLocator.alternativeAugmentEffectRuntime.SetAugment(augment);
    }

    private void InitRuntime(CharacterStats stats)
    {
        if (stats.serviceLocator.sumAugmentEffectRuntime == null)
        {
            stats.serviceLocator.sumAugmentEffectRuntime = stats.gameObject.AddComponent<SumAugmentEffectRuntime>();
        }

        stats.serviceLocator.sumAugmentEffectRuntime.Init(sumEfficiency);
    }

    private void Apply(CharacterStats stats)
    {
        if (stats.serviceLocator.sumAugmentEffectRuntime == null)
        {
            stats.serviceLocator.sumAugmentEffectRuntime = stats.gameObject.AddComponent<SumAugmentEffectRuntime>();
        }

        stats.serviceLocator.sumAugmentEffectRuntime.Apply(stats);
    }
}

public class SumAugmentEffectRuntime : BaseAugmentEffectRuntime
{
    public float sumEfficiency;

    private StatModifier statModifier;

    public void Init(float sumEfficiency)
    {
        this.sumEfficiency = sumEfficiency;
    }

    public void Apply(CharacterStats stats)
    {
        BaseCharacter node = stats.serviceLocator.character;

        List<BaseCharacter> adjacentNodes = NodeManager.Ins.GetNeighborNodes(node);

        bool isNoAdjacentNode = true;
        double totalPower = 0;

        for (int i = 0; i < adjacentNodes.Count; i++)
        {
            BaseCharacter otherNode = adjacentNodes[i];
            CharacterStats otherNodeStats = otherNode.serviceLocator.stats;

            if (otherNode.nodeData.isUnlocked)
            {
                totalPower += otherNodeStats.GetStat(StatType.Damage).Value;

                isNoAdjacentNode = false;
            }
        }

        if (isNoAdjacentNode)
        {
            adjacentNodes.Shuffle();
            adjacentNodes[0].Unlock();
        }
        else
        {
            totalPower *= sumEfficiency;

            if (statModifier != null)
            {
                stats.RemoveModifier(statModifier);
            }

            double oldPowerValue = stats.GetStat(StatType.Damage).Value;
            double newPowerValue;

            statModifier = new StatModifier(StatType.Damage, ModifierType.Flat, totalPower);

            newPowerValue = stats.AddModifier(statModifier);

            AugmentManager.Ins.UpdatePowerGeneratedByAugment(augment, newPowerValue - oldPowerValue);
        }
    }
}
