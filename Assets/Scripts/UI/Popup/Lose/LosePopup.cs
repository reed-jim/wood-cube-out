using System;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using static GameEnum;

public class LosePopup : BasePopup
{
    [SerializeField] private Button replayButton;
    [SerializeField] private Button returnHomeButton;
    [SerializeField] private Slider levelProgress;
    [SerializeField] private TMP_Text progressText;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private LevelObserver levelObserver;
    [SerializeField] private LevelBoosterObserver levelBoosterObserver;

    [Header("CUSTOMIZE")]
    [SerializeField] private float transitionDuration;

    public static event Action replayLevelEvent;

    protected override void RegisterMoreEvent()
    {
        // GameStateLose.loseLevelEvent += OnLevelLose;
        GameStateLose.loseLevelEvent += OnLevelLose;
        RevivePopup.reviveRefusedEvent += OnLevelLose;

        replayButton.onClick.AddListener(Replay);
        returnHomeButton.onClick.AddListener(ReturnHome);
    }

    protected override void UnregisterMoreEvent()
    {
        // GameStateLose.loseLevelEvent -= OnLevelLose;
        GameStateLose.loseLevelEvent -= OnLevelLose;
        RevivePopup.reviveRefusedEvent -= OnLevelLose;
    }


    private void OnApplicationQuit()
    {
        SaferioTracking.TrackLevelLose(currentLevel.Value, levelObserver.Progress, levelBoosterObserver, EndLevelReason.Quit.ToString());
    }

    private void OnLevelLose()
    {
        levelProgress.value = 0;

        progressText.text = $"{0}%";

        if (levelObserver.Progress > 0.25f)
        {
            progressText.gameObject.SetActive(true);
        }
        else
        {
            progressText.gameObject.SetActive(false);
        }

        SaferioTracking.TrackLevelLose(currentLevel.Value, levelObserver.Progress, levelBoosterObserver, EndLevelReason.Lose.ToString());

        Show(onCompletedAction: () =>
        {
            Tween.Custom(0, levelObserver.Progress, duration: transitionDuration, onValueChange: newVal =>
            {
                levelProgress.value = newVal;

                progressText.text = $"{(int)(newVal * 100)}%";
            });
        });

        SoundManager.Instance.PlaySoundLose();
    }

    private void Replay()
    {
        if (GameController.Instance.segmentController.GetCurrentSegment().adsData.config.interstitials.showAfterDefeat)
        {
            AdmobAdsMax.Instance.ShowInterstitial(actionIniterClose: OnInterstitialAdCompleted);
        }
        else
        {
            replayLevelEvent?.Invoke();

            SaferioTracking.TrackReplayLevel(currentLevel.Value);

            Hide();
        }
    }

    private void OnInterstitialAdCompleted()
    {
        replayLevelEvent?.Invoke();

        SaferioTracking.TrackReplayLevel(currentLevel.Value);
        SaferioTracking.TrackInterstitialAdCompleted(currentLevel.Value, "replay");

        Hide();
    }

    private void ReturnHome()
    {
        Addressables.LoadSceneAsync(GameConstants.MENU_SCENE);
    }
}
