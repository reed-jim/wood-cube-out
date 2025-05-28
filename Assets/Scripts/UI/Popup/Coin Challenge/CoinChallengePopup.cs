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
    [SerializeField] private DailyRewardData rewardData;

    [SerializeField] private int requireGemForEachLevel;

    private CoinChallengeData _challengeData;

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
        base.Show(onCompletedAction);

        Setup();

        StartCoroutine(Counting());
    }

    private void Setup()
    {
        int gemCollected = Mathf.Min(_challengeData.gemCollected, requireGemForEachLevel);

        int remaining = requireGemForEachLevel - gemCollected;
        float progress = (float)gemCollected / requireGemForEachLevel;

        // claimButton.interactable = remaining == 0;
        // claimButton.image.color = remaining == 0 ? ColorUtil.WithAlpha(claimButton.image.color, 1) : ColorUtil.WithAlpha(claimButton.image.color, 0.5f);

        AnimateProgress(progress);

        // progressSlider.value = progress;
        // progressText.text = $"{(progress * 100).ToString("F1")}%";

        localizedDescription.UpdateTranslationWithParameter(GameConstants.VALUE_PARAMETER, $"{remaining}");
    }

    private void AnimateProgress(float progress)
    {
        if (progress > 0.19f)
        {
            progressSlider.value = 0;

            _tweens.Add(Tween.Custom(progressSlider.value, progress, duration: 0.3f, onValueChange: newVal =>
            {
                progressSlider.value = newVal;
                progressText.text = $"{(newVal * 100).ToString("F0")}%";
            }));
        }
        else
        {
            progressSlider.value = 0;
            progressText.text = $"{(progress * 100).ToString("F0")}%";
        }
    }

    private void Claim()
    {
        int coinQuantity = rewardData.CoinQuantity;
        int[] boosterQuantites = rewardData.BoosterQuantities;

        userResourcesObserver.ChangeBoosterQuantity(0, boosterQuantites[0]);
        userResourcesObserver.ChangeBoosterQuantity(1, boosterQuantites[1]);
        userResourcesObserver.ChangeBoosterQuantity(2, boosterQuantites[2]);

        userResourcesObserver.ChangeCoin(coinQuantity);

        InvokeShowResourcesEarnPopupPopupEvent(coinQuantity, boosterQuantites, false);

        _challengeData.gemCollected = 0;

        DataUtility.Save(GameConstants.COIN_CHALLENGE_DATA, _challengeData);

        Setup();
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
        }

        _tweens.Add(Tween.Custom(start, end, duration: 0.3f, onValueChange: newVal =>
        {
            popupCanvasGroup.alpha = newVal;
        }));
    }
}
