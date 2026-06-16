using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ConditionChangeSkill : MonoBehaviour
{
    public enum TypeCondition
    {
        StarAfterPassLevel = 0,
        WinLevelNoUseItem = 1,
        NumberLoseLevel = 2,
        TimeWaitingLevel = 3,
        LoseLevelWithTime = 4//Thua level + Thời gian chưa vượt qua level đó > x giờ
    }

    public enum TypeChange
    {
        Up = 0,
        Down = 1
    }

    [HideInInspector] public TypeChange typeChange;
    public TypeCondition typeCondition;
    public int priovirty;
    public bool isEndManual;//End condition ko theo hệ thống (theo sự kiện tuỳ chỉnh)
    public virtual void Init()
    {

    }

    public abstract void AddValue(long value);

    public abstract bool IsActiveCondition();

    public abstract void EndCondition();
}
