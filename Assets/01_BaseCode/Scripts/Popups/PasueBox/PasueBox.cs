using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PasueBox : BaseBox
{
//     private static PasueBox instance;
//     public static PasueBox Setup()
//     {
//         if (instance == null)
//         {
//             instance = Instantiate(Resources.Load<PasueBox>(PathPrefabs.PAUSE_BOX));
//             instance.Init();
//         }

//         instance.gameObject.SetActive(true);
//         instance.InitState();
//         return instance;
//     }

//     [SerializeField] private Button resumeBtn;
//     [SerializeField] private Button quitBtn;
//     [SerializeField] private Button settingBtn;
//     [SerializeField] private Button closeBtn;

//   //  private AdsSegmentData adsData;

//     public void Init()
//     {
//         resumeBtn.onClick.AddListener(OnClickResume);
//         closeBtn.onClick.AddListener(OnClickResume);
//         quitBtn.onClick.AddListener(OnClickQuit);
//         settingBtn.onClick.AddListener(OnClickSetting);

//     //    adsData = GameController.Instance.segmentController.GetCurrentSegment().adsData;
//     }

//     public void InitState()
//     {
//         // GamePlayController.Instance.level.PauseLevel(true);

//      //   BirdSort2.GamePlayController_BirdSort.Instance.countDownController.PauseCoolDown();
//         BirdSort2.GamePlayController_BirdSort.Instance.state = BirdSort2.StateGame.Pause;
//     }

//     public void OnClickResume()
//     {
//         //if (adsData.config.interstitials.showOnAppResume)
//         //{
//         //    if (GamePlayController.Instance.level.GetPercentRequirementFound() >= adsData.config.interstitials.appResumedThreshold)
//         //    {
//         //        GameController.Instance.admobAds.ShowInterstitial(actionIniterClose: () =>
//         //        {
//         //            Close();
//         //            GamePlayController.Instance.level.PauseLevel(false);
//         //        }, actionWatchLog: AdsPlacement.Interstitials.RESUME_CAMPAIN);
//         //    }
//         //    else
//         //    {
//         //        Close();
//         //        GamePlayController.Instance.level.PauseLevel(false);
//         //    }
//         //}
//         //else
//         //{
//         //    Close();
//         //    GamePlayController.Instance.level.PauseLevel(false);
//         //}
//         Close();
//       //  BirdSort2.GamePlayController_BirdSort.Instance.countDownController.ContinueCollDown();
//         BirdSort2.GamePlayController_BirdSort.Instance.state = BirdSort2.StateGame.Playing;
        
//     }
    
//     private void OnClickQuit()
//     {
//         Close();
//         QuitLevelBox.Setup().Show(BirdSort2.GamePlayController_BirdSort.Instance.level.GetItemYouMiss());
//     }

//     private void OnClickSetting()
//     {
//         SettingBox.Setup().Show();
//         GameController.Instance.musicManager.PlayClickSound();
//     }
}
