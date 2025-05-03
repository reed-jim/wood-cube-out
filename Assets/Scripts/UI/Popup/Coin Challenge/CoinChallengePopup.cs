using System;
using Lean.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoinChallengePopup : BasePopup
{
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private LeanLocalizedTextMeshProUGUI localizedDescription;

    [SerializeField] private int requireGemForEachLevel;

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);

        Setup();
    }

    private void Setup()
    {
        CoinChallengeData coinChallengeData = DataUtility.Load(GameConstants.COIN_CHALLENGE_DATA, new CoinChallengeData());

        int remaining = requireGemForEachLevel - coinChallengeData.gemCollected;
        float progress = (float)coinChallengeData.gemCollected / requireGemForEachLevel;

        progressSlider.value = progress;
        progressText.text = $"{(progress * 100).ToString("F1")}%";

        localizedDescription.UpdateTranslationWithParameter(GameConstants.VALUE_PARAMETER, $"{remaining}");
    }
}
