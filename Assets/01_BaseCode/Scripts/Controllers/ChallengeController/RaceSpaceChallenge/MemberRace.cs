using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MemberRace : MonoBehaviour
{
    [System.Serializable]
    public class MemberRaceDataSync
    {
        public string nameMember;

        public string linkFlag;

        public bool IsFinished;

        public int RememberCurrentLevelPass;

        public int CurrentLevelPass;

        public string ProgessRaceSave;

        public long TimeFinshRace;

        public static MemberRaceDataSync GetData(int id, bool isMe)
        {
            MemberRaceDataSync dt = new MemberRaceDataSync();

            if (isMe)
            {
                dt.IsFinished = PlayerPrefs.GetInt(StringHelper.IS_FINISHED_RACE + "me", 0) == 1;
                dt.RememberCurrentLevelPass = PlayerPrefs.GetInt(StringHelper.REMEMBER_CURRENT_LEVEL_PASS_ME, 0);
                dt.CurrentLevelPass = PlayerPrefs.GetInt(StringHelper.CURRENT_LEVEL_PASS_ME, 0);
                if (GPlayerPrefs.HasKey(StringHelper.TIME_ME_FINISH_RACE_RACE_CHALLENGE))
                {
                    var temp = Convert.ToInt64(PlayerPrefs.GetString(StringHelper.TIME_ME_FINISH_RACE_RACE_CHALLENGE));
                    dt.TimeFinshRace = temp;
                }
                else
                {
                    var newDateTime = UnbiasedTime.Instance.Now().AddDays(-1);
                    dt.TimeFinshRace = newDateTime.ToBinary();
                }
            }   
            else
            {
                dt.nameMember = PlayerPrefs.GetString(StringHelper.NAME_BOSS + id, "");
                dt.linkFlag = PlayerPrefs.GetString(StringHelper.LINK_FLAG_BOSS + id, "");
                dt.IsFinished = PlayerPrefs.GetInt(StringHelper.IS_FINISHED_RACE + id, 0) == 1;
                dt.RememberCurrentLevelPass = PlayerPrefs.GetInt(StringHelper.REMEMBER_CURRENT_LEVEL_PASS_BOSS + id, 0);
                dt.CurrentLevelPass = PlayerPrefs.GetInt(StringHelper.CURRENT_LEVEL_PASS_BOSS + id, 0);
                dt.ProgessRaceSave = PlayerPrefs.GetString(StringHelper.PROGESS_BOSS_RACE_CHALLENGE + id, "");
            }

            return dt;
        }    
        public static void SetData(int id, bool isMe, MemberRaceDataSync dataSync)
        {
            if (isMe)
            {
                GPlayerPrefs.SetInt(StringHelper.IS_FINISHED_RACE + "me", dataSync.IsFinished ? 1 : 0);
                GPlayerPrefs.SetInt(StringHelper.REMEMBER_CURRENT_LEVEL_PASS_ME, dataSync.RememberCurrentLevelPass);
                GPlayerPrefs.SetInt(StringHelper.CURRENT_LEVEL_PASS_ME, dataSync.CurrentLevelPass);
                GPlayerPrefs.SetString(StringHelper.TIME_ME_FINISH_RACE_RACE_CHALLENGE, dataSync.TimeFinshRace.ToString());
            }   
            else
            {
                GPlayerPrefs.SetString(StringHelper.NAME_BOSS + id, dataSync.nameMember);
                GPlayerPrefs.SetString(StringHelper.LINK_FLAG_BOSS + id, dataSync.linkFlag);
                GPlayerPrefs.SetInt(StringHelper.IS_FINISHED_RACE + id, dataSync.IsFinished ? 1 : 0);
                GPlayerPrefs.SetInt(StringHelper.REMEMBER_CURRENT_LEVEL_PASS_BOSS + id, dataSync.RememberCurrentLevelPass);
                GPlayerPrefs.SetInt(StringHelper.CURRENT_LEVEL_PASS_BOSS + id, dataSync.CurrentLevelPass);
                GPlayerPrefs.SetString(StringHelper.PROGESS_BOSS_RACE_CHALLENGE + id, dataSync.ProgessRaceSave);
            }
        }    
    }


    public abstract string nameMember
    {
        get;
        set;
    }

    public abstract string linkFlag
    {
        get;
        set;
    }

    public abstract bool IsFinished
    {
        get;
        set;
    }

    public abstract int RememberCurrentLevelPass
    {
        get;
        set;
    }

    public abstract int CurrentLevelPass
    {
        get;
        set;
    }

    public float totalTimeFinishRace;
    public bool isMe;
    

    public int ID;

    public abstract void SetData();

    public virtual void ResetData()
    {
        IsFinished = false;
       RememberCurrentLevelPass = 0;
       CurrentLevelPass = 0;
    }
}
