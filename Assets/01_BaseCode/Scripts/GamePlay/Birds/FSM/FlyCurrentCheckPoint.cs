using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class FlyCurrentCheckPoint : StateBase
{
    [SerializeField] private int percentFlyPath = 50;

    private CheckPointTran checkPoint;

    private UnityAction actionCome;
    private Vector3 posMiddle;
    private Vector3 posTrans;
    private StandingBase branch;
    private float maxSpeed;

    public override void StartState()
    {
        checkPoint = brain.birdBase.checkPointIn;

        if(UseProfile.CurrentLevel <= 1)
        {
            brain.birdBase.PlayAnimIdle();
            brain.ChangeState(FSMBird.StateFSM.Idle);
            return;
        }    


        if (!brain.birdBase.isHasExtral)
        {
            if (checkPoint != null && Vector2.Distance(brain.birdBase.transform.position, checkPoint.posTrans.position) >= 0.05f)
            {
                brain.ChangeState(FSMBird.StateFSM.FlyAroundCheckPoint);
            }
            else
            {
                int randomFly = Random.Range(0, 100);
                if (randomFly <= 5)
                {
                    brain.ChangeState(FSMBird.StateFSM.FlyToCheckPoint);
                }
                else
                {
                    brain.birdBase.PlayAnimIdle();
                    brain.ChangeState(FSMBird.StateFSM.Idle);
                }

            }
        }
        else
        {
            brain.birdBase.PlayAnimIdle();
            brain.ChangeState(FSMBird.StateFSM.Idle);
        }    
    }

    public override void UpdateState()
    {

    }
    public override void EndState()
    {

    }
}
