using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePlayer : BaseCharacter
{
    public float delayPerShot;

    void Start()
    {
        StartCoroutine(Shooting());
    }

    public void Shoot()
    {
        Projectile projectile = ObjectPoolingEverything.GetFromPool<Projectile>(GameConstants.projectile);

        float flySpeed = stats.GetStat(StatType.FlySpeed).Value;

        projectile.Launch(this, StarManager.Ins.Mars, transform.position, flySpeed);
    }

    private IEnumerator Shooting()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);

        yield return waitForSeconds;

        float shootSpeed = stats.GetStat(StatType.AttackSpeed).Value;
        delayPerShot = 1 / shootSpeed;

        while (true)
        {
            Shoot();

            shootSpeed = stats.GetStat(StatType.AttackSpeed).Value;
            delayPerShot = 1 / shootSpeed;

            yield return new WaitForSeconds(delayPerShot);
        }
    }
}
