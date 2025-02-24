using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TimeWaitingLevelCondition : ConditionChangeSkill
{
    //Thua level + Thời gian chưa vượt qua level đó > 12h

    public const string TIME_WAITING_LEVEL = "time_waiting_level_TimeWaitingLevelCondition_skill_system";
    public const string LEVEL_WAITING = "level_waiting_TimeWaitingLevelCondition_skill_system";

    public static int LevelWaiting
    {
        get
        {
            return PlayerPrefs.GetInt(LEVEL_WAITING, -1000);
        }
        set
        {
            PlayerPrefs.SetInt(LEVEL_WAITING, value);
            PlayerPrefs.Save();
        }
    }
    [SerializeField] protected long timeDelay = 129600;//36h

    public static DateTime TimeWaitingLevel
    {
        get
        {
            if (GPlayerPrefs.HasKey(TIME_WAITING_LEVEL))
            {
                var temp = Convert.ToInt64(PlayerPrefs.GetString(TIME_WAITING_LEVEL));
                return DateTime.FromBinary(temp);
            }
            else
            {
                var newDateTime = UnbiasedTime.Instance.Now().AddDays(-1);
                GPlayerPrefs.SetString(TIME_WAITING_LEVEL, newDateTime.ToBinary().ToString());
                GPlayerPrefs.Save();
                return newDateTime;
            }
        }
        set
        {
            GPlayerPrefs.SetString(TIME_WAITING_LEVEL, value.ToBinary().ToString());
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
        if (LevelWaiting <= 0)
        {
            LevelWaiting = UseProfile.CurrentLevel;
            TimeWaitingLevel = UnbiasedTime.Instance.Now();
        }
    }

    public override bool IsActiveCondition()
    {
        if (LevelWaiting > 0)
        {
            if (TimeManager.CaculateTime(UnbiasedTime.Instance.Now(), TimeWaitingLevel) >= timeDelay)
                return true;
        }

        return false;
    }

    public override void EndCondition()
    {
        LevelWaiting = -1000;
        TimeWaitingLevel = UnbiasedTime.Instance.Now();
    }
}
