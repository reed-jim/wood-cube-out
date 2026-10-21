using System.Collections.Generic;
using Dreamteck.Utilities;
using UnityEngine;

[CreateAssetMenu(fileName = "DebuffAdjacentBuffSelfAugmentEffect", menuName = "Stats/Effects/DebuffAdjacentBuffSelfAugmentEffect")]
public class DebuffAdjacentBuffSelfAugmentEffect : AugmentEffect
{
    public StatModifierData buff;
    public StatModifierData debuff;

    public override void ApplyEffect(CharacterStats target, object source)
    {
        InitRuntime(target);
        Apply(target);
    }

    public override void RemoveEffect(CharacterStats target, object source)
    {

    }

    public override void ApplyEffectOnCycleCompleted(CharacterStats target)
    {
        TrackUtility(target);
    }

    public override void SetAugment(CharacterStats target, Augment augment)
    {
        SetAugmentForRuntime(target, augment);
    }

    protected override (string Key, object Value)[] GetDescriptionTokens()
    {
        var tokens = new (string Key, object Value)[2];

        tokens[0] = ($"value0", buff.value);
        tokens[1] = ($"value1", debuff.value);

        return tokens;
    }

    private void SetAugmentForRuntime(CharacterStats stats, Augment augment)
    {
        stats.serviceLocator.debuffAdjacentBuffSelfAugmentEffectRuntime.SetAugment(augment);
    }

    private void InitRuntime(CharacterStats stats)
    {
        if (stats.serviceLocator.debuffAdjacentBuffSelfAugmentEffectRuntime == null)
        {
            stats.serviceLocator.debuffAdjacentBuffSelfAugmentEffectRuntime = stats.gameObject.AddComponent<DebuffAdjacentBuffSelfAugmentEffectRuntime>();
        }

        stats.serviceLocator.debuffAdjacentBuffSelfAugmentEffectRuntime.Init(buff, debuff);
    }

    private void Apply(CharacterStats stats)
    {
        if (stats.serviceLocator.debuffAdjacentBuffSelfAugmentEffectRuntime == null)
        {
            stats.serviceLocator.debuffAdjacentBuffSelfAugmentEffectRuntime = stats.gameObject.AddComponent<DebuffAdjacentBuffSelfAugmentEffectRuntime>();
        }

        stats.serviceLocator.debuffAdjacentBuffSelfAugmentEffectRuntime.Apply(stats);
    }

    private void TrackUtility(CharacterStats stats)
    {
        stats.serviceLocator.debuffAdjacentBuffSelfAugmentEffectRuntime.TrackUtility();
    }
}

public class DebuffAdjacentBuffSelfAugmentEffectRuntime : BaseAugmentEffectRuntime
{
    private StatModifier buffStatModifier;
    private StatModifier debuffStatModifier;

    private double bonusPower;

    public void Init(StatModifierData buff, StatModifierData debuff)
    {
        buffStatModifier = new StatModifier(buff.statType, buff.modType, buff.value);
        debuffStatModifier = new StatModifier(debuff.statType, debuff.modType, debuff.value);
    }

    public void Apply(CharacterStats stats)
    {
        BaseCharacter node = stats.serviceLocator.character;

        List<BaseCharacter> adjacentNodes = NodeManager.Ins.GetNeighborNodes(node);

        double oldStatValue;
        double newStatValue;

        for (int i = 0; i < adjacentNodes.Count; i++)
        {
            BaseCharacter otherNode = adjacentNodes[i];
            CharacterStats otherNodeStats = otherNode.serviceLocator.stats;

            if (otherNode.nodeData.isUnlocked)
            {
                oldStatValue = otherNodeStats.GetStat(debuffStatModifier.Type).Value;
                newStatValue = otherNodeStats.AddModifier(debuffStatModifier);

                bonusPower += (newStatValue - oldStatValue);
            }
        }

        oldStatValue = stats.GetStat(debuffStatModifier.Type).Value;
        newStatValue = node.serviceLocator.stats.AddModifier(buffStatModifier);

        bonusPower += (newStatValue - oldStatValue);
    }

    public void TrackUtility()
    {
        AugmentManager.Ins.UpdatePowerGeneratedByAugment(augment, bonusPower);
    }
}
