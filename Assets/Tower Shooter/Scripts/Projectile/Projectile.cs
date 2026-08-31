using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private Rigidbody2D projectileRigidbody;

    private DamageData damageData;
    private float lifestealPercent;

    public void Initialize(DamageData data, float lifesteal)
    {
        damageData = data;
        lifestealPercent = lifesteal;
    }

    public void Launch(DamageData data, float lifesteal, Vector2 origin, Vector2 direction, float force)
    {
        damageData = data;
        lifestealPercent = lifesteal;
        
        // Instant 2D physics impulse launch
        projectileRigidbody.AddForce(direction.normalized * force, ForceMode2D.Impulse);
        
        // Orient projectile along movement direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.position = origin;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore self-collision
        if (other.gameObject == damageData.Attacker) return;

        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            // Cache target reference
            damageData.Target = other.gameObject;

            // Deal damage
            damageable.TakeDamage(damageData);

            // Handle Lifesteal
            if (lifestealPercent > 0 && damageData.Attacker != null)
            {
                if (damageData.Attacker.TryGetComponent<HealthComponent>(out var attackerHealth))
                {
                    // Lifesteal calculated from raw damage before armor mitigation
                    float healAmount = damageData.RawDamage * lifestealPercent;
                    attackerHealth.Heal(healAmount);
                }
            }

            ObjectPoolingEverything.ReturnToPool(GameConstants.projectile, gameObject);
        }
    }
}
