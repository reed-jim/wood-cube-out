using System;
using System.Collections;
using DG.Tweening;
using NamPhuThuy.AnimateWithScripts;
using NamPhuThuy.Data;
using NamPhuThuy.UI;
using Saferio.Util.SaferioTween;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace NamPhuThuy.UI
{
    public class CoinRewardSegmentedProgressBar : SegmentedProgressBar
    {
        #region GiftBox Animation
        [Header("Gift Animation")]
        [SerializeField] private SkeletonGraphic giftSkeleton;
        [SerializeField] private RectTransform giftTargetPivot;
        [SerializeField] private float giftFlyDuration = 1f;
        [SerializeField] private float pathCurveHeight = 200f;

        private Vector3 _giftInitialScale;
        #endregion

        private Action onHandleRewardProgressCompleted;
        public static event Action<int> collectCoinEvent;

        public override void InitRewardProgressBar()
        {
            base.InitRewardProgressBar();

            if (DataManager.Ins.PlayerData.RequiredLevelForCoinRewardGift == 0)
            {
                DataManager.Ins.PlayerData.RequiredLevelForCoinRewardGift = 5;
            }
        }

        public override void SetupRewardProgressBar(float currentProgress, float prevProgress)
        {
            _numLevelRequiredForNextReward = DataManager.Ins.PlayerData.RequiredLevelForCoinRewardGift;

            base.SetupRewardProgressBar(currentProgress, prevProgress);

            giftSkeleton.AnimationState.SetAnimation(0, "idle", true);

            SetGiftPosition();
        }

        protected override void HandleRewardCompleted(Action onCompletedAction)
        {
            base.HandleRewardCompleted(onCompletedAction);

            _giftInitialScale = giftSkeleton.transform.localScale;

            StartCoroutine(IEFlyTheGiftBox());

            onHandleRewardProgressCompleted = onCompletedAction;
        }

        void OnCompleteIdle(TrackEntry trackEntry)
        {
            giftSkeleton.AnimationState.SetAnimation(0, "open", false);
            giftSkeleton.AnimationState.Complete -= OnCompleteIdle;

            DOVirtual.DelayedCall(0.8f, () =>
            {
                OnGiftOpenCompleted();
            });
        }

        void OnGiftOpenCompleted(TrackEntry trackEntry = null)
        {
            groupRewardProgress.gameObject.SetActive(true);
            groupRewardProgress.DOFade(1, 0.3f);
            groupRewardProgress.transform.DOScale(1, duration: 0.3f);

            giftSkeleton.transform.DOScale(1.1f * giftSkeleton.transform.localScale, duration: 0.1f)
            .OnComplete(() =>
            {
                giftSkeleton.transform.DOScale(0, 0.3f)
                .OnComplete(() =>
                {
                    giftSkeleton.Skeleton.SetToSetupPose();
                    giftSkeleton.AnimationState.ClearTrack(0);

                    giftSkeleton.transform.localScale = _giftInitialScale;

                    giftSkeleton.gameObject.SetActive(false);
                });
            });

            // DataManager.Ins.RewardDatas.TryApplyRewards(RewardTrigger.COIN_GIFT_AFTER_N_LEVEL, 1);

            DebugLogger.Log(message: $"about to get some coins, current coin: {DataManager.Ins.UserResourcesObserver.UserResources.CoinQuantity}", context: this);
            DataManager.Ins.UserResourcesObserver.UserResources.CoinQuantity += DataConst.COIN_AFTER_N_LEVEL;
            DataManager.Ins.UserResourcesObserver.Save();
            
            DebugLogger.Log(message: $"got some coins, new coin: {DataManager.Ins.UserResourcesObserver.UserResources.CoinQuantity}", context: this);
            collectCoinEvent?.Invoke(DataConst.COIN_AFTER_N_LEVEL);

            DOVirtual.DelayedCall(2.5f, () =>
            {
                onHandleRewardProgressCompleted?.Invoke();
            });

            DataManager.Ins.PlayerData.RequiredLevelForCoinRewardGift = UnityEngine.Random.Range(5, 10);
        }

        IEnumerator IEFlyTheGiftBox()
        {
            // Create path points for curved movement
            Vector3 startPos = giftSkeleton.transform.position;
            Vector3 endPos = giftTargetPivot.position;
            Vector3 controlPoint = (startPos + endPos) / 2f + Vector3.up * pathCurveHeight;

            Vector3[] path = new Vector3[3];
            path[0] = startPos;
            path[1] = controlPoint;
            path[2] = endPos;

            // Turn off some things
            groupRewardProgress.DOFade(0, 0.3f)
            .OnComplete(() =>
            {
                groupRewardProgress.gameObject.SetActive(false);

                rewardProgressBackground.gameObject.SetActive(false);
                rewardProgressText.gameObject.SetActive(false);
            });
            groupRewardProgress.transform.DOScale(0, duration: 0.3f);

            // Animate gift box along the path
            bool flyComplete = false;
            giftSkeleton.transform
                .DOPath(path, giftFlyDuration, PathType.CatmullRom)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => flyComplete = true);

            giftSkeleton.transform.DOScale(_giftInitialScale * 2.5f, giftFlyDuration).SetEase(Ease.OutQuad);

            yield return new WaitUntil(() => flyComplete);



            giftSkeleton.gameObject.SetActive(true);

            giftSkeleton.AnimationState.SetAnimation(0, "idle", false);
            giftSkeleton.AnimationState.Complete += OnCompleteIdle;
        }

        private void SetGiftPosition()
        {
            giftSkeleton.gameObject.SetActive(true);

            Vector3 giftPositon = giftSkeleton.transform.localPosition.ChangeX(
                rewardProgressBackground.transform.localPosition.x + 0.5f * rewardProgressBackground.sizeDelta.x
            );

            giftSkeleton.transform.localPosition = giftPositon;
        }
    }
}
