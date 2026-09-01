using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DamageOverTimeType
{
    Burn,
    Bleed,
    Poison
}

[CreateAssetMenu(fileName = "DamageOverTimeEffect", menuName = "Stats/Effects/DamageOverTimeEffect")]
public class DamageOverTimeEffect : AugmentEffect
{
    public DamageOverTimeType damageOverTimeType;

    [Header("Tick Settings")]
    public float totalDuration = 3f;   // Total lifetime in seconds
    public float tickInterval = 0.5f;   // Deals damage every 0.5s
    public float tickDamage = 10f;      // Base raw damage per tick

    [Header("Stacking Rules")]
    public int maxStacks = 3;
    public bool refreshDurationOnReapply = true;

    public override void ApplyEffect(CharacterStats target, object source)
    {
        // target.characterServiceLocator.damageOverTimeReceiver.ApplyDamageOverTime(this, (GameObject)source);
    }

    public override void RemoveEffect(CharacterStats target, object source)
    {
        // target.characterServiceLocator.damageOverTimeReceiver.ClearDamageOverTime(this);
    }
}
