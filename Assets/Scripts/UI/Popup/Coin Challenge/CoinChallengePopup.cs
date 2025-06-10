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
    [SerializeField] private RectTransform timeCounterContainer;
    [SerializeField] private TMP_Text timeCounterText;
    [SerializeField] private RectTransform nextEventTimeContainer;
    [SerializeField] private TMP_Text nextEventTimeCounterText;
    // [SerializeField] private Button claimButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Image continueButtonImage;
    [SerializeField] private TMP_Text continueText;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private UserResourcesObserver userResourcesObserver;
    [SerializeField] private CoinChallengeConfigContainer coinChallengeConfigContainer;

    private CoinChallengeData _challengeData;
    private CoinChallengeData _lastChallengeData;
    private bool _isAvailable;

    public static event Action<DailyRewardData> showClaimRewardPopupEvent;
    public static event Action claimCoinChallengeEvent;

    protected override void RegisterMoreEvent()
    {
        ClaimRewardPopup.showClaimRewardPopupEvent += OnClaimRewardPopupShown;
        ResourceEarnPopup.showResourcesEarnPopupEvent += OnResourcesEarnPopupShown;

        // claimButton.onClick.AddListener(Claim);
        continueButton.onClick.AddListener(Continue);
    }

    protected override void UnregisterMoreEvent()
    {
        ClaimRewardPopup.showClaimRewardPopupEvent -= OnClaimRewardPopupShown;
        ResourceEarnPopup.showResourcesEarnPopupEvent -= OnResourcesEarnPopupShown;
    }

    protected override void MoreActionInAwake()
    {
        _challengeData = ResetChallenge();
    }

    protected override void Show(Action onCompletedAction = null)
    {
        nextEventTimeContainer.gameObject.SetActive(false);


        #region DISABLE ANIMATION
        int currentMilestone = _challengeData.CurrentMilestone;

        int gemCollectedOfMilestone = _challengeData.EventItemCollectedOfMilstones[currentMilestone];
        int requiredItemOfMilestone = coinChallengeConfigContainer.RequiredItems[currentMilestone];
        int remaining = requiredItemOfMilestone - gemCollectedOfMilestone;

        float progress = (float)gemCollectedOfMilestone / coinChallengeConfigContainer.RequiredItems[currentMilestone];

        int requiredItem = coinChallengeConfigContainer.RequiredItems[currentMilestone];

        progressSlider.value = progress;
        progressText.text = $"{Mathf.RoundToInt(progress * requiredItem)}/{requiredItem}";
        #endregion

        base.Show(onCompletedAction: () =>
        {
            // Setup();

            CheckShowRewards(_challengeData);

            try
            {
                CheckShowRewards(_lastChallengeData);
            }
            catch (System.Exception)
            {

            }
        });

        CheckAvailabilty();
    }

    private CoinChallengeData ResetChallenge()
    {
        CoinChallengeData data = DataUtility.Load(GameConstants.COIN_CHALLENGE_DATA, new CoinChallengeData());

        if (CommonUtil.IsNewWeek(data.startTimestamp))
        {
            _lastChallengeData = data;

            data = new CoinChallengeData();

            data.startTimestamp = CommonUtil.GetCurrentTimestamp();

            DataUtility.Save(GameConstants.COIN_CHALLENGE_DATA, data);
        }

        return data;
    }

    private void Setup()
    {
        int currentMilestone = _challengeData.CurrentMilestone;

        int gemCollectedOfMilestone = _challengeData.EventItemCollectedOfMilstones[currentMilestone];
        int requiredItemOfMilestone = coinChallengeConfigContainer.RequiredItems[currentMilestone];
        int remaining = requiredItemOfMilestone - gemCollectedOfMilestone;

        remaining = Mathf.Max(remaining, 0);

        float progress = (float)gemCollectedOfMilestone / coinChallengeConfigContainer.RequiredItems[currentMilestone];

        AnimateProgress(progress);

        if (_challengeData.IsCompleted())
        {
            localizedDescription.TranslationName = "The challenge has been completed";
        }
        else
        {
            localizedDescription.TranslationName = "Coin Challenge Description";

            // localizedDescription.UpdateTranslationWithParameter(GameConstants.VALUE_PARAMETER, $"{remaining}");
        }

        CheckAvailabilty();
    }

    private void Continue()
    {
        HideWithoutAd();
    }

    private void CheckAvailabilty()
    {
        DayOfWeek dayOfWeek = DateTime.Now.DayOfWeek;

        _isAvailable = dayOfWeek == DayOfWeek.Monday || dayOfWeek == DayOfWeek.Tuesday;

        if (_isAvailable)
        {
            if (_challengeData.IsCompleted())
            {
                _isAvailable = false;
            }
        }

        if (_isAvailable)
        {
            nextEventTimeContainer.gameObject.SetActive(false);
            continueText.gameObject.SetActive(true);
            timeCounterContainer.gameObject.SetActive(true);

            continueButton.interactable = true;

            continueButtonImage.color = Color.white;

            StartCoroutine(Counting());
        }
        else
        {
            nextEventTimeContainer.gameObject.SetActive(true);
            continueText.gameObject.SetActive(false);
            timeCounterContainer.gameObject.SetActive(false);

            continueButton.interactable = false;

            continueButtonImage.color = ColorUtil.WithAlpha(Color.white, 0.5f);

            StartCoroutine(CountingNextEvent());
        }
    }

    private void CheckShowRewards(CoinChallengeData data)
    {
        int currentMilestone = data.CurrentMilestone;

        DailyRewardData totalReward = new DailyRewardData();

        totalReward.CoinQuantity = 0;
        totalReward.BoosterQuantities = new int[3];

        bool isClaimable = false;

        for (int i = 0; i < data.IsMilestonesClaimed.Length; i++)
        {
            if (i > currentMilestone)
            {
                continue;
            }
            else
            {
                bool isValid = false;

                if (i == currentMilestone)
                {
                    if (data.EventItemCollectedOfMilstones[i] >= coinChallengeConfigContainer.RequiredItems[i] &&
                        !data.IsMilestonesClaimed[i]
                    )
                    {
                        isValid = true;
                    }
                }
                else
                {
                    if (!data.IsMilestonesClaimed[i])
                    {
                        isValid = true;
                    }
                }

                if (isValid)
                {
                    totalReward.CoinQuantity += coinChallengeConfigContainer.Rewards[i].CoinQuantity;

                    for (int j = 0; j < totalReward.BoosterQuantities.Length; j++)
                    {
                        totalReward.BoosterQuantities[j] += coinChallengeConfigContainer.Rewards[i].BoosterQuantities[j];
                    }

                    data.IsMilestonesClaimed[i] = true;

                    isClaimable = true;
                }
            }
        }

        if (isClaimable)
        {
            showClaimRewardPopupEvent?.Invoke(totalReward);

            GamePersistentVariable.currentReasonRewardedAd = ActionWatchVideo.CoinChallenge;

            DataUtility.Save(GameConstants.COIN_CHALLENGE_DATA, data);

            claimCoinChallengeEvent?.Invoke();
        }
    }

    private void AnimateProgress(float progress)
    {
        int currentMilestone = _challengeData.CurrentMilestone;
        int requiredItem = coinChallengeConfigContainer.RequiredItems[currentMilestone];

        if (progress > 0.19f)
        {
            progressSlider.value = 0;

            _tweens.Add(Tween.Custom(progressSlider.value, progress, duration: 0.0f, onValueChange: newVal =>
            {
                progressSlider.value = newVal;
                // progressText.text = $"{(newVal * 100).ToString("F0")}%";
                progressText.text = $"{Mathf.RoundToInt(newVal * requiredItem)}/{requiredItem}";

                localizedDescription.UpdateTranslationWithParameter(GameConstants.VALUE_PARAMETER, $"{(int)(requiredItem * (1 - newVal))}");
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

    private IEnumerator CountingNextEvent()
    {
        WaitForSeconds waitOneSecond = new WaitForSeconds(1);

        while (true)
        {
            UpdateNextEventTime();

            yield return waitOneSecond;
        }
    }

    private void UpdateRemainingTime()
    {
        DateTime now = DateTime.UtcNow;

        int daysUntilEndOfWeek = ((int)DayOfWeek.Tuesday - (int)now.DayOfWeek + 7) % 7;

        DateTime endOfWeek = new DateTime(now.Year, now.Month, now.Day)
            .AddDays(daysUntilEndOfWeek)
            .AddHours(23)
            .AddMinutes(59)
            .AddSeconds(59);

        TimeSpan timeLeft = endOfWeek - now;

        double remaningSecond = timeLeft.TotalSeconds;

        timeCounterText.text = $"{CommonUtil.ConvertTimeFormat(remaningSecond)}";
    }

    private void UpdateNextEventTime()
    {
        DateTime now = DateTime.UtcNow;

        int daysUntilEndOfWeek = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;

        DateTime endOfWeek = new DateTime(now.Year, now.Month, now.Day)
            .AddDays(daysUntilEndOfWeek)
            .AddHours(23)
            .AddMinutes(59)
            .AddSeconds(59);

        TimeSpan timeLeft = endOfWeek - now;

        double remaningSecond = timeLeft.TotalSeconds;

        nextEventTimeCounterText.text = $"{CommonUtil.ConvertTimeFormat(remaningSecond)}";
    }

    private void OnClaimRewardPopupShown(bool isShow)
    {
        float start = 0;
        float end = 1;

        if (isShow)
        {
            start = 1;
            end = 0;

            popupCanvasGroup.interactable = false;
        }

        _tweens.Add(Tween.Custom(start, end, duration: 0.3f, onValueChange: newVal =>
        {
            popupCanvasGroup.alpha = newVal;
        }));
    }

    private void OnResourcesEarnPopupShown(bool isShow)
    {
        float start = 0;
        float end = 1;

        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        if (isShow)
        {
            start = 1;
            end = 0;

            popupCanvasGroup.interactable = false;

            if (gameObject.activeInHierarchy)
            {
                Setup();
            }

            return;
        }

        _tweens.Add(Tween.Custom(start, end, duration: 0.3f, onValueChange: newVal =>
        {
            popupCanvasGroup.alpha = newVal;
        })
            .OnComplete(() =>
            {
                if (!isShow)
                {
                    popupCanvasGroup.interactable = true;
                }
            })
        );
    }
}
