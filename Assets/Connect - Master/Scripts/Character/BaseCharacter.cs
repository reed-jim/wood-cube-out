using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BaseCharacter : MonoBehaviour
{
    public float health;

    public List<DamageInflicter> damageInflicters;

    void Update()
    {
        ApplyDamage();
    }

    void OnCollisionEnter(Collision collision)
    {
        DamageInflicter damageInflicter = collision.gameObject.GetComponent<DamageInflicter>();

        damageInflicters.Add(damageInflicter);
    }

    private void ApplyDamage()
    {
        float currentTime = Time.time;

        for (int i = 0; i < damageInflicters.Count; i++)
        {
            DamageInflicter damageInflicter = damageInflicters[i];

            if (damageInflicter.IsNewCycleToApplyDamage(currentTime))
            {
                health -= damageInflicter.damagePerCycle;

                damageInflicter.UpdateNewDamageCycle(currentTime);
            }
        }

        damageInflicters = damageInflicters.Where(item => !item.IsExpire()).ToList();
    }
}
