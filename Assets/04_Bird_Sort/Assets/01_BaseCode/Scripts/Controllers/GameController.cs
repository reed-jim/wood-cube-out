//using MoreMountains.NiceVibrations;
using System.Collections;
using System.Collections.Generic;
using BirdSort2;
using UnityEngine;
using Falcon.FalconCore.Scripts;
using GPlay.GplayGoogleUMP;
using Falcon.FalconAnalytics.Scripts.Models.Messages.PreDefines;
using UnityEngine.SceneManagement;
using Falcon.FalconCore.Scripts.Logs;



#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif
using Gplay.Notifications;

public class GameController : MonoBehaviour
{
    public static GameController Instance;
    public UseProfile useProfile;
    public DataContain dataContain;
    public MusicManagerGameBase_BirdSort musicManager;
    public AdmobAds admobAds;
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
    public EventSaleController eventSaleController;
    public NotificationController notificationController;
    public SkillUserController skillUserController;
    public EventPackController eventPackController;
    [HideInInspector] public SceneType currentScene;

    public ModeGame modeGame;
    [HideInInspector] public bool openFeatureBox = false;

    [SerializeField] private StartLoading startLoading;
    [SerializeField] private LoadingPanel loadingAction;
    public static bool isFirstLoadLevel = false;
    protected void Start()
    {
        Instance = this;
        DontDestroyOnLoad(this);
        Init();
    }
    public void Init()
    {
        Application.targetFrameRate = 60;
        playerData = new PlayerData();
        useProfile.CurrentLevelPlay = UseProfile.CurrentLevel;

        musicManager.Init();
        iapController.Init();
        packController.Init();
        videoPackController.Init();
        challengeController.Init();
        eventSaleController.Init();
        loginController.Init();
        fXController.Init();
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
            if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                ATTrackingStatusBinding.RequestAuthorizationTracking();
            }
#endif
            InitLoadScene();
        });
        notificationController.Initialize();
        FalconMain.Init();
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

    public void LoadScene(string sceneName, bool showMrec = true)
    {
        if (segmentController.GetCurrentSegment().adsNewData.config.mrec.CheckActive(AdsNewSegmentData.BannerMrec.MrecType.Loading))
            loadingAction.GotoScene(sceneName, showMrec);
        else
            Initiate.Fade(sceneName.ToString(), Color.black, 2f);
        try
        {
            //new FEventLog(FalconLog.OPEN_SCENE_ + sceneName).Send();
            //Debug.Log(FalconLog.OPEN_SCENE_ + sceneName);
        }
        catch
        {

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



            UseProfile.SetTurnSuggetBuyItemVideo(GiftType.SuggetItem_BirdSort, 0);
            UseProfile.SetTurnSuggetBuyItemVideo(GiftType.ReturnItem_BirdSort, 0);
            UseProfile.SetTurnSuggetBuyItemVideo(GiftType.ShuffleItem_BirdSort, 0);
            UseProfile.SetTurnSuggetBuyItemVideo(GiftType.IceItem_BirdSort, 0);
            UseProfile.SetTurnSuggetBuyItemVideo(GiftType.BreakTheRule_BirdSort, 0);
            UseProfile.SetTurnSuggetBuyItemVideo(GiftType.AddTimeItem_BirdSort, 0);
            UseProfile.SetTurnSuggetBuyItemVideo(GiftType.BlockBrand_BirdSort, 0);
            UseProfile.SetTurnSuggetBuyItemVideo(GiftType.AddBrandItem_BirdSort, 0);

            packController.ResetDataNewDay();
            videoPackController.ResetDataNewDay();

            ReviveBox.CurrentTurnSuggetReviveVideo = 0;
            challengeController.ResetDay();
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

        if (eventSaleController.IsHasEventSaleReward())
            return true;

        foreach (var item in eventSaleController.lstEventSales)
        {
            if (UseProfile.CurrentLevel == item.LevelBackToHome())
            {
                return true;
            }
        }

        if (LevelChestController.IsHasChest())
            return true;

        if (StartChestController.IsHasChest())
            return true;


        if (!SceneManager.GetActiveScene().name.Equals(SceneName.GAME_PLAY_CHALLENGE))
        {
            if (UseProfile.CurrentLevel > Context.MAX_LEVEL)
            {
                return true;
            }

            string pathLevel = StringHelper.PATH_CONFIG_LEVEL;
            TextAsset lvJson = Resources.Load<TextAsset>(string.Format(pathLevel, UseProfile.CurrentLevel));
            if (lvJson == null) return true;
        }
        //if (segmentController.GetCurrentSegment().storySegmentData.config.is_come_home_when_can_build)
        //    if (storyController.IsHasElementCanBuildNoti())
        //        return true;

        return false;
    }

    public bool CheckEndLevelChallenge()
    {
        string pathLevel = StringHelper.PATH_CONFIG_LEVEL;
        TextAsset lvJson = Resources.Load<TextAsset>(string.Format(pathLevel,
            GameController.Instance.segmentController.GetCurrentSegment().challengeModeData.config.levelStart +
GameController.Instance.useProfile.CurrentHardLevel * 5));
        if (lvJson == null) return true;
        return false;
    }

    private void OnApplicationQuit()
    {
        //AnalyticsController.SessionLog(Time.time);
    }

    private void TestLevel()
    {
        for (int i = 0; i < 522; i++)
        {
            string pathLevel = StringHelper.PATH_CONFIG_LEVEL;
            TextAsset lvJson = Resources.Load<TextAsset>(string.Format(pathLevel, i + 1));
            if (lvJson != null)
            {
                Debug.Log("Level " + (i + 1));
            }
            else
            {
                Debug.LogError("Thieu level " + (i + 1));
            }
        }

    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space))
    //        TestLevel();
    //}
}
public enum SceneType
{
    StartLoading = 0,
    MainHome = 1,
    GamePlay = 2
}