using System.Collections.Generic;
using Dreamteck.Utilities;
using UnityEngine;

[CreateAssetMenu(fileName = "ChainCriticalAugmentEffect", menuName = "Stats/Effects/ChainCriticalAugmentEffect")]
public class ChainCriticalAugmentEffect : AugmentEffect
{
    public override void ApplyEffect(CharacterStats target, object source)
    {

    }

    public override void RemoveEffect(CharacterStats target, object source)
    {

    }

    public override void ApplyEffectOnCycleCompleted(CharacterStats target)
    {
        Apply(target);
    }

    private void Apply(CharacterStats stats)
    {
        if (stats.serviceLocator.chainCriticalAugmentEffectRuntime == null)
        {
            stats.serviceLocator.chainCriticalAugmentEffectRuntime = stats.gameObject.AddComponent<ChainCriticalAugmentEffectRuntime>();
        }

        stats.serviceLocator.chainCriticalAugmentEffectRuntime.Apply(stats);
    }
}

public class ChainCriticalAugmentEffectRuntime : BaseAugmentEffectRuntime
{
    public void Apply(CharacterStats stats)
    {
        BaseCharacter node = stats.serviceLocator.character;

        if (node.isJustCrit)
        {
            List<BaseCharacter> adjacentNodes = NodeManager.Ins.GetNeighborNodes(node);

            bool isNoAdjacentNode = true;

            double totalBonusPower = 0;

            for (int i = 0; i < adjacentNodes.Count; i++)
            {
                BaseCharacter otherNode = adjacentNodes[i];
                CharacterStats otherNodeStats = otherNode.serviceLocator.stats;

                if (otherNode.nodeData.isUnlocked)
                {
                    otherNode.isGuaranteedCrit = true;

                    isNoAdjacentNode = false;

                    totalBonusPower += otherNodeStats.GetStat(StatType.Damage).Value * otherNodeStats.GetStat(StatType.CritMultiplier).Value;
                }
            }

            if (isNoAdjacentNode)
            {
                adjacentNodes.Shuffle();
                adjacentNodes[0].Unlock();
            }
            else
            {
                AugmentManager.Ins.UpdatePowerGeneratedByAugment(augment, totalBonusPower);
            }
        }
    }
}
