using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PasueBox : BaseBox
{
    private static PasueBox instance;
    public static PasueBox Setup()
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<PasueBox>(PathPrefabs.PAUSE_BOX));
            instance.Init();
        }

        instance.gameObject.SetActive(true);
        instance.InitState();
        return instance;
    }

    [SerializeField] private Button restartBtn;
    [SerializeField] private Button quitBtn;
    [SerializeField] private Button closeBtn;
    [SerializeField] private SaferioToggle musicToggle;
    [SerializeField] private SaferioToggle soundToggle;
    [SerializeField] private SaferioToggle vibrationToggle;
    
  //  private AdsSegmentData adsData;

    public void Init()
    {
        restartBtn.onClick.AddListener(OnClickReplay);
        closeBtn.onClick.AddListener(OnClickResume);
        quitBtn.onClick.AddListener(OnClickQuit);
        musicToggle.SetState(GameController.Instance.useProfile.OnMusic);
        soundToggle.SetState(GameController.Instance.useProfile.OnSound);
        vibrationToggle.SetState(GameController.Instance.useProfile.OnVibration);
        musicToggle.RegisterOnSwitchedEvent(EnableGameMusic);
        soundToggle.RegisterOnSwitchedEvent(EnableGameSound);
        vibrationToggle.RegisterOnSwitchedEvent(EnableVibration);
    }

    public void InitState()
    {
        // GamePlayController.Instance.level.PauseLevel(true);

        // BirdSort2.GamePlayController_BirdSort.Instance.countDownController.PauseCoolDown();
        BirdSort2.GamePlayController_BirdSort.Instance.state = BirdSort2.StateGame.Pause;
    }

    public void OnClickResume()
    {
        
        Close();
        BirdSort2.GamePlayController_BirdSort.Instance.state = BirdSort2.StateGame.Playing;
        
    }
    
    private void OnClickQuit()
    {
        Close();
        QuitLevelBox.Setup().Show(BirdSort2.GamePlayController_BirdSort.Instance.level.GetItemYouMiss());
    }

    private void OnClickSetting()
    {
        SettingBox.Setup().Show();
        GameController.Instance.musicManager.PlayClickSound();
    }
    public void OnClickReplay()
    {
        GameController.Instance.musicManager.PlayClickSound();
        ConfirmBox.Setup().AddMessageYesHasCloseBtn(Localization.Get("s_replay_title").ToString(), Localization.Get("s_are_you_sure_replay").ToString(),
            () =>
            {
                if (GameController.Instance.segmentController.GetCurrentSegment().adsData.config.interstitials.showAfterQuit)
                {
                    GameController.Instance.admobAds.ShowInterstitial(actionIniterClose: () =>
                    {
                        GameController.Instance.LoadScene(SceneName.GAME_PLAY);
                    }, actionWatchLog: AdsPlacement.Interstitials.REPLAY_LEVEL_CAMPAIN);
                }
                else
                {
                    GameController.Instance.LoadScene(SceneName.GAME_PLAY);
                }
            });
        
    }
    private void EnableGameMusic(bool isTurnOn)
    {
       GameController.Instance.useProfile.OnMusic = isTurnOn;
    }

    private void EnableGameSound(bool isTurnOn)
    {
        GameController.Instance.useProfile.OnSound = isTurnOn;
    }

    private void EnableVibration(bool isTurnOn)
    {
        GameController.Instance.useProfile.OnVibration = isTurnOn;
    }
}
