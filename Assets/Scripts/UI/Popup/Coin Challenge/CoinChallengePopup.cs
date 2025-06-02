using System;
using System.Collections;
using Lean.Localization;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoinChallengePopup : BasePopup
{
    [SerializeField] private CanvasGroup popupCanvasGroup;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private LeanLocalizedTextMeshProUGUI localizedDescription;
    [SerializeField] private TMP_Text timeCounterText;
    // [SerializeField] private Button claimButton;
    [SerializeField] private Button continueButton;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private UserResourcesObserver userResourcesObserver;
    [SerializeField] private CoinChallengeConfigContainer coinChallengeConfigContainer;

    private CoinChallengeData _challengeData;

    public static event Action<DailyRewardData> showClaimRewardPopupEvent;

    protected override void RegisterMoreEvent()
    {
        ResourceEarnPopup.showResourcesEarnPopupEvent += OnResourcesEarnPopupShown;

        // claimButton.onClick.AddListener(Claim);
        continueButton.onClick.AddListener(Hide);
    }

    protected override void UnregisterMoreEvent()
    {
        ResourceEarnPopup.showResourcesEarnPopupEvent -= OnResourcesEarnPopupShown;
    }

    protected override void MoreActionInAwake()
    {
        _challengeData = DataUtility.Load(GameConstants.COIN_CHALLENGE_DATA, new CoinChallengeData());
    }

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction: () =>
        {
            CheckShowRewards();
        });

        Setup();

        StartCoroutine(Counting());
    }

    private void Setup()
    {
        int currentMilestone = _challengeData.CurrentMilestone;

        int gemCollectedOfMilestone = _challengeData.EventItemCollectedOfMilstones[currentMilestone];
        int requiredItemOfMilestone = coinChallengeConfigContainer.RequiredItems[currentMilestone];
        int remaining = requiredItemOfMilestone - gemCollectedOfMilestone;

        float progress = (float)gemCollectedOfMilestone / coinChallengeConfigContainer.RequiredItems[currentMilestone];

        AnimateProgress(progress);

        localizedDescription.UpdateTranslationWithParameter(GameConstants.VALUE_PARAMETER, $"{remaining}");
    }

    private void CheckShowRewards()
    {
        int currentMilestone = _challengeData.CurrentMilestone;

        DailyRewardData totalReward = new DailyRewardData();

        totalReward.CoinQuantity = 0;
        totalReward.BoosterQuantities = new int[3];

        bool isClaimable = false;

        for (int i = 0; i < _challengeData.IsMilestonesClaimed.Length; i++)
        {
            if (i >= currentMilestone)
            {
                continue;
            }
            else
            {
                if (!_challengeData.IsMilestonesClaimed[i])
                {

                    totalReward.CoinQuantity += coinChallengeConfigContainer.Rewards[i].CoinQuantity;

                    for (int j = 0; j < totalReward.BoosterQuantities.Length; j++)
                    {
                        totalReward.BoosterQuantities[j] += coinChallengeConfigContainer.Rewards[i].BoosterQuantities[j];
                    }

                    _challengeData.IsMilestonesClaimed[i] = true;

                    isClaimable = true;
                }
            }
        }

        if (isClaimable)
        {
            showClaimRewardPopupEvent?.Invoke(totalReward);

            DataUtility.Save(GameConstants.COIN_CHALLENGE_DATA, _challengeData);
        }
    }

    private void AnimateProgress(float progress)
    {
        int currentMilestone = _challengeData.CurrentMilestone;
        int requiredItem = coinChallengeConfigContainer.RequiredItems[currentMilestone];

        if (progress > 0.19f)
        {
            progressSlider.value = 0;

            _tweens.Add(Tween.Custom(progressSlider.value, progress, duration: 0.3f, onValueChange: newVal =>
            {
                progressSlider.value = newVal;
                progressText.text = $"{(newVal * 100).ToString("F0")}%";
                progressText.text = $"{Mathf.RoundToInt(newVal * requiredItem)}/{requiredItem}";
            }));
        }
        else
        {
            progressSlider.value = 0;
            progressText.text = $"{_challengeData.EventItemCollectedOfMilstones[currentMilestone]}/{requiredItem}";
        }
    }

    private IEnumerator Counting()
    {
        WaitForSeconds waitOneSecond = new WaitForSeconds(1);

        while (true)
        {
            UpdateRemainingTime();

            yield return waitOneSecond;
        }
    }

    private void UpdateRemainingTime()
    {
        if (_challengeData.endTimestamp == 0)
        {
            _challengeData.endTimestamp =
                new DateTimeOffset(DateTime.Now).AddDays(7).ToUnixTimeSeconds();

            DataUtility.Save(GameConstants.COIN_CHALLENGE_DATA, _challengeData);
        }

        double remainingTime = (DateTimeOffset.FromUnixTimeSeconds(_challengeData.endTimestamp).LocalDateTime - DateTime.Now).TotalSeconds;

        timeCounterText.text = $"{CommonUtil.ConvertTimeFormat(remainingTime)}";
    }

    private void OnResourcesEarnPopupShown(bool isShow)
    {
        float start = 0;
        float end = 1;

        if (isShow)
        {
            start = 1;
            end = 0;

            Setup();
        }

        _tweens.Add(Tween.Custom(start, end, duration: 0.3f, onValueChange: newVal =>
        {
            popupCanvasGroup.alpha = newVal;
        }));
    }
}
