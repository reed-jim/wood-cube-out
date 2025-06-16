using System;
using UnityEngine;

public static class DailyRewardUtil
{
    public static bool IsDailyRewardReadyForDay(int day)
    {
        if (day == CurrentDailyRewardIndex() && IsNewDay())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static int CurrentDailyRewardIndex()
    {
        if (!GamePersistentVariable.isCurrentDailyRewardIndexLoaded)
        {
            int index = DataUtility.Load($"current_daily_reward_day", 1) % 7;

            GamePersistentVariable.currentDailyRewardIndex = index;
            GamePersistentVariable.isCurrentDailyRewardIndexLoaded = true;

            return index;
        }
        else
        {
            return GamePersistentVariable.currentDailyRewardIndex;
        }
    }

    public static bool IsNewDay()
    {
        long lastTimestampClaimDailyReward = DataUtility.Load<long>("last_timestamp_claim_daily_reward", 0);

        if (lastTimestampClaimDailyReward == 0)
        {
            return true;
        }
        else
        {
            long currentTimestamp = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();

            DateTimeOffset firstOffset = DateTimeOffset.FromUnixTimeSeconds(lastTimestampClaimDailyReward).Date;
            DateTimeOffset secondOffset = DateTimeOffset.FromUnixTimeSeconds(currentTimestamp).Date;

            return firstOffset != secondOffset;
        }
    }

    public static bool IsClaimedDailyReward(int day)
    {
        return DataUtility.Load($"is_claim_daily_reward_day_{day}", false);
    }

    public static bool IsClaimedTodayDailyReward()
    {
        return DataUtility.Load("is_claim_today_daily_reward", false);
    }
}
