using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SkillUserSegemntData;

public class SkillUserController : MonoBehaviour
{
    [System.Serializable]
    public class SkillUserData
    {
        public float coNumber;//Hệ số nhân số lượng object trong level
        public float coPercentCover;//% giảm độ che phủ
    }

    public static int CurrentSkill
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.CURRENT_SKILL_USER, 5);
        }
        set
        {
            GPlayerPrefs.SetInt(StringHelper.CURRENT_SKILL_USER, value);
            GPlayerPrefs.Save();
        }
    }

    public List<ConditionChangeSkill> conditions;
    [HideInInspector] public SkillUserConfig config;

    public bool IsActive
    {
        get
        {
            return (config.isActive && UseProfile.CurrentLevel >= config.level_start);
        }
    }    

    public void Init()
    {
        for (int i = 0; i < conditions.Count; i++)
        {
            conditions[i].Init();
        }

        config = GameController.Instance.segmentController.GetCurrentSegment().skillUserData.config;

        conditions = Helper.SelectionSortListMono(conditions, (a, b) =>
        {
            if ((int)(((ConditionChangeSkill)a).priovirty) < (int)(((ConditionChangeSkill)b).priovirty))
                return 1;
            else
                return -1;
        });
    }

    public SkillUserData GetCurrentSkillData()
    {
        if(!IsActive)
        {
            CurrentSkill = 5;
            return config.datas[CurrentSkill];
        }    

        if (CurrentSkill <= 0)
            CurrentSkill = 5;//For sync data lỗi

        if (CurrentSkill >= config.datas.Length)
            CurrentSkill = config.datas.Length - 1;

        return config.datas[CurrentSkill];
    }

    public void AddValueCondition(ConditionChangeSkill.TypeCondition typeCondition, long value)
    {
        if (!IsActive)
        {
            return;
        }

        for (int i = 0; i < conditions.Count; i++)
        {
            if (conditions[i].typeCondition == typeCondition)
            {
                conditions[i].AddValue(value);
                break;
            }
        }
    }

    public ConditionChangeSkill GetCondition(ConditionChangeSkill.TypeCondition typeCondition)
    {
        for (int i = 0; i < conditions.Count; i++)
        {
            if (conditions[i].typeCondition == typeCondition)
            {
                return conditions[i];
            }
        }

        return null;
    }


    /// <summary>
    /// Gọi khi bắt đầu vào level
    /// </summary>
    public void CheckCurrentSkill()
    {
        if (!IsActive)
        {
            return;
        }

        for (int i = 0; i < conditions.Count; i++)
        {
            if (conditions[i].IsActiveCondition())
            {
                if (conditions[i].typeChange == ConditionChangeSkill.TypeChange.Up)
                {
                    CurrentSkill++;
                    if (CurrentSkill >= config.datas.Length)
                        CurrentSkill = config.datas.Length - 1;
                }
                else
                {
                    CurrentSkill--;
                    if (CurrentSkill <= 0)
                        CurrentSkill = 1;
                }
                ResetAllCondition();
                break;
            }
        }
    }

    /// <summary>
    /// Gọi khi win level
    /// </summary>
    public void CheckReDefaultCurrentSkill()
    {
        if (CurrentSkill < 5)
        {
            CurrentSkill = 5;
            ResetAllCondition();
        }
    }    

    public void ResetAllCondition()
    {
        for (int i = 0; i < conditions.Count; i++)
        {
            if (!conditions[i].isEndManual)
                conditions[i].EndCondition();
        }
    }
}
