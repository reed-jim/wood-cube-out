using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CatchingState : StateBase
{
    public override void StartState()
    {
        this.transform.DOKill();
    }

    public override void UpdateState()
    {
       
    }
    public override void EndState()
    {
        
    }
}
