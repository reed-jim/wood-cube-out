using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class FlyAroundCurrentCheckPointState : StateBase
{
    private CheckPointTran checkPoint;

    private UnityAction actionCome;
    private Vector3 posMiddle;
    private Vector3 posTrans;
    private StandingBase branch;
    private float maxSpeed;

    public override void StartState()
    {
        checkPoint = brain.birdBase.checkPointIn;
        if (checkPoint == null)
            checkPoint = GamePlayController.Instance.level.map.GetCheckPoint(false);

        if (checkPoint != null)
        {
            brain.birdBase.ChangeLayer(checkPoint.indexLayer, LayerGame.BIRD_FLY);
            actionCome = () =>
            {
                brain.ChangeState(FSMBird.StateFSM.Idle);

                if (checkPoint.birdOn != null && checkPoint.birdOn.GetInstanceID() != brain.birdBase.GetInstanceID())
                {
                    checkPoint.birdOn.fsm.ChangeState(FSMBird.StateFSM.FlyToCheckPoint);
                }
                GamePlayController.Instance.level.SetCheckPoint(ref checkPoint, ref brain.birdBase);
                brain.birdBase.ChangeLayer(checkPoint.indexLayer, LayerGame.BIRD);
            };

            posMiddle = checkPoint.posMiddle.position;
            posTrans = checkPoint.posTrans.position;
            branch = checkPoint.branch;
            maxSpeed = brain.birdBase.speedMovePathInGadern;

            brain.birdBase.FlyComePath(
             GamePlayController.Instance.level.map.GetPathGoAround(posMiddle, 3),
             posTrans, actionCome, branch, maxSpeed, typeMove: Ease.Linear);
        }
        else
        {
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
