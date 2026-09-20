using System.Collections.Generic;
using Dreamteck.Utilities;
using UnityEngine;

[CreateAssetMenu(fileName = "AugmentForPowerAugmentEffect", menuName = "Stats/Effects/AugmentForPowerAugmentEffect")]
public class AugmentForPowerAugmentEffect : AugmentEffect
{
    public float powerValuePerAugment;

    public override void ApplyEffect(CharacterStats target, object source)
    {
        SacrificeAugmentsForPower(target);
    }

    public override void RemoveEffect(CharacterStats target, object source)
    {

    }

    protected override (string Key, object Value)[] GetDescriptionTokens()
    {
        var tokens = new (string Key, object Value)[1];

        tokens[0] = ($"value0", powerValuePerAugment);

        return tokens;
    }

    private void SacrificeAugmentsForPower(CharacterStats stats)
    {
        BaseCharacter node = stats.serviceLocator.character;

        int numAugmentToRemove = node.ownedAugments.Count;
        numAugmentToRemove = Mathf.Max(numAugmentToRemove, 1);

        float powerValue = numAugmentToRemove * powerValuePerAugment;

        ResourceManager.Ins.ChangeResource(ResourceType.Coin, powerValue);
        CollectResourceManager.Ins.Collect(ResourceType.Coin, powerValue, Vector3.zero);

        node.ownedAugments.Clear();
    }
}
