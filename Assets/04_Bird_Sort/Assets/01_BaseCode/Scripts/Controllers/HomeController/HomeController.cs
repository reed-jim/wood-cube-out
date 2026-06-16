using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeController : Singleton<HomeController>
{
    protected override bool ShouldPersist() => false;

    public HomeScene homeScene;
    public PageController pageController;
    public DailyLoginController dailyLoginController;
    public SuggetPopupHomeController suggetPopupHomeController;
    [HideInInspector] public bool isInited;
    public ChallengeHomeController challengeHomeController;
    public MiniGameController miniGameController;
    public EventSaleViewBase eventBaseView;
    public HomeThemeController themeController;
    protected override void OnAwake()
    {
        BirdSortGameController.Instance.currentScene = BirdSortSceneType.MainHome;
        CheckHealth();
    }

    private void Start()
    {
        suggetPopupHomeController.Init();
        challengeHomeController.Init();
        eventBaseView.AwakeHomeSetup();
        themeController.Init();
        homeScene.Init();
        pageController.Init();

        BirdSortGameController.Instance.packController.CheckShowPopupSaleHome();
        BirdSortGameController.Instance.videoPackController.CheckShowPopupSaleHome();
        dailyLoginController.Init();
        isInited = true;
        if (IsShowRate())
        {
            suggetPopupHomeController.RegisterSuggetPopupShow();
            suggetPopupHomeController.AddActionShow(new SuggetPopupHomeController.SuggetPopupData()
            {
                actionShow = () =>
                {
                    DialogueRate.Setup(true).Show();
                },
                priovity = SuggetPopupHomeController.PriovitySugget.Important
            }
             );
        }

        if (IsShowChristmasEvent() && TimeManager.CaculateTime(UseProfile.LastTimeOpenMinigame, UnbiasedTime.Instance.Now()) >= BirdSortGameController.Instance.segmentController.GetCurrentSegment().minigameConnectSegmentData.config.time_delay_show)
        {
                suggetPopupHomeController.RegisterSuggetPopupShow();
                suggetPopupHomeController.AddActionShow(new SuggetPopupHomeController.SuggetPopupData()
                    {
                        actionShow = () =>
                        {
                            ConnectBirdMGBox.Setup(isPostEventPopupClose:true);
                        },
                        priovity = SuggetPopupHomeController.PriovitySugget.Normal
                    }
                );
            
        }
        /// BirdSortGameController.Instance.packController.CheckShowPopupSaleHome();

        //BirdSortGameController.Instance.admobAds.DestroyBanner();
       // BirdSortGameController.Instance.admobAds.ShowBanner();

        suggetPopupHomeController.CheckStartShowSugget();

       // BirdSortGameController.Instance.musicManager.PlayBGMusicHome();

        Level.isHasBeenWinLevel = false;

        if (!BirdSortGameController.isFirstLoadLevel)
        {
            //BirdSortGameController.Instance.admobAds.DestroyBanner();
            BirdSortGameController.Instance.admobAds.ShowBanner();
            BirdSortGameController.isFirstLoadLevel = true;

            //isBannerShow = true;
        }
        miniGameController.Init();
        // BirdSortGameController.Instance.segmentController.ConfigToJson();
    }

    public void CheckHealth()
    {
        if (UseProfile.IsRefillHealth || UseProfile.Health < GiftDatabase.DefaultItem[GiftType.Health])
        {
            var timeRefill = UseProfile.TimeLastOverHealth.AddSeconds(BirdSortGameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.healthConfig.timeReFillHealth);
            //var coolDownTime = TimeManager.CaculateTime(timeRefill, UnbiasedTime.Instance.Now());
            if (UnbiasedTime.Instance.Now() > timeRefill)
            {
                UseProfile.Health = GiftDatabase.DefaultItem[GiftType.Health];//Reload
                UseProfile.IsRefillHealth = false;
                return;
            }
        }

    }


    public void OnLeftHome()
    {
        BirdSortGameController.Instance.challengeController.OnLeftHome();
        BirdSortGameController.Instance.eventSaleController.OnLeftHome();
    }

    public void OnDestroy()
    {
        OnLeftHome();
    }

    public bool IsShowRate()
    {
        if (!UseProfile.CanShowRate)
            return false;


        int X = UseProfile.CurrentLevel - 1;
        var rateConfig = BirdSortGameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.rateConfig;

        if (!rateConfig.isActive)
            return false;
        if (X < rateConfig.levelStartShow)
            return false;
        if (X == rateConfig.levelStartShow || (X <= rateConfig.levelMaxShow && X % 10 == 1))
        {
            if (UseProfile.LevelShowedRate != X)
            {
                UseProfile.LevelShowedRate = X;
                return true;
            }
        }
        return false;
    }
    public bool IsShowChristmasEvent()
    {
        var config = BirdSortGameController.Instance.segmentController.GetCurrentSegment().minigameConnectSegmentData
            .config;
      
        if (TimeManager.CaculateTime2(UnbiasedTime.Instance.Now(), config.start_date)> 0 ||
            TimeManager.CaculateTime2(UnbiasedTime.Instance.Now(), config.end_date) < 0)
            return false;
        if (!config.IsActive || !(UseProfile.CurrentLevel >= config.min_level))
            return false;
        return true;
    }
}
