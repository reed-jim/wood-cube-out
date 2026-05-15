using UnityEngine;

public enum DamageType
{

}

public class DamageInflicter : MonoBehaviour
{
    public DamageType damageType;
    public int totalDamageCycle;
    public float damagePerCycle;
    public float damageInterval;

    public int currentDamageCycle;
    public float lastApplyDamageTime;

    public bool IsNewCycleToApplyDamage(float currentTime)
    {
        return currentTime > lastApplyDamageTime + damageInterval;
    }

    public void UpdateNewDamageCycle(float currentTime)
    {
        currentDamageCycle++;
        lastApplyDamageTime = currentTime;
    }

    public bool IsExpire()
    {
        return currentDamageCycle >= totalDamageCycle;
    }
}
