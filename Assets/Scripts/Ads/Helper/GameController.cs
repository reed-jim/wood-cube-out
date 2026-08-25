using UnityEngine;

#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif

public class GameController : MonoBehaviour
{
    public static GameController Instance;
    public SegmentController segmentController;
    public ActionLimitQueue ActionLimitQueue;
    public UseProfile useProfile;
    public IapController iapController;
    public DataContain dataContain;
    public MoneyEffectController moneyEffectController;
    [HideInInspector] public SceneType currentScene;

    [HideInInspector] public bool openFeatureBox = false;
    public static bool isFirstLoadLevel = false;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
        Init();
    }

    public void Init()
    {
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        ResetDay();
        UseProfile.NumPlayedInDay++;

        InitLoadScene();
    }

    public void InitLoadScene()
    {
        segmentController.Init();
        ActionLimitQueue.Init();
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
}

public enum SceneType
{
    StartLoading = 0,
    MainHome = 1,
    GamePlay = 2
}
