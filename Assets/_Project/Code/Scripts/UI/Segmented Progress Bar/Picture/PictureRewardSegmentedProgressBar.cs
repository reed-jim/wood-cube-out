using System;
using DG.Tweening;
using NamPhuThuy.Common;
using NamPhuThuy.Data;
using NamPhuThuy.UI;
using Unity.VisualScripting;
using UnityEngine;

namespace NamPhuThuy.UI
{
    public class PictureRewardSegmentedProgressBar : SegmentedProgressBar
    {
        [SerializeField] private RectTransform picture;
        [SerializeField] private IntVariable currentLevelSO;
        public static event Action<GameEnum.ScreenRoute, bool> switchRouteHideEvent; 

        public override void SetupRewardProgressBar()
        {
            if (DataManager.Ins.PlayerGalleryData.IsUnlockedAllPicture())
            {
                gameObject.SetActive(false);
                return;
            }
            
            if (DataManager.Ins.PlayerGalleryData.IsNoMorePicture())
            {
                gameObject.SetActive(false);
                return;
            }

            RemoteGalleryRewardConfig galleryRewardConfig = GamePersistentVariable.galleryRewardConfig;

            int currentLevel = currentLevelSO.Value;

            for (int i = 0; i < galleryRewardConfig.Items.Length; i++)
            {
                GalleryRewardConfigItem item = galleryRewardConfig.Items[i];
                DebugLogger.LogFrog(message: $"StartLevel: {item.StartLevel} - EndLevel: {item.EndLevel} - RequiredNumberOfLevel: {item.RequiredNumberOfLevel}", context:this);

                if (currentLevel >= item.StartLevel && currentLevel <= item.EndLevel)
                {
                    DebugLogger.LogFrog(message: $"Matching item {currentLevel}: StartLevel {item.StartLevel}, EndLevel {item.EndLevel}, RequiredNumberOfLevel {item.RequiredNumberOfLevel}", context:this);
                    _numLevelRequiredForNextReward = item.RequiredNumberOfLevel;

                    break;
                }
                else if (i == galleryRewardConfig.Items.Length - 1)
                {
                    DebugLogger.LogFrog(message: $"No matching item for lv {currentLevel}. Using default values: 10.", context:this);
                    _numLevelRequiredForNextReward = 10;
                }
            }

            _currentRewardProgress = galleryRewardConfig.GetCurrentProgress(currentLevelSO.Value);
            
            float prevRewardProgress = galleryRewardConfig.GetPrevProgress(currentLevelSO.Value);
            
            DebugLogger.LogFrog(message: $"currentRewardProgress: {_currentRewardProgress} - prevRewardProgress: {prevRewardProgress}", context:this);

            base.SetupRewardProgressBar(_currentRewardProgress, prevRewardProgress);

            SetPicturePosition();

            groupRewardProgress.alpha = 0;
        }

        public override void Progress(Action onCompletedAction)
        {
            if (DataManager.Ins.PlayerGalleryData.IsUnlockedAllPicture())
            {
                onCompletedAction?.Invoke();

                return;
            }

            if (DataManager.Ins.PlayerGalleryData.IsNoMorePicture())
            {
                onCompletedAction?.Invoke();

                return;
            }

            groupRewardProgress.DOFade(1, 0.3f)
            .OnComplete(() =>
            {
                base.Progress(onCompletedAction);
            });
        }

        protected override void HandleRewardCompleted(Action onCompletedAction)
        {
            base.HandleRewardCompleted(onCompletedAction);

            PlayerGalleryData pGalleryData = DataManager.Ins.PlayerGalleryData;

            while (pGalleryData.IsContain(pGalleryData.CurrentProgressPictureId))
            {
                pGalleryData.CurrentProgressPictureId++;
            }

            #region NEW

            GamePersistentVariable.currentPictureId_PictureNew = DataManager.Ins.PlayerGalleryData.CurrentProgressPictureId;
            switchRouteHideEvent.Invoke(GameEnum.ScreenRoute.PictureNew, false);

            #endregion
            

            picture.DOScale(1.2f * picture.localScale, 0.1f)
            .OnComplete(() =>
            {
                picture.DOScale(Vector3.zero, 0.2f);
            });

            onCompletedAction?.Invoke();
        }

        private void SetPicturePosition()
        {
            Vector3 position = picture.transform.localPosition;

            position.x = rewardProgressBackground.transform.localPosition.x + 0.5f * rewardProgressBackground.sizeDelta.x;
            position.y = rewardProgressBackground.transform.localPosition.y - 0.1f * rewardProgressBackground.sizeDelta.y;

            picture.transform.localPosition = position;
            picture.localEulerAngles = new Vector3(0, 0, -15);
            picture.transform.localScale = 0.15f * Vector3.one;
        }
    }
}
