using System.Collections.Generic;
using Dreamteck.Utilities;
using UnityEngine;

[CreateAssetMenu(fileName = "VampireAugmentEffect", menuName = "Stats/Effects/VampireAugmentEffect")]
public class VampireAugmentEffect : AugmentEffect
{
    public float percentPowerSteal;

    public override void ApplyEffect(CharacterStats target, object source)
    {


    }

    public override void RemoveEffect(CharacterStats target, object source)
    {

    }

    public override void ApplyEffectOnCycleCompleted(CharacterStats target)
    {
        BaseCharacter node = target.serviceLocator.character;

        List<BaseCharacter> neighborNodes = NodeManager.Ins.GetNeighborNodes(node);

        float totalPower = 0;
        bool isNoAdjacentNode = true;

        List<BaseCharacter> lockedNodes = new List<BaseCharacter>();

        for (int i = 0; i < neighborNodes.Count; i++)
        {
            BaseCharacter neighborNode = neighborNodes[i];

            if (neighborNode.nodeData.isUnlocked)
            {
                totalPower += percentPowerSteal * neighborNodes[i].stats.GetStat(StatType.Damage).Value;

                isNoAdjacentNode = false;
            }
            else
            {
                lockedNodes.Add(neighborNode);
            }
        }

        if (isNoAdjacentNode)
        {
            lockedNodes.Shuffle();
            lockedNodes[0].Unlock();
        }
        else
        {
            node.GenerateInstantPower(totalPower);
        }
    }

    protected override (string Key, object Value)[] GetDescriptionTokens()
    {
        var tokens = new (string Key, object Value)[1];

        tokens[0] = ($"value0", percentPowerSteal);

        return tokens;
    }
}
