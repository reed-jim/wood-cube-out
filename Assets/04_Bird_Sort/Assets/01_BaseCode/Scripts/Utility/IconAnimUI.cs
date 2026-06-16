using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using Spine.Unity;
using Spine;
using System;

public class IconAnimUI : MonoBehaviour
{
    [SerializeField] protected string nameAnim;
    [SerializeField] protected float timeDelayAnim = 5;
    protected float timerAnim;
    private Tween anim;
    [SerializeField] protected SkeletonGraphic body;

    private void Awake()
    {
        timerAnim = timeDelayAnim;
    }


    public virtual void PlayAnim(string nameAnim, bool isLoop, UnityAction actionDone = null)
    {
        if (body.SkeletonDataAsset != null)
        {
            body.AnimationState.ClearTracks();
            var track = body.AnimationState.SetAnimation(0, nameAnim, isLoop);
            track.Complete += (s) => { actionDone?.Invoke(); };
        }
    }

    private void Update()
    {
        timerAnim += Time.deltaTime;
        if(timerAnim >= timeDelayAnim)
        {
            PlayAnim(nameAnim, false);
            timerAnim = 0;
        }    
    }
}
