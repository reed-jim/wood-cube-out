using UnityEngine;

public class BaseEnemy : BaseCharacter
{
    public Animation animationController;

    public int level;

    private void MoveTo()
    {

    }

    public override void Die()
    {
        base.Die();

        CharacterManager.Ins.mainCharacter.EarnExp(level);
    }

    // private void Attack()
    // {
    //     animationController.Play("");
    // }
}
