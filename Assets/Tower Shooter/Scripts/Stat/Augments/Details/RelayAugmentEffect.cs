using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RelayAugmentEffect", menuName = "Stats/Effects/RelayAugmentEffect")]
public class RelayAugmentEffect : AugmentEffect
{
    public List<StatModifierData> statModifiers = new List<StatModifierData>();

    public override void ApplyEffect(CharacterStats target, object source)
    {
        List<BaseCharacter> neighborNodes = NodeManager.Ins.GetNeighborNodes(target.serviceLocator.character);

        for (int i = 0; i < neighborNodes.Count; i++)
        {
            BaseCharacter node = neighborNodes[i];

            foreach (var modData in statModifiers)
            {
                node.serviceLocator.stats.AddModifier(new StatModifier(modData.statType, modData.modType, modData.value, 0, this));
            }
        }
    }

    public override void RemoveEffect(CharacterStats target, object source)
    {

    }
}
