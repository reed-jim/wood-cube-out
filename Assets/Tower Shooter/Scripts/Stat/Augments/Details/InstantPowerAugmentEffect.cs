using System.Collections.Generic;
using Dreamteck.Utilities;
using UnityEngine;

[CreateAssetMenu(fileName = "InstantPowerAugmentEffect", menuName = "Stats/Effects/InstantPowerAugmentEffect")]
public class InstantPowerAugmentEffect : AugmentEffect
{
    public float powerValue;

    public override void ApplyEffect(CharacterStats target, object source)
    {

    }

    public override void RemoveEffect(CharacterStats target, object source)
    {

    }

    public override void ApplyGlobalEffect()
    {
        GrantInstantPower();
    }

    protected override (string Key, object Value)[] GetDescriptionTokens()
    {
        var tokens = new (string Key, object Value)[1];

        tokens[0] = ($"value0", powerValue);

        return tokens;
    }

    private void GrantInstantPower()
    {
        ResourceManager.Ins.ChangeResource(ResourceType.Coin, powerValue);
        CollectResourceManager.Ins.Collect(ResourceType.Coin, powerValue, Vector3.zero);
    }
}
