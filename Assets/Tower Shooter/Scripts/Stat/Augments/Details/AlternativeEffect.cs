using UnityEngine;

[CreateAssetMenu(fileName = "AlternativeEffect", menuName = "Stats/Effects/AlternativeEffect")]
public class AlternativeEffect : AugmentEffect
{
    public StatModifierData powerIncreaseData;
    public StatModifierData speedIncreaseData;

    private StatModifier powerIncrease;
    private StatModifier speedIncrease;

    public override void ApplyEffect(CharacterStats target, object source)
    {

    }

    // even cycle increase power, odd cycle increase speed
    public override void ApplyEffectOnCycleCompleted(CharacterStats target)
    {
        BaseCharacter node = target.serviceLocator.character;

        if (powerIncrease == null || powerIncrease.Value == 0)
        {
            powerIncrease = new StatModifier(powerIncreaseData.statType, powerIncreaseData.modType, powerIncreaseData.value);
        }

        if (speedIncrease == null || speedIncrease.Value == 0)
        {
            speedIncrease = new StatModifier(speedIncreaseData.statType, speedIncreaseData.modType, speedIncreaseData.value);
        }

        if (node.currentCycle % 2 == 0)
        {
            target.RemoveModifier(speedIncrease);
            target.AddModifier(powerIncrease);
        }
        else
        {
            target.RemoveModifier(powerIncrease);
            target.AddModifier(speedIncrease);
        }
    }

    public override void RemoveEffect(CharacterStats target, object source)
    {

    }
}
