using UnityEngine;

public class BaseBullet : MonoBehaviour
{
    public Rigidbody bulletRigidbody;

    public DamageInflicter damageInflicter;

    public float initialVelocity;

    public void Init(BaseRangedWeapon rangedWeapon)
    {

    }

    protected virtual void Shoot()
    {
        Vector3 direction = transform.position;

        bulletRigidbody.AddForce(direction, ForceMode.Impulse);
    }
}
