using UnityEngine;

public enum DamageType
{
    Normal,
    True
}

public struct DamageContext
{
    public GameObject Attacker;
    public GameObject Target;

    public float RawDamage;
    public DamageType DamageType;
}

public interface IDamageCalculator
{
    float Calculate(DamageContext context);
}
