using System.Collections.Generic;
using Dreamteck.Utilities;
using UnityEngine;

[CreateAssetMenu(fileName = "OverheatAugmentEffect", menuName = "Stats/Effects/OverheatAugmentEffect")]
public class OverheatAugmentEffect : AugmentEffect
{
    public float percentIncreasePowerPerCycle;

    public override void ApplyEffect(CharacterStats target, object source)
    {
        InitRuntime(target);
    }

    public override void RemoveEffect(CharacterStats target, object source)
    {

    }

    public override void ApplyEffectOnCycleCompleted(CharacterStats target)
    {
        IncreasePower(target);
    }

    protected override (string Key, object Value)[] GetDescriptionTokens()
    {
        var tokens = new (string Key, object Value)[1];

        tokens[0] = ($"value0", percentIncreasePowerPerCycle);

        return tokens;
    }

    private void InitRuntime(CharacterStats stats)
    {
        if (stats.serviceLocator.overheatAugmentEffectRuntime == null)
        {
            stats.serviceLocator.overheatAugmentEffectRuntime = stats.gameObject.AddComponent<OverheatAugmentEffectRuntime>();
        }

        stats.serviceLocator.overheatAugmentEffectRuntime.Init(stats, percentIncreasePowerPerCycle);
    }

    private void IncreasePower(CharacterStats stats)
    {
        if (stats.serviceLocator.overheatAugmentEffectRuntime == null)
        {
            stats.serviceLocator.overheatAugmentEffectRuntime = stats.gameObject.AddComponent<OverheatAugmentEffectRuntime>();
        }

        stats.serviceLocator.overheatAugmentEffectRuntime.Apply(stats);
    }
}

public class OverheatAugmentEffectRuntime : MonoBehaviour
{
    public float percentIncreasePowerPerCycle;
    private float heat;
    private float heatPerCycle = 5;
    private float maxHeat = 100;
    private float currentIncreasePowerPercent;

    private StoredPowerCapsuleUI storedPowerCapsuleUI;

    private StatModifier lastStatModifier;

    public void Init(CharacterStats stats, float percentIncreasePowerPerCycle)
    {
        this.percentIncreasePowerPerCycle = percentIncreasePowerPerCycle;

        BaseCharacter node = stats.serviceLocator.character;

        if (storedPowerCapsuleUI == null)
        {
            storedPowerCapsuleUI = ObjectPoolingEverything.GetFromPool<StoredPowerCapsuleUI>(GameConstants.storedPowerCapsuleUI);

            storedPowerCapsuleUI.Init(node);
        }
    }

    public void Apply(CharacterStats stats)
    {
        heat += heatPerCycle;

        if (heat < maxHeat)
        {
            currentIncreasePowerPercent = percentIncreasePowerPerCycle * heat;

            StatModifier statModifier = new StatModifier(StatType.Damage, ModifierType.PercentAdd, currentIncreasePowerPercent);

            if (lastStatModifier != null)
            {
                stats.RemoveModifier(lastStatModifier);
            }

            stats.AddModifier(statModifier, isApplyAugmentEffect: false);

            lastStatModifier = statModifier;
        }
        else
        {
            heat = 0;
            currentIncreasePowerPercent = 0;
        }

        storedPowerCapsuleUI.UpdateProgress(heat / maxHeat);
    }
}
