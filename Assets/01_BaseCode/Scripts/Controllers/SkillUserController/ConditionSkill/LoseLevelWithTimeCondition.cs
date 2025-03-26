using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LoseLevelWithTimeCondition : ConditionChangeSkill
{
    //Thua level + Thời gian chưa vượt qua level đó > 12h

    public const string LAST_TIME_LOSE_LEVEL = "last_time_lose_level_LoseLevelWithTimeCondition_skill_system";
    public const string LEVEL_START_CHECK = "level_start_check_LoseLevelWithTimeCondition_skill_system";

    public static bool isLosed;

    public static int LevelStartCheck
    {
        get
        {
            return PlayerPrefs.GetInt(LEVEL_START_CHECK, -1000);
        }
        set
        {
            PlayerPrefs.SetInt(LEVEL_START_CHECK, value);
            PlayerPrefs.Save();
        }
    }
    [SerializeField] protected long timeDelay = 43200;//12h

    public static DateTime LastTimeLoseLevel
    {
        get
        {
            if (GPlayerPrefs.HasKey(LAST_TIME_LOSE_LEVEL))
            {
                var temp = Convert.ToInt64(PlayerPrefs.GetString(LAST_TIME_LOSE_LEVEL));
                return DateTime.FromBinary(temp);
            }
            else
            {
                var newDateTime = UnbiasedTime.Instance.Now().AddDays(-1);
                GPlayerPrefs.SetString(LAST_TIME_LOSE_LEVEL, newDateTime.ToBinary().ToString());
                GPlayerPrefs.Save();
                return newDateTime;
            }
        }
        set
        {
            GPlayerPrefs.SetString(LAST_TIME_LOSE_LEVEL, value.ToBinary().ToString());
            GPlayerPrefs.Save();
        }
    }


    public override void Init()
    {
        base.Init();
        typeChange = TypeChange.Down;
    }

    public override void AddValue(long value)
    {
        if (LevelStartCheck <= 0)
        {
            LevelStartCheck = UseProfile.CurrentLevel;
            LastTimeLoseLevel = UnbiasedTime.Instance.Now();
        }
    }

    public override bool IsActiveCondition()
    {
        if (LevelStartCheck > 0 && isLosed)
        {
            if (TimeManager.CaculateTime(UnbiasedTime.Instance.Now(), LastTimeLoseLevel) >= timeDelay)
                return true;
        }

        return false;
    }

    public override void EndCondition()
    {
        LevelStartCheck = -1000;
        LastTimeLoseLevel = UnbiasedTime.Instance.Now();
    }
}
