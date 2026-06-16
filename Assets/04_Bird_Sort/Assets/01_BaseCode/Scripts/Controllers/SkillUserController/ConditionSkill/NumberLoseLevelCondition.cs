using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberLoseLevelCondition : ConditionChangeSkill
{
    public const string LEVEL_START_CHECK = "level_start_check__NumberLoseLevelCondition_skill_system";

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
    [SerializeField] protected int levelCheck = 3;

    public override void Init()
    {
        base.Init();
        typeChange = TypeChange.Down;
    }

    public override void AddValue(long value)
    {
        if (LevelStartCheck <= 0)
            LevelStartCheck = 1;
        else
            LevelStartCheck++;
    }

    public override bool IsActiveCondition()
    {
        if (LevelStartCheck >= levelCheck)
        {
            return true;
        }

        return false;
    }

    public override void EndCondition()
    {
        LevelStartCheck = -1000;
    }
}
