using Crystal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
public enum ModeGame
{
    Campaign = 0,
    PvP1 = 1,
}
public enum StateGame
{
    Loading = 0,
    Playing = 1,
    Win = 2,
    Lose = 3,
    Pause = 4
}

public class GamePlayController : Singleton<GamePlayController>
{
    public PlayerContain playerContain;
    public GameScene gameScene;
    public Level level;
    public ReturnController returnController;
    public ItemController itemController;
    public CameraFit cameraFit;
    public ScoreIQController scoreController;
    public IQScoreController iqScoreController;
    public ChallengeGameplayController challengeGameplayController;
    public ItemShowFarmeController itemShowFarmeController;
    public ThemeController themeController;

    public TutorialFunController tutCoreGameController;
    public TutorialFunController tutItemsController;
    public TutorialFunController tutExtralController;
    public TutorialFunController tutWeeklyChallengeController;
    

    public StateGame state;

    private static bool isBannerShow;
    [Header("Safe Area")]
    public SafeArea safeArea;


    protected override void OnAwake()
    {
        GameController.Instance.currentScene = SceneType.GamePlay;
        cameraFit.Init();
    }

    public void Start()
    {
        Init();
        InitState();
    }

    public void Init()
    {
#if UNITY_IOS
if (safeArea != null)
            safeArea.enabled = true;
#endif
        //! WARNING: KHÔNG THAY ĐỔI THỨ TỰ INIT
        GameController.Instance.skillUserController.AddValueCondition(ConditionChangeSkill.TypeCondition.TimeWaitingLevel, 1);
        GameController.Instance.skillUserController.CheckCurrentSkill();
        playerContain.Init();
        gameScene.Init();
        InitLevel();
        returnController.Init();
        itemController.Init();
        scoreController.Init();
        iqScoreController.Init();
        itemShowFarmeController.Init();
        challengeGameplayController.Init();
        themeController.Init();

        tutCoreGameController.Init();
        tutItemsController.Init();
        tutExtralController.Init();
        tutWeeklyChallengeController.Init();

        GameController.Instance.admobAds.DestroyBanner();
        GameController.Instance.admobAds.ShowBanner();


        isBannerShow = true;

        //GameController.Instance.musicManager.PlayBGMusicLevel();
    }

    public void InitState()
    {
        cameraFit.InitState();
        if (level != null)
        {
            level.InitState();
        }
    }    

    public void InitLevel()
    {
        if (level != null)
        {
            state = StateGame.Loading;
            level.Init();

        }


    }
    private void CheckShowFeatureBox()
    {
        if (GameController.Instance.openFeatureBox)
        {
            GameController.Instance.openFeatureBox = false;

        }
    }

    public bool IsShowRate()
    {
        if (!UseProfile.CanShowRate)
            return false;
        int X = GameController.Instance.useProfile.CurrentLevelPlay - 1;
        if (X < RemoteConfigController.GetFloatConfig(FirebaseConfig.LEVEL_START_SHOW_RATE, 5))
            return false;
        if (X == RemoteConfigController.GetFloatConfig(FirebaseConfig.LEVEL_START_SHOW_RATE, 5) || (X <= RemoteConfigController.GetIntConfig(FirebaseConfig.MAX_LEVEL_SHOW_RATE, 31) && X % 10 == 1))
        {
            return true;
        }
        return false;
    }

    public void PlayAnimFly()
    {

    }

    public void PlayAnimFlyOut()
    {

    }

    private void Update()
    {

    }


    
}
