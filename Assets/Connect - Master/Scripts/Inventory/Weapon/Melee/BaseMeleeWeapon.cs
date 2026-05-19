using System.Collections.Generic;
using UnityEngine;

public class BaseMeleeWeapon : BaseWeapon
{
    [SerializeField] private Collider weaponCollider;

    public List<BaseCharacter> hitTargets;

    void OnCollisionEnter(Collision collision)
    {
        BaseCharacter character = collision.gameObject.GetComponent<BaseCharacter>();

        if (character != null)
        {
            if (!hitTargets.Contains(character))
            {
                hitTargets.Add(character);
            }
        }
    }

    public override void Attack(BaseCharacter attacker)
    {
        weaponCollider.gameObject.SetActive(true);

        attacker.characterServiceLocator.characterAnimation.PlayAnimation(onComplete: () =>
        {
            weaponCollider.gameObject.SetActive(false);

            hitTargets = new List<BaseCharacter>();
        });
    }
}
