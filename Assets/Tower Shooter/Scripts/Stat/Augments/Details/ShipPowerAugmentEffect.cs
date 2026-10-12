using System.Collections.Generic;
using Dreamteck.Utilities;
using UnityEngine;

[CreateAssetMenu(fileName = "ShipPowerAugmentEffect", menuName = "Stats/Effects/ShipPowerAugmentEffect")]
public class ShipPowerAugmentEffect : AugmentEffect
{
    public float percentShipPower;
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

        tokens[0] = ($"value0", percentShipPower);

        return tokens;
    }

    private void SetAugmentForRuntime(CharacterStats stats, Augment augment)
    {
        stats.serviceLocator.shipPowerAugmentEffectRuntime.SetAugment(augment);
    }

    private void InitRuntime(CharacterStats stats)
    {
        if (stats.serviceLocator.shipPowerAugmentEffectRuntime == null)
        {
            stats.serviceLocator.shipPowerAugmentEffectRuntime = stats.gameObject.AddComponent<ShipPowerAugmentEffectRuntime>();
        }

        stats.serviceLocator.shipPowerAugmentEffectRuntime.Init(percentShipPower);
    }

    private void Apply(CharacterStats stats)
    {
        stats.serviceLocator.shipPowerAugmentEffectRuntime.Apply(stats);
    }
}

public class ShipPowerAugmentEffectRuntime : BaseAugmentEffectRuntime
{
    private StatModifier statModifier;
    private float percentShipPower;

    public void Init(float percentShipPower)
    {
        this.percentShipPower = percentShipPower;
    }

    public void Apply(CharacterStats stats)
    {
        BaseCharacter node = stats.serviceLocator.character;

        double shipPower = ResourceManager.Ins.GetResourceValue(ResourceType.Coin);

        if (statModifier != null)
        {
            stats.RemoveModifier(statModifier);
        }

        statModifier = new StatModifier(StatType.Damage, ModifierType.Flat, (float)(percentShipPower * shipPower));

        float oldPowerValue = stats.GetStat(StatType.Damage).Value;
        float newPowerValue;

        newPowerValue = stats.AddModifier(statModifier);

        AugmentManager.Ins.UpdatePowerGeneratedByAugment(augment, newPowerValue - oldPowerValue);
    }
}
