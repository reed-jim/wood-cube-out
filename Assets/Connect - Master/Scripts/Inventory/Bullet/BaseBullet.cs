using UnityEngine;

public class BaseBullet : MonoBehaviour
{
    public Rigidbody bulletRigidbody;

    public DamageInflicter damageInflicter;

    public float initialVelocity;
    public float diameter;

    public void Init(BaseRangedWeapon rangedWeapon)
    {

    }

    public virtual void Shoot()
    {
        Vector3 direction = transform.forward;

        bulletRigidbody.linearVelocity = initialVelocity * direction;

        damageInflicter = new DamageInflicter();

        damageInflicter.damageInterval = 0;
        damageInflicter.damagePerCycle = initialVelocity * diameter;
    }
}
