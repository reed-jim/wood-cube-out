using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointTran : MonoBehaviour
{
    public Transform posStart;
    public Transform posMiddle;
    public Transform posTrans;

    public bool isSlotInBranch;
    [HideInInspector] public StandingBase branch;
    public BirdBase birdOn;

    public bool isSlotCanMove;
    public Transform posMoveLeft;
    public Transform posMoveRight;

    public int indexLayer;
    [HideInInspector] public int id;

    public List<CheckPointTran> checkPointsCover;

    public bool IsEmpty
    {
        get
        {
            if (birdOn != null)
                return false;
            else
                return true;
        }
    }

    public void Init(int id)
    {
        this.id = id;
    }    

    public bool IsOutScreen
    {
        get
        {
            return !GamePlayController.Instance.level.map.IsCheckPointInLimit(this.transform.position);
        }
    }    
}
