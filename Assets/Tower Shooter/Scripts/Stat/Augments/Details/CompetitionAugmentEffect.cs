using System.Collections.Generic;
using Dreamteck.Utilities;
using EventDispatcher;
using UnityEngine;

[CreateAssetMenu(fileName = "CompetitionAugmentEffect", menuName = "Stats/Effects/CompetitionAugmentEffect")]
public class CompetitionAugmentEffect : AugmentEffect
{
    public StatModifierData damageModifier;

    public override void ApplyEffect(CharacterStats target, object source)
    {

    }

    public override void RemoveEffect(CharacterStats target, object source)
    {

    }

    public override void ApplyEffect(NodeGroup nodeGroup)
    {
        BuffTheMostPowerNode(nodeGroup);
    }

    public override void ApplyEffectOnNodeStatModified(NodeGroup nodeGroup)
    {
        BuffTheMostPowerNode(nodeGroup);
    }

    public override void ApplyEffectWhenAnyNodeInGroupCompletesCycle(NodeGroup nodeGroup, BaseCharacter node)
    {
        nodeGroup.serviceLocator.competitionAugmentEffectRuntime.TrackAugmentUtility(node);
    }

    public override void SetAugment(NodeGroup nodeGroup, Augment augment)
    {
        if (nodeGroup.serviceLocator.competitionAugmentEffectRuntime == null)
        {
            nodeGroup.serviceLocator.competitionAugmentEffectRuntime = nodeGroup.gameObject.AddComponent<CompetitionAugmentEffectRuntime>();
        }

        nodeGroup.serviceLocator.competitionAugmentEffectRuntime.augment = augment;
    }

    protected override (string Key, object Value)[] GetDescriptionTokens()
    {
        var tokens = new (string Key, object Value)[1];

        tokens[0] = ($"value0", damageModifier.value);

        return tokens;
    }

    private void BuffTheMostPowerNode(NodeGroup nodeGroup)
    {
        if (nodeGroup.serviceLocator.competitionAugmentEffectRuntime == null)
        {
            nodeGroup.serviceLocator.competitionAugmentEffectRuntime = nodeGroup.gameObject.AddComponent<CompetitionAugmentEffectRuntime>();
        }

        nodeGroup.serviceLocator.competitionAugmentEffectRuntime.BuffTheMostPowerNode(nodeGroup, damageModifier);
    }
}

public class CompetitionAugmentEffectRuntime : BaseAugmentEffectRuntime
{
    float maxPower = 0;
    private StatModifier statModifier;
    private BaseCharacter prevStrongestNode = null;
    private BaseCharacter strongestNode = null;
    private float bonusPower = 0;

    public void BuffTheMostPowerNode(NodeGroup nodeGroup, StatModifierData damageModifier)
    {
        for (int i = 0; i < nodeGroup.nodes.Count; i++)
        {
            BaseCharacter node = nodeGroup.nodes[i];

            if (node.nodeData.isUnlocked)
            {
                float power = node.serviceLocator.stats.GetStat(StatType.Damage).Value;
                if (power > maxPower)
                {
                    maxPower = power;
                    strongestNode = node;
                }
            }
        }

        if (statModifier == null)
        {
            statModifier = new StatModifier(StatType.Damage, damageModifier.modType, damageModifier.value);
        }

        if (prevStrongestNode != null)
        {
            prevStrongestNode.serviceLocator.stats.RemoveModifier(statModifier);
        }

        float oldPowerValue = strongestNode.serviceLocator.stats.GetStat(StatType.Damage).Value;

        float newPowerValue = strongestNode.serviceLocator.stats.AddModifier(statModifier, isInvokeStatModifiedEvent: false);

        bonusPower = newPowerValue - oldPowerValue;

        prevStrongestNode = strongestNode;
    }

    public void TrackAugmentUtility(BaseCharacter node)
    {
        if (prevStrongestNode == node)
        {
            AugmentManager.Ins.UpdatePowerGeneratedByAugment(augment, bonusPower);
        }
    }
}
