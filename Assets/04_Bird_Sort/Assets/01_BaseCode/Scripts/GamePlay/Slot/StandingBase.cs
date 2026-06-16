using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
//using MoreMountains.NiceVibrations;
using UnityEngine.UI;
using Spine.Unity;
using Spine;
using System;
using Random = UnityEngine.Random;


public class StandingBase : MonoBehaviour
{
    public DOTweenAnimation animShake;


    public void Init()
    {
      
    }

    public virtual void Shake()
    {
        animShake.DOPlay();
        animShake.DOComplete(true);
        animShake.onComplete.RemoveAllListeners();
        animShake.onComplete.AddListener(() => { animShake.DORewind(); });
    }
  
}
