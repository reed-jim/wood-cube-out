using System;
using UnityEngine;

public static class DailyRewardUtil
{
    public static bool IsClaimedTodayReward(int day)
    {
        int lastDay = DataUtility.Load(GameConstants.LAST_DAY_CLAIM_DAILY_REWARD, -1);
        int numDayClaimedReward = DataUtility.Load(GameConstants.NUM_DAY_CLAIMED_REWARD, 0);
        int today = DateTime.Now.Day;

        if (today == lastDay)
        {
            return true;
        }

        if (numDayClaimedReward % 7 == day - 1)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public static int GetTodayRewardIndex()
    {
        return DateTime.Now.Day & 7;
    }
}
