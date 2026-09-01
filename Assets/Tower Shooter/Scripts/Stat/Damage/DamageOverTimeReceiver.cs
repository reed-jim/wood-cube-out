using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageOverTimeReceiver : MonoBehaviour
{
    private HealthComponent healthComponent;

    private readonly Dictionary<DamageOverTimeType, DamageOverTimeItem> activeDots = new Dictionary<DamageOverTimeType, DamageOverTimeItem>();

    public void ApplyDamageOverTime(DamageOverTimeEffect damageOverTimeEffect, GameObject attacker)
    {
        DamageOverTimeType damageOverTimeType = damageOverTimeEffect.damageOverTimeType;

        if (activeDots.ContainsKey(damageOverTimeType))
        {
            DamageOverTimeItem existingDamageOverTimeItem = activeDots[damageOverTimeType];

            if (existingDamageOverTimeItem.processDamageOverTimeCoroutine != null)
            {
                StopCoroutine(existingDamageOverTimeItem.processDamageOverTimeCoroutine);
            }

            DamageOverTimeItem newDamageOverTimeItem = DamageOverTimeItem.StackDamageOverTime(existingDamageOverTimeItem);

            newDamageOverTimeItem.processDamageOverTimeCoroutine = StartCoroutine(TickingDamageOverTime(newDamageOverTimeItem));
        }
        else
        {
            DamageOverTimeItem newDamageOverTimeItem = new DamageOverTimeItem(damageOverTimeEffect, attacker);

            activeDots.Add(damageOverTimeType, newDamageOverTimeItem);

            newDamageOverTimeItem.processDamageOverTimeCoroutine = StartCoroutine(TickingDamageOverTime(newDamageOverTimeItem));
        }
    }

    private IEnumerator TickingDamageOverTime(DamageOverTimeItem damageOverTimeItem)
    {
        float tickInterval = damageOverTimeItem.damageOverTimeEffect.tickInterval;

        WaitForSeconds waitForSeconds = new WaitForSeconds(tickInterval);

        while (damageOverTimeItem.remaniningTime > 0)
        {
            DamageData damagePayload = new DamageData(
                rawDamage: damageOverTimeItem.damageOverTimeEffect.tickDamage,
                isCritical: false,
                attacker: damageOverTimeItem.attacker,
                target: gameObject
            );

            healthComponent.TakeDamage(damagePayload);

            damageOverTimeItem.remaniningTime -= tickInterval;

            yield return waitForSeconds;
        }

        activeDots.Remove(damageOverTimeItem.damageOverTimeEffect.damageOverTimeType);
    }

    public void ClearDamageOverTime(DamageOverTimeEffect damageOverTimeEffect)
    {
        DamageOverTimeType damageOverTimeType = damageOverTimeEffect.damageOverTimeType;

        if (activeDots.ContainsKey(damageOverTimeType))
        {
            if (activeDots[damageOverTimeType].processDamageOverTimeCoroutine != null)
            {
                StopCoroutine(activeDots[damageOverTimeType].processDamageOverTimeCoroutine);
            }
        }
    }

    public void ClearAllDamageOverTimes()
    {
        foreach (var instance in activeDots.Values)
        {
            if (instance.processDamageOverTimeCoroutine != null)
            {
                StopCoroutine(instance.processDamageOverTimeCoroutine);
            }
        }

        activeDots.Clear();
    }
}

public class DamageOverTimeItem
{
    public DamageOverTimeEffect damageOverTimeEffect;

    public float remaniningTime;
    public int currentStack;
    public float timer;
    public GameObject attacker;

    public Coroutine processDamageOverTimeCoroutine;

    public DamageOverTimeItem()
    {

    }

    public DamageOverTimeItem(DamageOverTimeEffect damageOverTimeEffect, GameObject attacker)
    {
        this.damageOverTimeEffect = damageOverTimeEffect;
        this.attacker = attacker;

        remaniningTime = damageOverTimeEffect.totalDuration;
        currentStack = 0;
        timer = 0;
    }

    public static DamageOverTimeItem StackDamageOverTime(DamageOverTimeItem existingDamageOverTimeItem)
    {
        DamageOverTimeItem damageOverTimeItem = new DamageOverTimeItem();

        damageOverTimeItem.damageOverTimeEffect = existingDamageOverTimeItem.damageOverTimeEffect;
        damageOverTimeItem.remaniningTime += existingDamageOverTimeItem.remaniningTime;
        damageOverTimeItem.currentStack++;
        damageOverTimeItem.timer = 0;

        damageOverTimeItem.attacker = existingDamageOverTimeItem.attacker;

        return damageOverTimeItem;
    }
}
