using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BirdSort2;
using GPlay.GplayGoogleUMP;
using Gplay.Notifications;
using PrimeTween;
#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif

public class GameController : MonoBehaviour
{
    public static GameController Instance;
    public UseProfile useProfile;
    public DataContain dataContain;
    public MusicManagerGameBase_BirdSort musicManager;
    public AdmobAdsMax admobAds;
    public AnalyticsController AnalyticsController;
    public IapController iapController;
    public SegmentController segmentController;
    public PackController packController;
    public SaleVideoController videoPackController;
    public PlayerData playerData;
    public MoneyEffectController moneyEffectController;
    public LoginController loginController;
    public PoolController poolController;
    public FXController fXController;
    public ChallengeController challengeController;
    public NotificationController notificationController;
    public SkillUserController skillUserController;
    [SerializeField] public SceneType currentScene;

    public ModeGame modeGame;
    [HideInInspector] public bool openFeatureBox = false;
    [SerializeField] private StartLoading startLoading;
    public static bool isFirstLoadLevel = false;
    protected void Start()
    {
        Instance = this;
        DontDestroyOnLoad(this);
        PrimeTweenConfig.warnTweenOnDisabledTarget = false;
        PrimeTweenConfig.warnEndValueEqualsCurrent = false;
        PrimeTweenConfig.warnZeroDuration = false;
        Init();
    }
    public void Init()
    {
        Application.targetFrameRate = 60;
        playerData = new PlayerData();
        useProfile.CurrentLevelPlay = UseProfile.CurrentLevel;

        segmentController.Init();
        musicManager.Init();
        iapController.Init();
        packController.Init();
        videoPackController.Init();
        challengeController.Init();
        loginController.Init();
        fXController.Init();
        //notificationController.Initialize();
        skillUserController.Init();
        //MMVibrationManager.SetHapticsActive(useProfile.OnVibration);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        ResetDay();
        UseProfile.NumPlayedInDay++;
        GameController.Instance.musicManager.PlayBGMusicHome();
        GplayUMP.ShowConsentForm((consert) =>
        {
            UseProfile.GDPRContentValue = consert;
#if UNITY_IOS
 if(ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                ATTrackingStatusBinding.RequestAuthorizationTracking();
            }
#endif
            InitLoadScene();
        });


    }
    public void InitLoadScene()
    {
        segmentController.Init();
        admobAds.Init();
        if (startLoading != null)
        {
            startLoading.Init();
        }
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
            ShopItemVideoCoin.NumTurnVideoCoinInDay = 0;

            UseProfile.NumberLoseLevelInDay = 0;

            UseProfile.NumPlayedInDay = 0;

            UseProfile.ResetDayMiniGame();

        }
    }
    public bool CheckHomeIsHasReward()
    {
        //if (UseProfile.CurrentLevel == 2)
        //{
        //    return true;
        //}

        if (challengeController.IsHasChallengeReward())
            return true;

        if (LevelChestController.IsHasChest())
            return true;

        if (StartChestController.IsHasChest())
            return true;

        if (UseProfile.CurrentLevel > Context.MAX_LEVEL)
        {
            return true;
        }

        //if (segmentController.GetCurrentSegment().storySegmentData.config.is_come_home_when_can_build)
        //    if (storyController.IsHasElementCanBuildNoti())
        //        return true;

        return false;
    }
    public void LoadScene(string sceneName)
    {
        Initiate.Fade(sceneName.ToString(), Color.black, 2f);
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