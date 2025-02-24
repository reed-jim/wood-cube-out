using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockController : MonoBehaviour
{
    [SerializeField] private float minTimeCallFlock = 5;
    [SerializeField] private float maxTimeCallFlock = 10;
    private float timerCallFlock;
    private float timeCallFlock;

    [SerializeField] private int minNumBirdFlock = 1;
    [SerializeField] private int maxNumBirdFlock = 5;
    [SerializeField] private float distanceBetweenFlock = 3;

    [SerializeField] private float percentFlyAround = 30;

    public bool isLockFlock;

    public void Init()
    {
        timeCallFlock = Random.Range(minTimeCallFlock, maxTimeCallFlock);
    }

    private void PlayFlock()
    {
        var birdInLevel = GamePlayController.Instance.level.birdsInLevel;
        List<BirdBase> birdIdleInGadern = new List<BirdBase>();

        foreach (var birds in birdInLevel)
        {
            for (int i = 0; i < birds.Value.Count; i++)
            {
                int index = i;
                if (birds.Value[index].isBirdInGadern &&
                    !birds.Value[index].isDie &&
                    !birds.Value[index].isLockMove &&
                    !birds.Value[index].isHasExtral &&
                    !birds.Value[index].isBirdHasCover &&
                     !birds.Value[index].isRoping &&
                    birds.Value[index].fsm.state == FSMBird.StateFSM.Idle)
                {
                    birdIdleInGadern.Add(birds.Value[index]);
                }
            }
        }

        birdIdleInGadern = Helper.DisruptiveList(birdIdleInGadern);

        FlockRandomType(birdIdleInGadern);
    }

    private void FlockRandomType(List<BirdBase> birdIdleInGadern)
    {
        if (birdIdleInGadern == null)
            return;

        if (birdIdleInGadern.Count <= 0)
            return;

        if (HasBirdFlyAround())
        {
            var birdFlyAround = birdIdleInGadern[0];
            birdFlyAround.fsm.ChangeState(FSMBird.StateFSM.FlyAroundCheckPoint);
            birdIdleInGadern.RemoveAt(0);
        }

        if (birdIdleInGadern.Count <= 0)
            return;

        BirdBase birdCenter = birdIdleInGadern[Random.Range(0, birdIdleInGadern.Count)];
        int numGetBirdFly = Random.Range(minNumBirdFlock, maxNumBirdFlock);
        List<BirdBase> birdsFly = new List<BirdBase>(); birdsFly.Add(birdCenter);
        for (int i = 0; i < birdIdleInGadern.Count; i++)
        {
            if (birdIdleInGadern[i].GetInstanceID() != birdCenter.GetInstanceID())
            {
                if (Vector2.Distance(birdIdleInGadern[i].transform.position, birdCenter.transform.position) < distanceBetweenFlock)
                {
                    birdsFly.Add(birdIdleInGadern[i]);

                    if (birdsFly.Count >= numGetBirdFly)
                        break;
                }
            }
        }

        for (int i = 0; i < birdsFly.Count; i++)
        {
            int index = i;

            int random = Random.Range(1, 101);
            if (random <= 50)
                birdsFly[index].fsm.ChangeState(FSMBird.StateFSM.FlyToCheckPoint);
            else
                birdsFly[index].fsm.ChangeState(FSMBird.StateFSM.FlyToCheckPointNoEmpty);
        }
    }

    private bool HasBirdFlyAround()
    {
        int random = Random.Range(1, 101);
        if (random <= percentFlyAround)
            return true;
        else
            return false;
    }

    private void UpdateFlocking()
    {
        timerCallFlock += Time.deltaTime;
        if (timerCallFlock >= timeCallFlock)
        {
            PlayFlock();
            timerCallFlock = 0;
            timeCallFlock = Random.Range(minTimeCallFlock, maxTimeCallFlock);
        }
    }

    public void ActiveFanFlock()
    {
        var birdInLevel = GamePlayController.Instance.level.birdsInLevel;
        List<BirdBase> birdIdleInGadern = new List<BirdBase>();

        foreach (var birds in birdInLevel)
        {
            for (int i = 0; i < birds.Value.Count; i++)
            {
                int index = i;
                if (birds.Value[index].isBirdInGadern && !birds.Value[index].isDie && !birds.Value[index].isLockMove && !birds.Value[index].isRoping)
                {
                    birdIdleInGadern.Add(birds.Value[index]);
                }
            }
        }

        var level = GamePlayController.Instance.level;
        int numBirdAppearTopLayer = (int)(level.totalBirdInCover * 0.2f); //Đưa bird bị che ra ngoài (mỗi lần đưa 20%)
        if (numBirdAppearTopLayer < 1)
            numBirdAppearTopLayer = 1;
        level.totalBirdInCover -= numBirdAppearTopLayer;
        if (level.totalBirdInCover < 0)
            level.totalBirdInCover = 0;

        for (int i = 0; i < birdIdleInGadern.Count; i++)
        {
            //Đưa bird bị che ra ngoài
            if (birdIdleInGadern[i].isBirdInCover)
            {
                if (numBirdAppearTopLayer > 0)
                {
                    birdIdleInGadern[i].fsm.ChangeState(FSMBird.StateFSM.FlyToCheckPoint);
                    birdIdleInGadern[i].isBirdInCover = false;
                    numBirdAppearTopLayer--;
                }
                else
                {
                    birdIdleInGadern[i].fsm.ChangeState(FSMBird.StateFSM.FlyAroundCurrentCheckPoint);
                }
                continue;
            }
            if (birdIdleInGadern[i].isBirdHasCover)
            {
                birdIdleInGadern[i].fsm.ChangeState(FSMBird.StateFSM.FlyAroundCurrentCheckPoint);
                continue;
            }


            birdIdleInGadern[i].fsm.ChangeState(FSMBird.StateFSM.FlyToCheckPoint);
        }

        timerCallFlock = 0;
        timeCallFlock = Random.Range(minTimeCallFlock, maxTimeCallFlock);
    }

    private void Update()
    {
        if (isLockFlock)
            return;
        if (GamePlayController.Instance.state != StateGame.Playing)
            return;
        if (!GamePlayController.Instance.extralPlayController.IsAllExtralReady)
            return;

        UpdateFlocking();
    }
}
