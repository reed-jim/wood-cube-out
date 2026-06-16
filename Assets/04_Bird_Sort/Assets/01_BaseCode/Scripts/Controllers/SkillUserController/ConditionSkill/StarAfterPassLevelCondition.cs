using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Range
{
    public float min;
    public float max;
}

public class StarAfterPassLevelCondition : ConditionChangeSkill
{
    public const string NUM_STAR_CLAIMED = "num_star_claimed_StarAfterPassLevelCondition_skill_system";
    public const string LEVEL_START_CHECK = "level_start_check_StarAfterPassLevelCondition_skill_system";

    public static int NumStarClaimed
    {
        get
        {
            return PlayerPrefs.GetInt(NUM_STAR_CLAIMED, 0);
        }
        set
        {
            PlayerPrefs.SetInt(NUM_STAR_CLAIMED, value);
            PlayerPrefs.Save();
        }
    }

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
    [SerializeField] protected Range rangeUpSkill;
    [SerializeField] protected Range rangeDownSkill;

    public override void AddValue(long value)
    {
        if (LevelStartCheck <= 0)
            LevelStartCheck = 1;
        else
            LevelStartCheck++;

        NumStarClaimed += (int)value;
    }

    public override void EndCondition()
    {
        NumStarClaimed = 0;
        LevelStartCheck = -10000;
    }

    public override bool IsActiveCondition()
    {
        if (LevelStartCheck >= levelCheck)
        {
            if (NumStarClaimed >= (int)rangeUpSkill.min && NumStarClaimed <= (int)rangeUpSkill.max)
            {
                typeChange = TypeChange.Up;
                return true;
            }

            if (NumStarClaimed >= (int)rangeDownSkill.min && NumStarClaimed <= (int)rangeDownSkill.max)
            {
                typeChange = TypeChange.Down;
                return true;
            }
        }

        return false;
    }

}
