using UnityEngine;

public static class StatUtil
{
    public static bool IsPercentStat(StatType statType)
    {
        if(statType == StatType.CritChance)
        {
            return true;
        }

        return false;
    }
}
