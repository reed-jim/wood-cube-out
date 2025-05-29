using System;
using Lean.Localization;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StarChestPopup : BasePopup
{
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private LeanLocalizedTextMeshProUGUI localizedDescription;

    [SerializeField] private Button continueButton;

    [SerializeField] private int requireGemForEachLevel;

    private StarLevelData _data;

    protected override void MoreActionInAwake()
    {
        continueButton.onClick.AddListener(Continue);

        _data = DataUtility.Load(GameConstants.STAR_LEVEL_DATA, new StarLevelData());
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

        localizedDescription.UpdateTranslationWithParameter(GameConstants.VALUE_PARAMETER, $"{remaining}");

        progressSlider.value = 0;
        progressText.text = $"{0}/{requireGemForEachLevel}";
    }

    private void Continue()
    {
        Hide();
    }

    private void AnimateProgress()
    {
        int starCollected = Mathf.Min(_data.starCollected, requireGemForEachLevel);

        float progress = (float)starCollected / requireGemForEachLevel;

        if (progress > 0.19f)
        {
            progressSlider.value = 0;

            _tweens.Add(Tween.Custom(progressSlider.value, progress, duration: 0.3f, onValueChange: newVal =>
            {
                progressSlider.value = newVal;
                progressText.text = $"{(int)(newVal * requireGemForEachLevel)}/{requireGemForEachLevel}";
            }));
        }
        else
        {
            progressSlider.value = 0;
            progressText.text = $"{starCollected}/{requireGemForEachLevel}";
        }
    }
}
