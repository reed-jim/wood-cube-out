using UnityEngine;

[CreateAssetMenu(fileName = "AlternativeEffect", menuName = "Stats/Effects/AlternativeEffect")]
public class AlternativeEffect : AugmentEffect
{
    public StatModifierData powerIncreaseData;
    public StatModifierData speedIncreaseData;

    public override void ApplyEffect(CharacterStats target, object source)
    {
        InitRuntime(target);
    }

    // even cycle increase power, odd cycle increase speed
    public override void ApplyEffectOnCycleCompleted(CharacterStats target)
    {
        Alternate(target);
    }

    public override void RemoveEffect(CharacterStats target, object source)
    {

    }

    public override void SetAugment(CharacterStats target, Augment augment)
    {
        SetAugmentForRuntime(target, augment);
    }

    protected override (string Key, object Value)[] GetDescriptionTokens()
    {
        var tokens = new (string Key, object Value)[2];

        tokens[0] = ($"value0", powerIncreaseData.value);
        tokens[1] = ($"value1", speedIncreaseData.value);

        return tokens;
    }

    private void InitRuntime(CharacterStats stats)
    {
        if (stats.serviceLocator.alternativeAugmentEffectRuntime == null)
        {
            stats.serviceLocator.alternativeAugmentEffectRuntime = stats.gameObject.AddComponent<AlternativeAugmentEffectRuntime>();
        }

        stats.serviceLocator.alternativeAugmentEffectRuntime.Init(powerIncreaseData, speedIncreaseData);
    }

    private void SetAugmentForRuntime(CharacterStats stats, Augment augment)
    {
        stats.serviceLocator.alternativeAugmentEffectRuntime.SetAugment(augment);
    }

    private void Alternate(CharacterStats stats)
    {
        stats.serviceLocator.alternativeAugmentEffectRuntime.Alternate(stats);
    }
}

public class AlternativeAugmentEffectRuntime : BaseAugmentEffectRuntime
{
    private StatModifier powerIncrease;
    private StatModifier speedIncrease;

    public void Init(StatModifierData powerIncreaseData, StatModifierData speedIncreaseData)
    {
        powerIncrease = new StatModifier(powerIncreaseData.statType, powerIncreaseData.modType, powerIncreaseData.value);
        speedIncrease = new StatModifier(speedIncreaseData.statType, speedIncreaseData.modType, speedIncreaseData.value);
    }

    public void Alternate(CharacterStats target)
    {
        BaseCharacter node = target.serviceLocator.character;

        if (node.currentCycle % 2 == 0)
        {
            double oldPowerValue = target.GetStat(StatType.Damage).Value;
            double newPowerValue;

            target.RemoveModifier(speedIncrease);
            newPowerValue = target.AddModifier(powerIncrease);

            AugmentManager.Ins.UpdatePowerGeneratedByAugment(augment, newPowerValue - oldPowerValue);
        }
        else
        {
            target.RemoveModifier(powerIncrease);
            target.AddModifier(speedIncrease);
        }
    }
}
