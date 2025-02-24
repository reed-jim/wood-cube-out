using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSMBird : MonoBehaviour
{
    public enum StateFSM
    {
        Idle = 0,
        FlyToCheckPoint = 1,
        Catching = 2,
        FlyToCheckPointNoEmpty = 3,
        FlyAroundCheckPoint = 4,
        FlyCurrentCheckPoint = 5,
        FlyAroundCurrentCheckPoint = 6
    }

    public StateFSM state;
  public BirdBase birdBase;

    [SerializeField] private StateBase[] states;
    private StateBase currentState;

    public bool lockFSM;

    public void Init(BirdBase birdBase)
    {
        this.birdBase = birdBase;

        for (int i = 0; i < states.Length; i++)
        {
            states[i].Init(this);
        }
    }

    public void ChangeState(StateFSM newState)
    {
        if (lockFSM)
            return;

        if (currentState != null)
            currentState.EndState();

        state = newState;
        currentState = states[(int)newState];
        currentState.StartState();
    }

    public void Update()
    {
        if (birdBase.isDie)
            return;
        if (lockFSM)
            return;

        if (currentState != null)
            currentState.UpdateState();
    }

    public void ClearFSM()
    {
        for (int i = 0; i < states.Length; i++)
        {
            states[i].EndState();
        }
    }    

    public void ResetState()
    {
        ClearFSM();
        state = StateFSM.Idle;
    }    
}
