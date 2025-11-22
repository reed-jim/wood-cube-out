using System;
using DG.Tweening;
using NamPhuThuy.Common;
using NamPhuThuy.Data;
using NamPhuThuy.UI;
using UnityEngine;

namespace NamPhuThuy.UI
{
    public class PictureRewardSegmentedProgressBar : SegmentedProgressBar
    {
        [SerializeField] private RectTransform picture;
        public static event Action<GameEnum.ScreenRoute, bool> switchRouteHideEvent; 

        public override void SetupRewardProgressBar()
        {
            if (DataManager.Ins.PlayerGalleryData.IsUnlockedAllPicture())
            {
                gameObject.SetActive(false);

                return;
            }

            RemoteGalleryRewardConfig galleryRewardConfig = GamePersistentVariable.galleryRewardConfig;

            int currentLevel = DataManager.Ins.CurrentLevel.Value;

            for (int i = 0; i < galleryRewardConfig.Items.Length; i++)
            {
                GalleryRewardConfigItem item = galleryRewardConfig.Items[i];

                if (currentLevel >= item.StartLevel && currentLevel <= item.EndLevel)
                {
                    _numLevelRequiredForNextReward = item.RequiredNumberOfLevel;

                    break;
                }
                else if (i == galleryRewardConfig.Items.Length - 1)
                {
                    _numLevelRequiredForNextReward = 10;
                }
            }

            _currentRewardProgress = galleryRewardConfig.GetCurrentProgress(DataManager.Ins.CurrentLevel.Value);
            
            float prevRewardProgress = galleryRewardConfig.GetPrevProgress(DataManager.Ins.CurrentLevel.Value);

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
