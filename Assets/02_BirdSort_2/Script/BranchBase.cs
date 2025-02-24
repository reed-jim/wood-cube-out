namespace BirdSort2
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Spine.Unity;
    using Spine;
    using UnityEngine.UI;
    using DG.Tweening;
    public class BranchBase : MonoBehaviour
    {
        public bool isShakeGrounding;
        public Transform body;
        public Image icon;
        public SkeletonGraphic branchAnim;
        public Vector3 scaleInShop;
        public Vector3 scaleInReward;
        public TypeStand standType;

        private TrackEntry entryAnim;

        public void Init()
        {

        }
        public virtual void InitPlay()
        {
            PlayAnimationIdle();
        }
        public void SetScaleInShop(Transform parent)
        {
            this.transform.SetParent(parent);
            this.transform.localScale = scaleInShop;
            this.transform.localPosition = Vector3.zero;
        }
        public void SetScaleInReward()
        {
            this.transform.localScale = scaleInReward;
        }
        public virtual void PlayAnimationGrounded()
        {
            if (branchAnim != null)
            {
                switch (standType)
                {
                    case TypeStand.Slot_4:
                        branchAnim.AnimationState.ClearTracks();
                        var track = branchAnim.AnimationState.SetAnimation(0, "cành/1", false);
                        track.Complete += OnAnimEndDone;
                        break;
                    case TypeStand.Slot_5:
                        branchAnim.AnimationState.ClearTracks();
                        var track2 = branchAnim.AnimationState.SetAnimation(0, "cành/2", false);
                        track2.Complete += OnAnimEndDone;
                        break;
                    case TypeStand.Slot_6:
                        branchAnim.AnimationState.ClearTracks();
                        var track3 = branchAnim.AnimationState.SetAnimation(0, "cành/3", false);
                        track3.Complete += OnAnimEndDone;
                        break;
                }
            }
        }
        public void TurnGray()
        {
            if (branchAnim != null)
            {
                branchAnim.color = Color.gray;
            }
            else
            {
                icon.color = Color.gray;
            }
        }
        public virtual void PlayAnimationIdle()
        {

        }
        public void SetParentBird(Transform parentBird)
        {
            foreach (Transform bird in body)
            {
                bird.SetParent(parentBird);
            }
        }
        public virtual void SetBirdToBranchBody(Transform bird)
        {
            //bird.SetParent(body);
        }
        private void OnAnimEndDone(TrackEntry trackEntry)
        {
            if (branchAnim != null)
            {
                if (trackEntry.Animation.Name.Equals("cành/1"))
                {
                    branchAnim.AnimationState.ClearTracks();
                    entryAnim = branchAnim.AnimationState.SetAnimation(0, "cành/1.1", true);
                }
                else if (trackEntry.Animation.Name.Equals("cành/2"))
                {
                    branchAnim.AnimationState.ClearTracks();
                    entryAnim = branchAnim.AnimationState.SetAnimation(0, "cành/2.1", true);
                }
                else if (trackEntry.Animation.Name.Equals("cành/3"))
                {
                    branchAnim.AnimationState.ClearTracks();
                    entryAnim = branchAnim.AnimationState.SetAnimation(0, "cành/3.1", true);
                }
            }
        }
        public void StopAnim()
        {
            if (branchAnim != null)
            {
                branchAnim.AnimationState.GetCurrent(0).TimeScale = 0;
            }
        }
    }
}