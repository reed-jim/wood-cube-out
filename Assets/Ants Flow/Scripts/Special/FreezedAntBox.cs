using System;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Spine.Unity;
using TMPro;
using UnityEngine;

namespace AntsFlow
{
    public class FreezedAntBox : WashingMachineItem
    {
        [SerializeField] private TMP_Text numAntBoxToUnfreezeText;
        [SerializeField] private SkeletonAnimation frozenSkeletonAnimation;
        [SerializeField] private Renderer frozenSkeletonRenderer;
        [SpineAnimation] public string frozenStateOneAnimation;
        [SpineAnimation] public string frozenStateTwoAnimation;
        [SpineAnimation] public string frozenStateThreeAnimation;

        [SerializeField] private ParticleSystem iceBreakFx;
        [SerializeField] private ParticleSystem bigIceBreakFx;

        public int numAntBoxToUnfreeze;
        public int totalNumAntBoxToUnfreeze;
        public bool isFreezed;

        private MaterialPropertyBlock materialPropertyBlock;
        private bool isPlayingIceBreakFx;

        protected override void ExtendAwake()
        {
            base.ExtendAwake();

            materialPropertyBlock = new MaterialPropertyBlock();
        }

        public override void Init(WashingMachineStartSlot washingMachineStartSlot, Color faction, int number, Action onCompleteAction = null)
        {
            base.Init(washingMachineStartSlot, faction, number);

            totalNumAntBoxToUnfreeze = SpecialElementManager.Ins.nextFreezeAntBoxRequiredNumber;
            numAntBoxToUnfreeze = totalNumAntBoxToUnfreeze;
            isFreezed = true;

            numAntBoxToUnfreezeText.text = $"{numAntBoxToUnfreeze}";

            frozenSkeletonAnimation.gameObject.SetActive(true);
            numberText.gameObject.SetActive(false);

            Tween.Custom(0, 1, duration: 0.3f, onValueChange: newVal =>
            {
                materialPropertyBlock.SetColor("_Color", ColorUtil.WithAlpha(Color.white, newVal));
                frozenSkeletonRenderer.SetPropertyBlock(materialPropertyBlock);
            });

            SetState();

            SaferioTween.AlphaShow(numAntBoxToUnfreezeText);

            SoundManager.Instance.PlaySound(GameConstants.startFreezeSound);
        }

        public void AssignRequirementNumber()
        {
            totalNumAntBoxToUnfreeze =
                Mathf.Min(totalNumAntBoxToUnfreeze, SpecialElementManager.Ins.GetMaxValueFreezedAntBox(washingMachineStartSlot.row, washingMachineStartSlot.column));

            numAntBoxToUnfreeze = totalNumAntBoxToUnfreeze;
            numAntBoxToUnfreezeText.text = $"{numAntBoxToUnfreeze}";
        }

        public override bool IsValidToSelect()
        {
            return base.IsValidToSelect() && numAntBoxToUnfreeze <= 0;
        }

        public override bool IsValidToSelect(int currentNumSlotRequired)
        {
            return base.IsValidToSelect(currentNumSlotRequired) && numAntBoxToUnfreeze <= 0;
        }

        protected override void PlayInvalidSelectAnimation()
        {
            if (numAntBoxToUnfreeze > 0)
            {
                Tween.ShakeScale(transform, strength: 0.2f * Vector3.one, duration: 0.3f);

                iceBreakFx.Play();
            }
            else
            {
                base.PlayInvalidSelectAnimation();
            }
        }

        private void Unfreeze()
        {
            Tween.Custom(1, 0, duration: 0.3f, onValueChange: newVal =>
            {
                materialPropertyBlock.SetColor("_Color", ColorUtil.WithAlpha(Color.white, newVal));
                frozenSkeletonRenderer.SetPropertyBlock(materialPropertyBlock);
            });
            SaferioTween.AlphaHide(numAntBoxToUnfreezeText);
            SaferioTween.AlphaShow(numberText);

            isFreezed = false;
        }

        protected override void OnAntBoxCompleted(WashingMachineItem antBox)
        {
            base.OnAntBoxCompleted(antBox);

            if (!isFreezed)
            {
                return;
            }

            if (washingMachineStartSlot == null || washingMachineStartSlot.row > 1)
            {
                return;
            }

            numAntBoxToUnfreeze--;
            numAntBoxToUnfreeze = Mathf.Max(numAntBoxToUnfreeze, 0);

            numAntBoxToUnfreezeText.text = $"{numAntBoxToUnfreeze}";

            if (numAntBoxToUnfreeze <= 0)
            {
                Unfreeze();

                bigIceBreakFx.Play();
            }
            else
            {
                if (!isPlayingIceBreakFx)
                {
                    iceBreakFx.Play();

                    isPlayingIceBreakFx = true;

                    Tween.Delay(1, onComplete: () =>
                    {
                        isPlayingIceBreakFx = false;
                    });
                }
            }

            SoundManager.Instance.PlaySound(GameConstants.iceBreakSound);

            SetState();
        }

        private void SetState()
        {
            float progress = GetProgress();

            if (progress > 0.5f)
            {
                frozenSkeletonAnimation.AnimationState.SetAnimation(0, frozenStateOneAnimation, false);
            }
            else if (progress > 0)
            {
                frozenSkeletonAnimation.AnimationState.SetAnimation(0, frozenStateThreeAnimation, false);
            }
            else
            {
                frozenSkeletonAnimation.AnimationState.SetAnimation(0, frozenStateThreeAnimation, false);
            }
        }

        private float GetProgress()
        {
            return (float)numAntBoxToUnfreeze / totalNumAntBoxToUnfreeze;
        }
    }
}
