using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class FlyCheckPointState : StateBase
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
        checkPoint = GamePlayController.Instance.level.map.GetCheckPoint(true);
        if (checkPoint != null)
        {
            GamePlayController.Instance.level.SetCheckPoint(ref checkPoint, ref brain.birdBase);
            brain.birdBase.ChangeLayer(checkPoint.indexLayer, LayerGame.BIRD_FLY);

            actionCome = () =>
            {
                brain.ChangeState(FSMBird.StateFSM.Idle);
                brain.birdBase.ChangeLayer(checkPoint.indexLayer, LayerGame.BIRD);
            };

            posMiddle = checkPoint.posMiddle.position;
            posTrans = checkPoint.posTrans.position;
            branch = checkPoint.branch;


            Ease easy = Ease.OutSine;
            int random = Random.Range(1, 6);
            if (random == 1)
                easy = Ease.OutQuad;
            else if (random == 2)
                easy = Ease.OutQuart;
            else if (random == 3)
                easy = Ease.OutFlash;
            else if (random == 4)
                easy = Ease.InOutFlash;

            if (Vector2.Distance(brain.birdBase.transform.position, posMiddle) >= 3)
            {
                int randomFly = Random.Range(1, 101);
                if (randomFly <= percentFlyPath)
                {
                    maxSpeed = brain.birdBase.speedMovePathInGadern;

                    checkPoint.birdOn.FlyComePath(
               GamePlayController.Instance.level.map.GetPathGoToCheckPoint(brain.birdBase.transform.position, posMiddle),
               posTrans, actionCome, branch, maxSpeed, typeMove: Ease.Linear);
                }
                else
                {
                    maxSpeed = brain.birdBase.speedMoveInGadern;

                    checkPoint.birdOn.FlyCome(
                   posMiddle,
                   posTrans, actionCome, branch, maxSpeed, typeMove: easy);
                }
            }
            else
            {
                maxSpeed = brain.birdBase.speedMoveInGadern;

                checkPoint.birdOn.FlyCome(
               posMiddle,
               posTrans, actionCome, branch, maxSpeed, typeMove: easy);
            }

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
