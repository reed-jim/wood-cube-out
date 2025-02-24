using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerRace : MemberRace
{
    public override int RememberCurrentLevelPass
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.REMEMBER_CURRENT_LEVEL_PASS_ME, 0);
        }
        set
        {
            GPlayerPrefs.SetInt(StringHelper.REMEMBER_CURRENT_LEVEL_PASS_ME, value);
            GPlayerPrefs.Save();
        }
    }

    public override int CurrentLevelPass
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.CURRENT_LEVEL_PASS_ME, 0);
        }
        set
        {
            GPlayerPrefs.SetInt(StringHelper.CURRENT_LEVEL_PASS_ME, value);
            GPlayerPrefs.Save();
        }
    }

    public override bool IsFinished
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.IS_FINISHED_RACE + "me", 0) == 1;
        }
        set
        {
            GPlayerPrefs.SetInt(StringHelper.IS_FINISHED_RACE + "me", value ? 1 : 0);
            GPlayerPrefs.Save();
        }
    }

    public override string nameMember 
    { 
        get
        {
            return UseProfile.NamePlayer;
        }
        set
        {

        }
    }
    public override string linkFlag
    {
        get
        {
            return UseProfile.FlagLink;
        }
        set
        {

        }
    }

    public DateTime TimeFinshRace
    {
        get
        {
            if (GPlayerPrefs.HasKey(StringHelper.TIME_ME_FINISH_RACE_RACE_CHALLENGE))
            {
                var temp = Convert.ToInt64(PlayerPrefs.GetString(StringHelper.TIME_ME_FINISH_RACE_RACE_CHALLENGE));
                return DateTime.FromBinary(temp);
            }
            else
            {
                var newDateTime = UnbiasedTime.Instance.Now().AddDays(-1);
                GPlayerPrefs.SetString(StringHelper.TIME_ME_FINISH_RACE_RACE_CHALLENGE, newDateTime.ToBinary().ToString());
                GPlayerPrefs.Save();
                return newDateTime;
            }
        }
        set
        {
            GPlayerPrefs.SetString(StringHelper.TIME_ME_FINISH_RACE_RACE_CHALLENGE, value.ToBinary().ToString());
            GPlayerPrefs.Save();
        }
    }

    public override void ResetData()
    {
        base.ResetData();
        CurrentLevelPass = 0;
    }

    public override void SetData()
    {
        if (CurrentLevelPass >= RaceSpaceChallenge.NumLevelPassToWinEvent)
        {
            totalTimeFinishRace = TimeManager.CaculateTime(RaceSpaceChallenge.TimeStartRace, TimeFinshRace);
        }
        else
        {
            totalTimeFinishRace = -1;
        }    
    }
}
