using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceBoss : MemberRace
{
    public int idBoss;
    public string ProgessRaceSave
    {
        get
        {
            return PlayerPrefs.GetString(StringHelper.PROGESS_BOSS_RACE_CHALLENGE + idBoss, "");
        }
        set
        {
            GPlayerPrefs.SetString(StringHelper.PROGESS_BOSS_RACE_CHALLENGE + idBoss, value);
            GPlayerPrefs.Save();
        }
    }

    public override int RememberCurrentLevelPass
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.REMEMBER_CURRENT_LEVEL_PASS_BOSS + idBoss, 0);
        }
        set
        {
            GPlayerPrefs.SetInt(StringHelper.REMEMBER_CURRENT_LEVEL_PASS_BOSS + idBoss, value);
            GPlayerPrefs.Save();
        }
    }

    public override int CurrentLevelPass
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.CURRENT_LEVEL_PASS_BOSS + idBoss, 0);
        }
        set
        {
            GPlayerPrefs.SetInt(StringHelper.CURRENT_LEVEL_PASS_BOSS + idBoss, value);
            GPlayerPrefs.Save();
        }
    }

    public override bool IsFinished
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.IS_FINISHED_RACE + idBoss, 0) == 1;
        }
        set
        {
            GPlayerPrefs.SetInt(StringHelper.IS_FINISHED_RACE + idBoss, value ? 1 : 0);
            GPlayerPrefs.Save();
        }
    }

    public override string nameMember
    {
        get
        {
            return PlayerPrefs.GetString(StringHelper.NAME_BOSS + idBoss, "");
        }
        set
        {
            GPlayerPrefs.SetString(StringHelper.NAME_BOSS + idBoss, value);
            GPlayerPrefs.Save();
        }
    }
    public override string linkFlag
    {
        get
        {
            return PlayerPrefs.GetString(StringHelper.LINK_FLAG_BOSS + idBoss, "");
        }
        set
        {
            GPlayerPrefs.SetString(StringHelper.LINK_FLAG_BOSS + idBoss, value);
            GPlayerPrefs.Save();
        }
    }

    private RaceBossController controller;
    private ProgessRaceBossData progessPlay;

    public void Init(RaceBossController controller)
    {
        this.controller = controller;
    }

    public void InitStateDataBoss()
    {
        if (ProgessRaceSave != "")
        {
            progessPlay = JsonUtility.FromJson<ProgessRaceBossData>(ProgessRaceSave);

            if (progessPlay != null)
            {
                CurrentLevelPass = GetCurrentLevelBoss(controller.controller.timerRaccing);
                if (CurrentLevelPass < RaceSpaceChallenge.NumLevelPassToWinEvent)
                {
                    RememberCurrentLevelPass = CurrentLevelPass;
                }
                SetData();
            }
        }
    }

    public void StartEvent(int tier)
    {
        ResetData();
        var data = this.controller.data.GetDataTier(tier);
        data.SetTotalTimePassEvent();
        ProgessRaceSave = data.ToJsonProgess();
        SetData();
    }

    public int GetCurrentLevelBoss(float time)
    {
        if (progessPlay != null)
        {
            for (int i = progessPlay.data.Count - 1; i >= 0; i--)
            {
                if (time >= progessPlay.data[i].timer)
                {
                    return progessPlay.data[i].level;
                }
            }
        }

        return 0;
    }

    public override void ResetData()
    {
        base.ResetData();
        ProgessRaceSave = "";
        RememberCurrentLevelPass = 0;
        CurrentLevelPass = 0;
    }

    public override void SetData()
    {
        if (progessPlay != null)
        {
            totalTimeFinishRace = progessPlay.totalTimePassEvent;
        }
    }
}
