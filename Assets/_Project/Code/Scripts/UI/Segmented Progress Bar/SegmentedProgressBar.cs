using System;
using System.Collections.Generic;
using DG.Tweening;
using Lean.Localization;
using NamPhuThuy.Common;
using NamPhuThuy.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NamPhuThuy.UI
{
    public class SegmentedProgressBar : MonoBehaviour
    {
        [Header("Reward Progress")]
        [SerializeField] protected CanvasGroup groupRewardProgress;
        [SerializeField] protected RectTransform rewardProgressBackground;
        [SerializeField] protected Image rewardProgressBackgroundImage;
        [SerializeField] protected Image rewardProgressFill;
        [SerializeField] protected RectTransform rewardProgressFillRT;
        [SerializeField] protected ProgressBarSegment progressBarSegmentPrefab;
        [SerializeField] protected ProgressBarSegment progressBarSegmentBackgroundPrefab;
        [SerializeField] protected ProgressBarSegment[] headProgressBarSegments;
        [SerializeField] protected ProgressBarSegment[] progressBarSegmentPool;
        [SerializeField] protected RectTransform progressBarBackgroundSegmentContainer;
        [SerializeField] protected LeanLocalizedTextMeshProUGUI rewardProgressText;
        [SerializeField] protected TMP_Text rewardProgressTextTMP;

        protected List<ProgressBarSegment> _currentProgressBarSegments;
        protected List<ProgressBarSegment> _currentProgressBarBackgroundSegments;
        protected int _numLevelRequiredForNextReward;
        protected float _currentRewardProgress;

        [Header("CUSTOMIZE")]
        [SerializeField] private int numLevelRequiredForNextReward = 5;

        public virtual void InitRewardProgressBar()
        {
            // DebugLogger.Log(context:this);
            int maxLevelRequired = 15;

            progressBarSegmentPool = new ProgressBarSegment[15];

            for (int i = 0; i < maxLevelRequired; i++)
            {
                progressBarSegmentPool[i] = Instantiate(progressBarSegmentPrefab, rewardProgressBackground).GetComponent<ProgressBarSegment>();

                progressBarSegmentPool[i].gameObject.SetActive(false);
            }

            _currentProgressBarBackgroundSegments = new List<ProgressBarSegment>();

            for (int i = 0; i < maxLevelRequired; i++)
            {
                ProgressBarSegment item = Instantiate(progressBarSegmentBackgroundPrefab, progressBarBackgroundSegmentContainer).GetComponent<ProgressBarSegment>();

                item.gameObject.SetActive(false);
            }

            for (int i = 0; i < progressBarBackgroundSegmentContainer.childCount; i++)
            {
                _currentProgressBarBackgroundSegments.Add(progressBarBackgroundSegmentContainer.GetChild(i).GetComponent<ProgressBarSegment>());
            }

            _numLevelRequiredForNextReward = numLevelRequiredForNextReward;
        }

        public virtual void SetupRewardProgressBar()
        {

        }

        public virtual void SetupRewardProgressBar(float currentProgress, float prevProgress)
        {
            _currentRewardProgress = currentProgress;
            rewardProgressFill.fillAmount = prevProgress == 1 ? 0 : prevProgress;

            // DebugLogger.Log(message: $"_currentRewardProgress: {_currentRewardProgress}", context:this);

            if (_currentRewardProgress < 1)
            {
                // DebugLogger.Log(message: $"Done 1", context:this);
                rewardProgressTextTMP.color = Color.white;

                rewardProgressText.TranslationName = "Only n levels left";
                rewardProgressText.UpdateLocalization();
                rewardProgressText.UpdateTranslationWithParameter(
                        LocalizationConst.LEVEL, $"<color=#78EB66>{Mathf.RoundToInt((1 - _currentRewardProgress) * _numLevelRequiredForNextReward)}</color>");
            }
            else
            {
                // DebugLogger.Log(message: $"Done 2", context:this);
                rewardProgressTextTMP.color = ColorHelper.GetColorFromHex("#78EB66");

                rewardProgressText.TranslationName = "Reward Completed";
                rewardProgressText.UpdateLocalization();
            }

            rewardProgressBackground.GetComponent<Image>().enabled = false;

            for (int i = 0; i < headProgressBarSegments.Length; i++)
            {
                headProgressBarSegments[i].gameObject.SetActive(true);
            }

            int numSegment = _numLevelRequiredForNextReward - 2;
           
            Vector2 segmentSize;

            if (numSegment < 4)
            {
                segmentSize = progressBarSegmentPrefab.InitialSize * 4;
            }
            else
            {
                segmentSize = progressBarSegmentPrefab.InitialSize * 2.5f;
            }
            
            // DebugLogger.Log(message:$"numSegment: {numSegment}", context:this);
            // DebugLogger.Log(message:$"segmentSize: {segmentSize}", context:this);

            rewardProgressBackground.sizeDelta = new Vector2((numSegment + 2) * segmentSize.x, rewardProgressBackground.sizeDelta.y);
            rewardProgressFillRT.sizeDelta = 1.01f * rewardProgressBackground.sizeDelta;

            for (int i = 0; i < progressBarSegmentPool.Length; i++)
            {
                progressBarSegmentPool[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < numSegment; i++)
            {
                progressBarSegmentPool[i].gameObject.SetActive(true);
            }

            // Segment Background
            for (int i = 0; i < _currentProgressBarBackgroundSegments.Count; i++)
            {
                _currentProgressBarBackgroundSegments[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < numSegment + 2; i++)
            {
                _currentProgressBarBackgroundSegments[i].gameObject.SetActive(true);
            }

            // Case Only 1 level required
            if (numSegment < 0)
            {
                for (int i = 0; i < headProgressBarSegments.Length; i++)
                {
                    headProgressBarSegments[i].gameObject.SetActive(false);
                }

                for (int i = 0; i < progressBarSegmentPool.Length; i++)
                {
                    progressBarSegmentPool[i].gameObject.SetActive(false);
                }

                for (int i = 0; i < _currentProgressBarBackgroundSegments.Count; i++)
                {
                    _currentProgressBarBackgroundSegments[i].gameObject.SetActive(false);
                }

                rewardProgressBackgroundImage.enabled = true;

                rewardProgressBackground.sizeDelta = new Vector2(5 * segmentSize.x, rewardProgressBackground.sizeDelta.y);
                rewardProgressFillRT.sizeDelta = 1.01f * rewardProgressBackground.sizeDelta;
            }
            else
            {
                rewardProgressBackgroundImage.enabled = false;
            }
            //

            _currentProgressBarSegments = new List<ProgressBarSegment>();

            for (int i = 0; i < rewardProgressBackground.childCount; i++)
            {
                ProgressBarSegment segment = rewardProgressBackground.GetChild(i).GetComponent<ProgressBarSegment>();

                if (segment != null)
                {
                    if (segment.gameObject.activeSelf)
                    {
                        _currentProgressBarSegments.Add(segment);
                    }
                }
            }

            for (int i = 0; i < _currentProgressBarSegments.Count; i++)
            {
                _currentProgressBarSegments[i].SetSizeDeltaX(segmentSize.x);

                if (i == 0)
                {
                    _currentProgressBarSegments[i].SetAnchoredPositionX(0.5f * _currentProgressBarSegments[i].Size.x);
                }
                else if (i == 1)
                {
                    _currentProgressBarSegments[i].SetAnchoredPositionX(-0.5f * _currentProgressBarSegments[i].Size.x);
                }
                else
                {
                    _currentProgressBarSegments[i].SetLocalPositionX(-((numSegment - 1) / 2f) * segmentSize.x + (i - 2) * segmentSize.x);
                }

                // set background for the segment
                // warning: case _currentProgressBarSegments[i] scale = 0
                _currentProgressBarBackgroundSegments[i].transform.position = _currentProgressBarSegments[i].transform.position;
                _currentProgressBarBackgroundSegments[i].SetSizeDeltaX(segmentSize.x);
            }
        }

        public void Reset()
        {
            groupRewardProgress.gameObject.SetActive(true);
            groupRewardProgress.alpha = 1;
            groupRewardProgress.transform.localScale = Vector3.one;
            rewardProgressText.gameObject.SetActive(true);
            rewardProgressBackground.gameObject.SetActive(true);
        }

        public virtual void Progress(Action onCompletedAction)
        {
            float currentProgress = rewardProgressFill.fillAmount;

            DOTween.To(() => currentProgress, x => currentProgress = x, _currentRewardProgress, 1)
            .OnUpdate(() =>
            {
                rewardProgressFill.fillAmount = currentProgress;
            })
            .OnComplete(() =>
            {
                if (currentProgress >= 1f)
                {
                    HandleRewardCompleted(onCompletedAction);
                }
                else
                {
                    onCompletedAction?.Invoke();
                }
            });
        }

        public bool WillProgressCompleted()
        {
            return Mathf.Abs(1 - _currentRewardProgress) < 0.01f;
        }

        protected virtual void HandleRewardCompleted(Action onCompletedAction)
        {

        }
    }
}
