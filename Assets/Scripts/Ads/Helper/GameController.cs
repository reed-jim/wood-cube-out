//using MoreMountains.NiceVibrations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Falcon.FalconCore.Scripts;
using GPlay.GplayGoogleUMP;
using Falcon.FalconAnalytics.Scripts.Models.Messages.PreDefines;
using UnityEngine.SceneManagement;
using Falcon.FalconCore.Scripts.Logs;


#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif

public class GameController : MonoBehaviour
{
    public static GameController Instance;
    // public UseProfile useProfile;
    public AdmobAdsMax admobAds;
    public AnalyticsController AnalyticsController;
    public SegmentController segmentController;
    public ActionLimitQueue ActionLimitQueue;
    public UseProfile useProfile;
    public IapController iapController;
    public DataContain dataContain;
    public MoneyEffectController moneyEffectController;
    public GameLoading gameLoading;
    [HideInInspector] public SceneType currentScene;

    [HideInInspector] public bool openFeatureBox = false;
    public static bool isFirstLoadLevel = false;
    protected void Start()
    {
        Instance = this;
        DontDestroyOnLoad(this);

        useProfile.IsRemoveAds = true;

        Init();
    }
    public void Init()
    {
        Application.targetFrameRate = 60;
         // GameController.Instance.useProfile.CurrentLevel =  GameController.Instance.useProfile;

       
       // iapController.Init();
        //MMVibrationManager.SetHapticsActive(useProfile.OnVibration);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        ResetDay();
        UseProfile.NumPlayedInDay++;
        GplayUMP.ShowConsentForm((consert) =>
        {
            UseProfile.GDPRContentValue = consert;
#if UNITY_IOS
            if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                ATTrackingStatusBinding.RequestAuthorizationTracking();
            }
#endif
            InitLoadScene();
        });
       
        FalconMain.Init();
    }
   

    public void InitLoadScene()
    {
        segmentController.Init();
        // admobAds.Init();
        ActionLimitQueue.Init();
        gameLoading.Init();
    }

    public static void SetUserProperties()
    {
        if (UseProfile.IsFirstTimeInstall)
        {
            UseProfile.FirstTimeOpenGame = UnbiasedTime.Instance.Now();
            UseProfile.LastTimeOpenGame = UseProfile.FirstTimeOpenGame;
            UseProfile.IsFirstTimeInstall = false;

            //AnalyticsController.LogFirstOpen(0);
        }

        var lastTimeOpen = UseProfile.LastTimeOpenGame;
        UseProfile.RetentionD = (UnbiasedTime.Instance.Now() - UseProfile.FirstTimeOpenGame).Days;

        var dayPlayerd = (TimeManager.ParseTimeStartDay(UnbiasedTime.Instance.Now()) - TimeManager.ParseTimeStartDay(UseProfile.LastTimeOpenGame)).Days;
        if (dayPlayerd >= 1)
        {
            UseProfile.LastTimeOpenGame = UnbiasedTime.Instance.Now();
            UseProfile.DaysPlayed++;

            // AnalyticsController.LogFirstOpen(UseProfile.DaysPlayed);
        }

        AnalyticsController.SetUserProperties();


    }

    public void ResetDay()
    {
        ///Các loại ResetDay ở đây
        long time = TimeManager.CaculateTime(TimeManager.ParseTimeStartDay(UseProfile.LastTimeOnline), TimeManager.ParseTimeStartDay(UnbiasedTime.Instance.Now()));
        if (time >= 86400)
        {
            UseProfile.LastTimeOnline = UnbiasedTime.Instance.Now();

            UseProfile.NumberLoseLevelInDay = 0;



            UseProfile.SetTurnSuggetBuyItemVideo(GiftType.SuggetItem_BirdSort, 0);
            UseProfile.SetTurnSuggetBuyItemVideo(GiftType.ReturnItem_BirdSort, 0);
            UseProfile.SetTurnSuggetBuyItemVideo(GiftType.ShuffleItem_BirdSort, 0);
            UseProfile.SetTurnSuggetBuyItemVideo(GiftType.IceItem_BirdSort, 0);
            UseProfile.SetTurnSuggetBuyItemVideo(GiftType.BreakTheRule_BirdSort, 0);
            UseProfile.SetTurnSuggetBuyItemVideo(GiftType.AddTimeItem_BirdSort, 0);
            UseProfile.SetTurnSuggetBuyItemVideo(GiftType.BlockBrand_BirdSort, 0);
            UseProfile.SetTurnSuggetBuyItemVideo(GiftType.AddBrandItem_BirdSort, 0);

           
            UseProfile.NumPlayedInDay = 0;

            UseProfile.ResetDayMiniGame();

        }
    }

    private void OnApplicationQuit()
    {
        //AnalyticsController.SessionLog(Time.time);
    }
}
public enum SceneType
{
    StartLoading = 0,
    MainHome = 1,
    GamePlay = 2
}