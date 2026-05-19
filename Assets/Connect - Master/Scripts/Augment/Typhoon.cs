using UnityEngine;

public class Typhoon : AugmentItem
{
    public int requiredAttackTime;

    public override void ApplySpecialEffect(BaseCharacter character)
    {
        if (character.attackTime > 0 && character.attackTime % requiredAttackTime == 0)
        {
            character.Attack();
        }
    }

    public override string GetDescription()
    {
        return $"Apply 1 more time after {requiredAttackTime} attacks";
    }
}
