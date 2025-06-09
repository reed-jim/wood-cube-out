using System;
using Lean.Localization;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StarChestPopup : BasePopup
{
    [SerializeField] private CanvasGroup popupCanvasGroup;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private LeanLocalizedTextMeshProUGUI localizedDescription;

    [SerializeField] private Button continueButton;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private DailyRewardData rewardData;

    [SerializeField] private int requireGemForEachLevel;

    private StarLevelData _data;
    private int _numAccumulatedChest;

    public static event Action<DailyRewardData, int> showClaimRewardPopupEvent;
    public static event Action claimStarChestEvent;

    protected override void MoreActionInAwake()
    {
        continueButton.onClick.AddListener(Continue);

        _data = DataUtility.Load(GameConstants.STAR_LEVEL_DATA, new StarLevelData());

        requireGemForEachLevel = GameConstants.REQUIRE_STAR;
    }

    protected override void RegisterMoreEvent()
    {
        ResourceEarnPopup.showResourcesEarnPopupEvent += OnResourcesEarnPopupShown;
    }

    protected override void UnregisterMoreEvent()
    {
        ResourceEarnPopup.showResourcesEarnPopupEvent -= OnResourcesEarnPopupShown;
    }

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction: () =>
        {
            AnimateProgress();
        });

        Setup();
    }

    private void Setup()
    {
        int starCollected = Mathf.Min(_data.starCollected, requireGemForEachLevel);

        int remaining = requireGemForEachLevel - starCollected;
        // float progress = (float)starCollected / requireGemForEachLevel;

        // AnimateProgress(starCollected, requireGemForEachLevel, progress);

        // localizedDescription.UpdateTranslationWithParameter(GameConstants.VALUE_PARAMETER, $"{remaining}");

        // progressSlider.value = 0;
        progressText.text = $"{0}/{requireGemForEachLevel}";
    }

    private void Continue()
    {
        HideWithoutAd();
    }

    private void AnimateProgress()
    {
        int starCollected = Mathf.Min(_data.starCollected, requireGemForEachLevel);

        float progress = (float)starCollected / requireGemForEachLevel;

        if (progress > 0.19f)
        {
            // progressSlider.value = 0;

            _tweens.Add(Tween.Custom(progressSlider.value, progress, duration: 0.3f, onValueChange: newVal =>
            {
                progressSlider.value = newVal;
                progressText.text = $"{Mathf.RoundToInt(newVal * requireGemForEachLevel)}/{requireGemForEachLevel}";
            })
            .OnComplete(() =>
            {
                if (progress >= 1)
                {
                    _numAccumulatedChest = _data.starCollected / requireGemForEachLevel;

                    _data.starCollected %= requireGemForEachLevel;

                    DataUtility.Save(GameConstants.STAR_LEVEL_DATA, _data);

                    ShowReward();
                }
            })
            );
        }
        else
        {
            progressSlider.value = 0;
            progressText.text = $"{starCollected}/{requireGemForEachLevel}";
        }
    }

    private void ShowReward()
    {
        showClaimRewardPopupEvent?.Invoke(rewardData, _numAccumulatedChest);
        claimStarChestEvent?.Invoke();

        GamePersistentVariable.currentReasonRewardedAd = ActionWatchVideo.StarChest;
    }

    private void OnResourcesEarnPopupShown(bool isShow)
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
        })
            .OnComplete(() =>
            {
                if (!isShow)
                {
                    Setup();
                    AnimateProgress();

                    popupCanvasGroup.interactable = true;
                }
            })
        );
    }
}
