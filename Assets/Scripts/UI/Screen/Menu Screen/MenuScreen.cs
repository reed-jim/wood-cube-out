using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lean.Localization;
using PrimeTween;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameEnum;

public class MenuScreen : MonoBehaviour
{
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button openRemoveAdPopupButton;
    [SerializeField] private Button openAllScrewBoxesForeverButton;
    [SerializeField] private CanvasGroup buttonsCanvasGroup;
    [SerializeField] private RectTransform buttonsCanvasGroupRT;
    [SerializeField] private Button openDailyRewardPopupButton;
    [SerializeField] private Button openGoldPassPopupButton;
    [SerializeField] private Button openEndlessSalePopupButton;
    [SerializeField] private Button openWeeklyChallengePopupButton;
    [SerializeField] private Button openStarChestPopupButton;
    [SerializeField] private Button openChestLevelPopupButton;
    [SerializeField] private Button openCoinChallengePopupButton;
    [SerializeField] private Button openStarTournamentPopupButton;
    [SerializeField] private Button openMedalTournamentPopupButton;
    [SerializeField] private Button openSpaceRacePopupButton;
    [SerializeField] private RectTransform openRemoveAdPopupButtonRT;
    [SerializeField] private Button luckyWheelButton;
    [SerializeField] private Button weeklyTaskButton;

    #region SALE PACK BUTTONS
    [SerializeField] private Button[] openSaleIAPPackButtons;
    // [SerializeField] private Button openDailyOfferButton;
    // [SerializeField] private Button openStarterPackButton;
    // [SerializeField] private Button openSaleItemButton;
    // [SerializeField] private Button openSaleCoinButton;
    // [SerializeField] private Button openWeekendSaleButton;
    // [SerializeField] private Button openMasterOfferButton;
    // [SerializeField] private Button openMegaOfferButton;
    // [SerializeField] private Button openGiantOfferButton;
    #endregion

    [SerializeField] private LeanLocalizedTextMeshProUGUI localizedLevelText;
    [SerializeField] private Image topBarDivider;

    [SerializeField] private AudioListener menuScreenAudioListener;

    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private Vector2Variable canvasSize;

    #region PRIVATE FIELD
    private List<Tween> _tweens;
    private float _initialOpenRemoveAdPopupButtonPositionY;
    private Tween _openRemoveAdPopupButtonTween;
    private Tween _openUnlockScrewBoxesPopupButtonTween;
    private Tween _topBarDividerTween;
    #endregion

    public static event Action<ScreenRoute> switchRouteEvent;
    public static event Action<bool> showMenuBlackScreenEvent;
    public static event Action<string, string> showNotificationEvent;
    public static event Action<string, string, string, string> showNotificationWithParamEvent;

    private void Awake()
    {
        GameVariableInitializer.currentLevelFetchedEvent += UpdateCurrentLevelText;
        BackupPopup.updateLevelEvent += UpdateCurrentLevelTextOnBackup;
        CompensatePopup.updateLevelEvent += UpdateCurrentLevelTextOnBackup;
        BackupPopup.updateRemoveAdsStateEvent += UpdateRemoveAdsState;
        BottomBarItem.switchRouteEvent += OnScreenRouteSwitched;
        ResourceEarnPopup.removeAdsEvent += OnAdsRemoved;
        SaferioIAPManager.removeAdsEvent += OnAdsRemoved;
        SaferioIAPManager.unlockScrewBoxesPurchasedCompletedEvent += OnAllScrewBoxesUnlockedForever;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SpaceRacePopup.startGameEvent += StartGame;
        SalePackIAPManager.suggestSalePackEvent += ShowSalePack;
        GameEventManager.suggestCoinChallengeEvent += OpenCoinChallengePopup;
        StarChestProgressBar.openStarChestPopupEvent += OpenStarChestPopup;
        LevelChestProgressBar.openLevelChestPopupEvent += OpenChestLevelPopup;
        CoinChallengeProgressBar.openCoinChallengePopupEvent += OpenCoinChallengePopup;
        GameEventManager.openStarChestPopupEvent += OpenStarChestPopup;
        GameEventManager.openLevelChestPopupEvent += OpenChestLevelPopup;
        GameEventManager.openCoinChallengePopupEvent += OpenCoinChallengePopup;
        RemoteConfigController.remoteConfigReadyEvent += OnRemoteConfigReady;

        localizedLevelText.textTranslatedEvent += OnLevelTextTranslated;

        startGameButton.onClick.AddListener(StartGame);
        openRemoveAdPopupButton.onClick.AddListener(OpenRemoveAdPopup);
        openAllScrewBoxesForeverButton.onClick.AddListener(OpenUnlockAllScrewBoxesForeverPopup);
        openDailyRewardPopupButton.onClick.AddListener(OpenDailyRewardPopup);
        openGoldPassPopupButton.onClick.AddListener(OpenGoldPassPopup);
        openEndlessSalePopupButton.onClick.AddListener(OpenEndlessSalePopup);
        openWeeklyChallengePopupButton.onClick.AddListener(OpenWeeklyChallengePopup);
        openStarChestPopupButton.onClick.AddListener(OpenStarChestPopup);
        openChestLevelPopupButton.onClick.AddListener(OpenChestLevelPopup);
        openCoinChallengePopupButton.onClick.AddListener(OpenCoinChallengePopup);
        openStarTournamentPopupButton.onClick.AddListener(OpenStarTournamentPopup);
        openMedalTournamentPopupButton.onClick.AddListener(OpenMedalTournamentPopup);
        openSpaceRacePopupButton.onClick.AddListener(OpenSpaceRacePopup);

        #region SALE IAP PACKS
        openSaleIAPPackButtons[0].onClick.AddListener(OpenDailyOfferPopup);
        openSaleIAPPackButtons[1].onClick.AddListener(OpenStarterPackPopup);
        openSaleIAPPackButtons[2].onClick.AddListener(OpenSaleItemPopup);
        openSaleIAPPackButtons[3].onClick.AddListener(OpenSaleCoinPopup);
        openSaleIAPPackButtons[4].onClick.AddListener(OpenWeekendSalePopup);
        openSaleIAPPackButtons[5].onClick.AddListener(OpenMasterOfferPopup);
        openSaleIAPPackButtons[6].onClick.AddListener(OpenMegaOfferPopup);
        openSaleIAPPackButtons[7].onClick.AddListener(OpenGiantOfferPopup);
        #endregion

        luckyWheelButton.onClick.AddListener(OpenLuckyWheelScreen);
        weeklyTaskButton.onClick.AddListener(OpenWeeklyTaskScreen);

        _tweens = new List<Tween>();

        _initialOpenRemoveAdPopupButtonPositionY = openRemoveAdPopupButtonRT.localPosition.y;
        topBarDivider.color = ColorUtil.WithAlpha(topBarDivider.color, 0);

        if (UserData.IsRemoveAds)
        {
            openRemoveAdPopupButton.gameObject.SetActive(false);
        }

        if (DataUtility.Load(GameConstants.IS_UNLOCKED_ALL_SCREW_BOXES, false))
        {
            openAllScrewBoxesForeverButton.gameObject.SetActive(false);
        }

        currentLevel.Load();
        canvasSize.Load();

        localizedLevelText.UpdateTranslationWithParameter(GameConstants.LEVEL_PARAMETER, $"{GetDisplayedLevel()}");

        // PreventSpammingStartButton();
    }

    private async Task Start()
    {
        bool isFirstTimeOpenApp = DataUtility.Load(GameConstants.IS_FIRST_TIME_OPEN_APP, true);

        showMenuBlackScreenEvent?.Invoke(isFirstTimeOpenApp);

        PreventSpammingStartButton();

        if (UserData.IsFirstTimeOpenApp)
        {
            await Task.Delay(1500);

            StartGame();
        }

        SetupEvent();
    }

    void OnDestroy()
    {
        GameVariableInitializer.currentLevelFetchedEvent -= UpdateCurrentLevelText;
        BackupPopup.updateLevelEvent -= UpdateCurrentLevelTextOnBackup;
        CompensatePopup.updateLevelEvent -= UpdateCurrentLevelTextOnBackup;
        BackupPopup.updateRemoveAdsStateEvent -= UpdateRemoveAdsState;
        BottomBarItem.switchRouteEvent -= OnScreenRouteSwitched;
        ResourceEarnPopup.removeAdsEvent -= OnAdsRemoved;
        SaferioIAPManager.removeAdsEvent -= OnAdsRemoved;
        SaferioIAPManager.unlockScrewBoxesPurchasedCompletedEvent -= OnAllScrewBoxesUnlockedForever;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SpaceRacePopup.startGameEvent -= StartGame;
        SalePackIAPManager.suggestSalePackEvent -= ShowSalePack;
        GameEventManager.suggestCoinChallengeEvent -= OpenCoinChallengePopup;
        StarChestProgressBar.openStarChestPopupEvent -= OpenStarChestPopup;
        LevelChestProgressBar.openLevelChestPopupEvent -= OpenChestLevelPopup;
        CoinChallengeProgressBar.openCoinChallengePopupEvent -= OpenCoinChallengePopup;
        GameEventManager.openStarChestPopupEvent -= OpenStarChestPopup;
        GameEventManager.openLevelChestPopupEvent -= OpenChestLevelPopup;
        GameEventManager.openCoinChallengePopupEvent -= OpenCoinChallengePopup;
        RemoteConfigController.remoteConfigReadyEvent -= OnRemoteConfigReady;

        localizedLevelText.textTranslatedEvent -= OnLevelTextTranslated;

        CommonUtil.StopAllTweens(_tweens);
    }

    private void PreventSpammingStartButton()
    {
        startGameButton.interactable = false;

        _tweens.Add(Tween.Delay(0.5f).OnComplete(() =>
        {
            startGameButton.interactable = true;

            // startGameButton.gameObject.SetActive(true);

            // startGameButton.transform.localScale = Vector3.one;

            // _tweens.Add(Tween.Scale(startGameButton.transform, 1.1f, duration: 0.3f)
            // .OnComplete(() =>
            // {
            //     _tweens.Add(Tween.Scale(startGameButton.transform, 1f, duration: 0.1f)
            //     .OnComplete(() =>
            //     {
            //         startGameButton.interactable = true;
            //     }));
            // }));
        }));
    }

    private void OnLevelTextTranslated()
    {
        localizedLevelText.UpdateTranslationWithParameter(GameConstants.LEVEL_PARAMETER, $"{GetDisplayedLevel()}");
    }

    private void StartGame()
    {
        // if (currentLevel.Value > 10)
        // {
        //     if (!GamePersistentVariable.isRemoteLevelsDownloaded)
        //     {
        //         showNotificationEvent?.Invoke("Try again later", "Data is being downloaded. Please wait a moment.");

        //         return;
        //     }
        // }

        if (
            GamePersistentVariable.livesConfig.isActive &&
            DataUtility.Load(GameConstants.USER_LIVES_DATA, new LivesData()).CurrentLives == 0
        )
        {
            switchRouteEvent?.Invoke(ScreenRoute.LivesShop);

            return;
        }

        if (GamePersistentVariable.isEnableLoadingLevelInProgress)
        {
            GamePersistentVariable.isLoadingLevelInProgress = false;

            if (CommonUtil.IsLevelInProgress(currentLevel.Value))
            {
                switchRouteEvent?.Invoke(ScreenRoute.LevelInProgress);
            }
            else
            {
                Addressables.LoadSceneAsync(GameConstants.GAMEPLAY_SCENE, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            }
        }
        else
        {
            Addressables.LoadSceneAsync(GameConstants.GAMEPLAY_SCENE, UnityEngine.SceneManagement.LoadSceneMode.Additive);
        }

        startGameButton.interactable = false;

        // PreventSpammingStartButton();
    }

    private void OpenRemoveAdPopup()
    {
        switchRouteEvent?.Invoke(ScreenRoute.RemoveAd);
    }

    private void OpenUnlockAllScrewBoxesForeverPopup()
    {
        switchRouteEvent?.Invoke(ScreenRoute.UnlockAllScrewBoxesForever);
    }

    private void OpenDailyRewardPopup()
    {
        DailyRewardConfig dailyRewardConfig = GamePersistentVariable.dailyRewardConfig;

        if (dailyRewardConfig.IsAvailable(currentLevel.Value))
        {
            switchRouteEvent?.Invoke(ScreenRoute.DailyReward);
        }
        else
        {
            showNotificationWithParamEvent?.Invoke("Locked", "Unlocked at Level", GameConstants.LEVEL_PARAMETER, $"{dailyRewardConfig.unlockLevel}");
        }
    }

    private void OpenGoldPassPopup()
    {
        if (GamePersistentVariable.goldPassData == null)
        {
            GamePersistentVariable.goldPassData = DataUtility.Load(GameConstants.GOLD_PASS_DATA_CONTAINER, new GoldPassData());
        }

        if (GamePersistentVariable.goldPassData.isIntroDone)
        {
            switchRouteEvent?.Invoke(ScreenRoute.GoldPass);
        }
        else
        {
            switchRouteEvent?.Invoke(ScreenRoute.GoldPassIntro);
        }
    }

    private void OpenEndlessSalePopup()
    {
        switchRouteEvent?.Invoke(ScreenRoute.EndlessSale);
    }

    private void OpenWeeklyChallengePopup()
    {
        switchRouteEvent?.Invoke(ScreenRoute.WeeklyChallenge);
    }

    private void OpenStarChestPopup()
    {
        switchRouteEvent?.Invoke(ScreenRoute.StarChest);
    }

    private void OpenChestLevelPopup()
    {
        switchRouteEvent?.Invoke(ScreenRoute.ChestLevel);
    }

    private void OpenCoinChallengePopup()
    {
        if (currentLevel.Value >= 30)
        {
            switchRouteEvent?.Invoke(ScreenRoute.CoinChallenge);
        }
        else
        {
            showNotificationWithParamEvent?.Invoke("Locked", "Unlocked at Level", GameConstants.LEVEL_PARAMETER, $"{30}");
        }
    }

    private void OpenStarTournamentPopup()
    {
        switchRouteEvent?.Invoke(ScreenRoute.StarTournament);
    }

    private void OpenMedalTournamentPopup()
    {
        if (GamePersistentVariable.medalTournamentData == null)
        {
            GamePersistentVariable.medalTournamentData = DataUtility.Load(GameConstants.MEDAL_TOURNAMENT_DATA, new MedalTournamentData());
        }

        if (GamePersistentVariable.medalTournamentData.isIntroDone)
        {
            switchRouteEvent?.Invoke(ScreenRoute.MedalTournament);
        }
        else
        {
            switchRouteEvent?.Invoke(ScreenRoute.MedalTournamentIntro);
        }
    }

    private void OpenSpaceRacePopup()
    {
        SpaceRaceData spaceRaceData = DataUtility.Load(GameConstants.SPACE_RACE_DATA, new SpaceRaceData());

        if (spaceRaceData.isStarted)
        {
            switchRouteEvent?.Invoke(ScreenRoute.SpaceRace);
        }
        else
        {
            switchRouteEvent?.Invoke(ScreenRoute.JoinSpaceRace);
        }
    }

    private void OpenIAPOfferPopup()
    {
        switchRouteEvent?.Invoke(ScreenRoute.DailyOffer);
    }

    private void OpenLuckyWheelScreen()
    {
        switchRouteEvent?.Invoke(ScreenRoute.LuckyWheel);
    }

    private void OpenWeeklyTaskScreen()
    {
        switchRouteEvent?.Invoke(ScreenRoute.WeeklyTask);
    }

    #region SALE PACKS
    private void OpenDailyOfferPopup()
    {
        switchRouteEvent?.Invoke(ScreenRoute.DailyOffer);
    }

    private void OpenStarterPackPopup()
    {
        switchRouteEvent?.Invoke(ScreenRoute.StarterPack);
    }

    private void OpenSaleItemPopup()
    {
        switchRouteEvent?.Invoke(ScreenRoute.SaleItem);
    }

    private void OpenSaleCoinPopup()
    {
        switchRouteEvent?.Invoke(ScreenRoute.SaleCoin);
    }

    private void OpenWeekendSalePopup()
    {
        switchRouteEvent?.Invoke(ScreenRoute.WeekendSale);
    }

    private void OpenMasterOfferPopup()
    {
        switchRouteEvent?.Invoke(ScreenRoute.MasterOffer);
    }

    private void OpenMegaOfferPopup()
    {
        switchRouteEvent?.Invoke(ScreenRoute.MegaOffer);
    }

    private void OpenGiantOfferPopup()
    {
        switchRouteEvent?.Invoke(ScreenRoute.GiantOffer);
    }

    private void ShowSalePack(SaleIAPPackType packType)
    {
        return;

        openRemoveAdPopupButton.gameObject.SetActive(false);

        for (int i = 0; i < openSaleIAPPackButtons.Length; i++)
        {
            openSaleIAPPackButtons[i].gameObject.SetActive(false);
        }

        try
        {
            openSaleIAPPackButtons[(int)packType].gameObject.SetActive(true);

            // if (packType == SaleIAPPackType.DailyOffer)
            // {
            //     OpenDailyOfferPopup();
            // }
            // else if (packType == SaleIAPPackType.StarterPack)
            // {
            //     OpenStarterPackPopup();
            // }
            // else if (packType == SaleIAPPackType.SaleItem)
            // {
            //     OpenSaleItemPopup();
            // }
            // else if (packType == SaleIAPPackType.SaleCoin)
            // {
            //     OpenSaleCoinPopup();
            // }
            // else if (packType == SaleIAPPackType.WeekendSale)
            // {
            //     OpenWeekendSalePopup();
            // }
            // else if (packType == SaleIAPPackType.MasterOffer)
            // {
            //     OpenMasterOfferPopup();
            // }
            // else if (packType == SaleIAPPackType.MegaOffer)
            // {
            //     OpenMegaOfferPopup();
            // }
            // else if (packType == SaleIAPPackType.GiantOffer)
            // {
            //     OpenGiantOfferPopup();
            // }
        }
        catch (System.Exception)
        {

        }
    }

    #endregion

    #region EVENT
    private void SetupEvent()
    {
        openSpaceRacePopupButton.gameObject.SetActive(false);
        // openSpaceRacePopupButton.gameObject.SetActive(GameEventManager.Instance.IsSpaceRaceUnlocked());
        openCoinChallengePopupButton.gameObject.SetActive(GameEventManager.Instance.IsShowCoinChallengeButton());

        #region DAILY REWARD
        // bool isDailyRewardAvailable = GamePersistentVariable.dailyRewardConfig.IsAvailable(currentLevel.Value);

        // openDailyRewardPopupButton.gameObject.SetActive(isDailyRewardAvailable);
        #endregion

        #region STAR CHEST
        // StarChestConfig starChestConfig;

        // RemoteConfigController.GetJsonConfig(FirebaseConfig.REMOTE_CONFIG_LEVEL_LIST, out starChestConfig);

        // openStarChestPopupButton.gameObject.SetActive(starChestConfig.isActive);

        // GameConstants.REQUIRE_STAR = starChestConfig.requiredStar;
        #endregion
    }

    private void OnRemoteConfigReady()
    {
        // bool isDailyRewardAvailable = GamePersistentVariable.dailyRewardConfig.IsAvailable(currentLevel.Value);

        // openDailyRewardPopupButton.gameObject.SetActive(isDailyRewardAvailable);
    }
    #endregion

    private void PlayClickSound()
    {
        AudioSource clickSound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.CLICK_SOUND);

        clickSound.Play();
    }

    private void UpdateCurrentLevelText()
    {
        localizedLevelText.UpdateTranslationWithParameter(GameConstants.LEVEL_PARAMETER, $"{GetDisplayedLevel()}");
    }

    private void UpdateCurrentLevelTextOnBackup()
    {
        currentLevel.Load();

        localizedLevelText.UpdateTranslationWithParameter(GameConstants.LEVEL_PARAMETER, $"{GetDisplayedLevel()}");
    }

    private void OnScreenRouteSwitched(ScreenRoute screenRoute)
    {
        ShowOpenRemoveAdPopupButtonOnScreenRouteSwitch(screenRoute);
        ShowTopBarDivider(screenRoute);
    }

    private void OnAdsRemoved()
    {
        openRemoveAdPopupButton.gameObject.SetActive(false);
    }

    private void OnAllScrewBoxesUnlockedForever()
    {
        openAllScrewBoxesForeverButton.gameObject.SetActive(false);
    }

    private void UpdateRemoveAdsState()
    {
        if (UserData.IsRemoveAds)
        {
            openRemoveAdPopupButton.gameObject.SetActive(false);

            AdmobAdsMax.Instance.HideBanner();
            AdmobAdsMax.Instance.DestroyBanner();
        }
    }

    private void ShowOpenRemoveAdPopupButtonOnScreenRouteSwitch(ScreenRoute screenRoute)
    {
        // if (UserData.IsRemoveAds)
        // {
        //     return;
        // }

        // CommonUtil.StopTween(_openRemoveAdPopupButtonTween);

        // if (screenRoute == ScreenRoute.Home)
        // {
        //     _openRemoveAdPopupButtonTween = Tween.LocalPositionY(openRemoveAdPopupButtonRT, _initialOpenRemoveAdPopupButtonPositionY, 0.3f);
        // }
        // else
        // {
        //     _openRemoveAdPopupButtonTween = Tween.LocalPositionY(
        //         openRemoveAdPopupButtonRT, openRemoveAdPopupButtonRT.localPosition.y + 0.3f * GamePersistentVariable.canvasSize.y, 0.3f);
        // }

        // Unlock All Screw boxes
        CommonUtil.StopTween(_openUnlockScrewBoxesPopupButtonTween);

        if (screenRoute == ScreenRoute.Home)
        {
            buttonsCanvasGroup.interactable = true;

            _openUnlockScrewBoxesPopupButtonTween = Tween.LocalPositionX(buttonsCanvasGroupRT, 0, duration: 0.3f);

            // _openUnlockScrewBoxesPopupButtonTween = Tween.Alpha(buttonsCanvasGroup, 1, duration: 0.3f);
        }
        else
        {
            float positionX = 1.5f * GamePersistentVariable.canvasSize.x;

            // leaderboard
            if (screenRoute == ScreenRoute.Screen2)
            {
                positionX = -1.5f * GamePersistentVariable.canvasSize.x;
            }

            buttonsCanvasGroup.interactable = false;

            _openUnlockScrewBoxesPopupButtonTween = Tween.LocalPositionX(buttonsCanvasGroupRT, positionX, duration: 0.3f);

            // _openUnlockScrewBoxesPopupButtonTween = Tween.Alpha(buttonsCanvasGroup, 0, duration: 0.3f);
        }
    }

    private void ShowTopBarDivider(ScreenRoute screenRoute)
    {
        CommonUtil.StopTween(_topBarDividerTween);

        if (screenRoute != ScreenRoute.Home)
        {
            _topBarDividerTween = Tween.Alpha(topBarDivider, 1, 0.3f);
        }
        else
        {
            _topBarDividerTween = Tween.Alpha(topBarDivider, 0, 0.3f);
        }
    }

    private int GetDisplayedLevel()
    {
        return currentLevel.Value;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == GameConstants.GAMEPLAY_SCENE)
        {
            menuScreenAudioListener.enabled = false;

            SceneManager.UnloadSceneAsync(GameConstants.MENU_SCENE);
        }
    }
}
