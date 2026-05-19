using System.Collections.Generic;
using UnityEngine;

public class AugmentItem : MonoBehaviour
{
    public CharacterStat bonusStat;
    public CharacterStat multiplyStat;

    public List<DamageInflicter> damageInflicters;

    public bool isApplied;
    public int level;
    public bool isInvisible;

    public CharacterStat GetBonusStat()
    {
        return bonusStat;
    }

    public CharacterStat GetMultiplyStat()
    {
        return multiplyStat;
    }

    public List<DamageInflicter> GetDamageInflicters()
    {
        return damageInflicters;
    }

    public virtual void Effect()
    {

    }

    public virtual void ApplySpecialEffect(BaseCharacter character)
    {

    }

    public virtual string GetDescription()
    {
        return "level";
    }
}
