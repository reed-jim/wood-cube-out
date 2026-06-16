using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Spine.Unity;
using System;

public class MiniGameBird : MonoBehaviour
{
    public Material normalMat;
    public Material outlineMat;
    public bool idLeftRight;
    public SkeletonGraphic anim;
    public Action callBackThisBird;
    public void FlyCome(string nameAction)
    {

        anim.SetAnimation(nameAction, false, delegate { anim.SetAnimation("idle",true);  });
    }
    public void FlyOut(Vector3 post , Action callback)
    {
        callBackThisBird = null ;
        callBackThisBird = callback;
        anim.AnimationState.ClearTracks();
        anim.Skeleton.SetToSetupPose();
        anim.SetAnimation("fly", true);
        this.transform.DOMove(post, UnityEngine.Random.Range(1,1.25f)).OnComplete(() => { callBackThisBird(); });

    }
  
    public void OnOutLine()
    {
        anim.material = outlineMat;
    }
    public void OffOutLine()
    {
        anim.material = normalMat;
    }
}
