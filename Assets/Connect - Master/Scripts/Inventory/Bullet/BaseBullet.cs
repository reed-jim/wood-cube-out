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

        // Plane at y = 0
        Plane plane = new Plane(Vector3.up, Vector3.zero);

        float distance;

        if (plane.Raycast(ray, out distance))
        {
            InitDamageInflicter();

            Vector3 targetPoint = ray.GetPoint(distance);

            Vector3 direction = (targetPoint - transform.position).normalized;

            bulletRigidbody.AddForce(initialVelocity * direction, ForceMode.Impulse);
        }
    }

    private void InitDamageInflicter()
    {
        damageInflicter = new DamageInflicter();

        damageInflicter.damageInterval = 0;
        damageInflicter.damagePerCycle = initialVelocity * diameter;
    }
}
