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
        base.Show(onCompletedAction);

        Setup();
    }

    private void Setup()
    {
        int starCollected = Mathf.Min(_data.starCollected, requireGemForEachLevel);

        int remaining = requireGemForEachLevel - starCollected;
        float progress = (float)starCollected / requireGemForEachLevel;

        AnimateProgress(progress);

        localizedDescription.UpdateTranslationWithParameter(GameConstants.VALUE_PARAMETER, $"{remaining}");
    }

    private void Continue()
    {
        Hide();
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
}
