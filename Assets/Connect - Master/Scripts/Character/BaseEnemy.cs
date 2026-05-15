using UnityEngine;

public class BaseEnemy : BaseCharacter
{
    public Animation animationController;

    private void MoveTo()
    {

    }

    private void Attack()
    {
        animationController.Play("");
    }
}
