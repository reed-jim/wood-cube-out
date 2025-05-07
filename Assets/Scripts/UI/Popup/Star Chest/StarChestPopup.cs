using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StarChestPopup : BasePopup
{
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TMP_Text progressText;

    [SerializeField] private Button claimButton;

    // private void AnimateProgress()
    // {
    //     WeeklyChallengeData weeklyChallengeData = DataUtility.Load(GameConstants.WEEKLY_CHALLENGE_DATA, new WeeklyChallengeData());

    //     float progress = weeklyChallengeData.EventItemCollected % 2 / 2f;

    //     if (progress > 0.19f)
    //     {
    //         progressSlider.value = 0;

    //         _tweens.Add(Tween.Custom(progressSlider.value, progress, duration: 0.3f, onValueChange: newVal =>
    //         {
    //             progressSlider.value = newVal;
    //             progressText.text = $"{(newVal * 100).ToString("F0")}%";
    //         }));
    //     }
    //     else
    //     {
    //         progressSlider.value = 0;
    //         progressText.text = $"{(progress * 100).ToString("F0")}%";
    //     }
    // }   
}
