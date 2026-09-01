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

        projectile.Launch(this, transform.position, Vector2.up, projectileSpeed);
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
