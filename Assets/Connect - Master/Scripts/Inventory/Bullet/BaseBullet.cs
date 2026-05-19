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
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100f);
        }

        InitDamageInflicter();

        Vector3 direction = (targetPoint - transform.position).normalized;

        bulletRigidbody.AddForce(initialVelocity * direction, ForceMode.Impulse);
    }

    private void InitDamageInflicter()
    {
        damageInflicter.damageInterval = 0;
        damageInflicter.damagePerCycle = initialVelocity * diameter * 0.1f;
    }
}
