using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutRedoInGame : TutorialItemInGame
{

    public override void StartTut()
    {
                base.StartTut(); 
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
