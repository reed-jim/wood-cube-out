using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : StateBase
{
    [SerializeField] private int minTimeIdle = 3;
    [SerializeField] private int maxTimeIdle = 5;
    [SerializeField] private int percentWantMoveIdle = 40;
    private int randomTimeMoveIdle;
    private float timer;
    private bool isMoveIdle;

    public override void StartState()
    {
        timer = 0;
        randomTimeMoveIdle = Random.Range(minTimeIdle, maxTimeIdle);
    }

    public override void UpdateState()
    {
        timer += Time.deltaTime;
        if (timer >= randomTimeMoveIdle)
        {
            randomTimeMoveIdle = Random.Range(minTimeIdle, maxTimeIdle);
            int wantMove = Random.Range(1, 101);
            if (wantMove <= percentWantMoveIdle)
            {
                isMoveIdle = true;

            }
            else
            {
                isMoveIdle = false;
            }    
            timer = 0;
        }
    }

    public override void EndState()
    {

    }
}
