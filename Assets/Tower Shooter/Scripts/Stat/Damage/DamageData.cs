using UnityEngine;

public struct DamageData
{
    public float RawDamage;
    public bool IsCritical;
    public GameObject Attacker;
    public GameObject Target;

    public DamageData(float rawDamage, bool isCritical, GameObject attacker, GameObject target)
    {
        RawDamage = rawDamage;
        IsCritical = isCritical;
        Attacker = attacker;
        Target = target;
    }
}
