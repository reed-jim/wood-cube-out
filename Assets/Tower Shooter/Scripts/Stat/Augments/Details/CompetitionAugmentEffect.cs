using System.Collections.Generic;
using Dreamteck.Utilities;
using EventDispatcher;
using UnityEngine;

[CreateAssetMenu(fileName = "ChainCriticalAugmentEffect", menuName = "Stats/Effects/ChainCriticalAugmentEffect")]
public class CompetitionAugmentEffect : AugmentEffect
{
    public StatModifierData damageModifier;

    private StatModifier statModifier;

    public override void ApplyEffect(CharacterStats target, object source)
    {

    }

    public override void RemoveEffect(CharacterStats target, object source)
    {

    }

    private void BuffTheMostPowerNode()
    {
        float maxPower = 0;
        BaseCharacter strongestNode = null;

        for (int i = 0; i < NodeManager.Ins.nodes.Count; i++)
        {
            BaseCharacter node = NodeManager.Ins.nodes[i];

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

        strongestNode.serviceLocator.stats.RemoveModifier(statModifier);
        strongestNode.serviceLocator.stats.AddModifier(statModifier);
    }
}

public class CompetitionAugmentEffectRuntime : MonoBehaviour
{
    public void Apply(CharacterStats stats)
    {
        BaseCharacter node = stats.serviceLocator.character;

        if (node.isJustCrit)
        {
            List<BaseCharacter> adjacentNodes = NodeManager.Ins.GetNeighborNodes(node);

            bool isNoAdjacentNode = true;

            for (int i = 0; i < adjacentNodes.Count; i++)
            {
                BaseCharacter otherNode = adjacentNodes[i];

                if (otherNode.nodeData.isUnlocked)
                {
                    otherNode.isGuaranteedCrit = true;

                    isNoAdjacentNode = false;
                }
            }

            if (isNoAdjacentNode)
            {
                adjacentNodes.Shuffle();
                adjacentNodes[0].Unlock();
            }
        }
    }
}
