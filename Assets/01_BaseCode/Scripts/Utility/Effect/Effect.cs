using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
public class Effect : MonoBehaviour
{
    public SkeletonGraphic skeletonGraphic;
public void ShowCombo()
    {
        skeletonGraphic.AnimationState.ClearTracks();
        skeletonGraphic.Skeleton.SetBonesToSetupPose();
        skeletonGraphic.AnimationState.SetAnimation(0, "animation", false);

        Invoke("HandleEnd", 2);
    }
    private void HandleEnd()
    {
        SimplePool.Despawn(this.gameObject);
    }
}
