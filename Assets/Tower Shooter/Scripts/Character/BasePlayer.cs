using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePlayer : BaseCharacter
{
    public float projectileSpeed = 5;

    void Start()
    {
        StartCoroutine(Shooting());
    }

    public void Shoot()
    {
        Projectile projectile = ObjectPoolingEverything.GetFromPool<Projectile>(GameConstants.projectile);

        // 1. Calculate Base Damage
        float baseDamage = stats.GetValue(StatType.Damage);

        // 2. Roll for Critical Strike (e.g. CritChance = 0.25 -> 25%)
        float critChance = stats.GetValue(StatType.CritChance);
        bool isCrit = Random.value < critChance;

        // 3. Apply Crit Multiplier (Default: 175% in LoL)
        float finalRawDamage = isCrit ? baseDamage * 1.75f : baseDamage;

        // 4. Construct Damage Data Payload
        DamageData damagePayload = new DamageData(finalRawDamage, isCrit, gameObject, null);

        float lifesteal = stats.GetValue(StatType.AbilityHaste);

        projectile.Launch(damagePayload, lifesteal, transform.position, Vector2.up, projectileSpeed);
    }

    private IEnumerator Shooting()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);

        yield return waitForSeconds;

        while (true)
        {
            Shoot();

            yield return waitForSeconds;
        }
    }
}
