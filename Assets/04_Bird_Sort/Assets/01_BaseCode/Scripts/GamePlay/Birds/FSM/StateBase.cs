using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateBase : MonoBehaviour
{
    protected FSMBird brain;

    public void Init(FSMBird brain)
    {
        this.brain = brain;
    }    

    public abstract void StartState();

    public abstract void UpdateState();

    public abstract void EndState();
}
