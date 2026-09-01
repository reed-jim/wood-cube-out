using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private Rigidbody2D projectileRigidbody;

    private BaseCharacter attacker;
    private float lifestealPercent;

    public void Launch(BaseCharacter attacker, Vector2 origin, Vector2 direction, float force)
    {
        this.attacker = attacker;

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
        if (other.gameObject == attacker) return;

        #region Direct Damage
        if (other.TryGetComponent<IDamageable>(out var damageable))
        {

            DamageData damageData = GetDamageData();

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
        #endregion

        #region Damage Over Time
        BaseCharacter character = other.GetComponent<BaseCharacter>();

        if (character != null)
        {
            for (int i = 0; i < attacker.ownedAugments.Count; i++)
            {
                Augment augment = attacker.ownedAugments[i];

                if (augment.dynamicEffects.Count > 0)
                {
                    for (int j = 0; j < augment.dynamicEffects.Count; j++)
                    {
                        AugmentEffect augmentEffect = augment.dynamicEffects[j];

                        if (augmentEffect is DamageOverTimeEffect)
                        {
                            character.serviceLocator.damageOverTimeReceiver.ApplyDamageOverTime((DamageOverTimeEffect)augmentEffect, attacker.gameObject);
                        }
                    }
                }
            }
        }
        #endregion
    }

    public DamageData GetDamageData()
    {
        // 1. Calculate Base Damage
        float baseDamage = attacker.stats.GetValue(StatType.Damage);

        // 2. Roll for Critical Strike (e.g. CritChance = 0.25 -> 25%)
        float critChance = attacker.stats.GetValue(StatType.CritChance);
        bool isCrit = Random.value < critChance;

        // 3. Apply Crit Multiplier (Default: 175% in LoL)
        float finalRawDamage = isCrit ? baseDamage * 1.75f : baseDamage;

        // 4. Construct Damage Data Payload
        DamageData damagePayload = new DamageData(finalRawDamage, isCrit, gameObject, null);

        float lifesteal = attacker.stats.GetValue(StatType.AbilityHaste);

        return damagePayload;
    }
}
