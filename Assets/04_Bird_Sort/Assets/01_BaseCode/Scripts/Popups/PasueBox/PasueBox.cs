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

    [SerializeField] private Button resumeBtn;
    [SerializeField] private Button quitBtn;
    [SerializeField] private Button settingBtn;
    [SerializeField] private Button closeBtn;

  //  private AdsSegmentData adsData;

    public void Init()
    {
        resumeBtn.onClick.AddListener(OnClickResume);
        closeBtn.onClick.AddListener(OnClickResume);
        quitBtn.onClick.AddListener(OnClickQuit);
        settingBtn.onClick.AddListener(OnClickSetting);

    //    adsData = GameController.Instance.segmentController.GetCurrentSegment().adsData;
    }

    public void InitState()
    {
        // GamePlayController.Instance.level.PauseLevel(true);

     //   BirdSort2.GamePlayController_BirdSort.Instance.countDownController.PauseCoolDown();
        if (BirdSort2.GamePlayController_BirdSort.Instance != null)
        {
            BirdSort2.GamePlayController_BirdSort.Instance.state = BirdSort2.StateGame.Pause;
        }
    }

    // ── Connect Scene Integration ─────────────────────────────────
    // public void SetConnectCallbacks(System.Action onResume, System.Action onHome)
    // {
    //     if (resumeBtn != null)
    //     {
    //         resumeBtn.onClick.RemoveAllListeners();
    //         resumeBtn.onClick.AddListener(() => { Close(); onResume?.Invoke(); });
    //     }
    //     if (closeBtn != null)
    //     {
    //         closeBtn.onClick.RemoveAllListeners();
    //         closeBtn.onClick.AddListener(() => { Close(); onResume?.Invoke(); });
    //     }
    //     if (quitBtn != null)
    //     {
    //         quitBtn.onClick.RemoveAllListeners();
    //         quitBtn.onClick.AddListener(() => { Close(); });
    //     }
    // }

    public void OnClickResume()
    {
        //if (adsData.config.interstitials.showOnAppResume)
        //{
        //    if (GamePlayController.Instance.level.GetPercentRequirementFound() >= adsData.config.interstitials.appResumedThreshold)
        //    {
        //        GameController.Instance.admobAds.ShowInterstitial(actionIniterClose: () =>
        //        {
        //            Close();
        //            GamePlayController.Instance.level.PauseLevel(false);
        //        }, actionWatchLog: AdsPlacement.Interstitials.RESUME_CAMPAIN);
        //    }
        //    else
        //    {
        //        Close();
        //        GamePlayController.Instance.level.PauseLevel(false);
        //    }
        //}
        //else
        //{
        //    Close();
        //    GamePlayController.Instance.level.PauseLevel(false);
        //}
        Close();
      //  BirdSort2.GamePlayController_BirdSort.Instance.countDownController.ContinueCollDown();
        if (BirdSort2.GamePlayController_BirdSort.Instance != null)
        {
            BirdSort2.GamePlayController_BirdSort.Instance.state = BirdSort2.StateGame.Playing;
        }
        
    }
    
    private void OnClickQuit()
    {
        Close();
        if (BirdSort2.GamePlayController_BirdSort.Instance != null)
        {
            QuitLevelBox.Setup().Show(GameplayScreen.Instance.GetItemYouMiss());
        }
    }

    

    private void OnClickSetting()
    {
        SettingBox.Setup().Show();
        GameController.Instance.musicManager.PlayClickSound();
    }
}
