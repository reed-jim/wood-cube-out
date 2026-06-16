using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class TutorialExtralController : TutorialFunController
{
    private IEnumerator actionAllExtralReady;

    public override void Init()
    {
        base.Init();
        actionAllExtralReady = OnAllExtralReady();
        StartCoroutine(actionAllExtralReady);
    }

    private IEnumerator OnAllExtralReady()
    {
        yield return new WaitUntil(() => GamePlayController.Instance.extralPlayController.IsAllExtralReady);
        
        StartTut();
    }

    private void OnDestroy()
    {
        if (actionAllExtralReady != null)
        {
            StopCoroutine(actionAllExtralReady);
        }
    }
}
