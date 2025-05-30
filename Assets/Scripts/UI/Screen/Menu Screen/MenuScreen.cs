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
    [SerializeField] private Button openIAPOfferPopupButton;
    [SerializeField] private RectTransform openRemoveAdPopupButtonRT;
    [SerializeField] private Button luckyWheelButton;
    [SerializeField] private Button weeklyTaskButton;

    #region SALE PACK BUTTONS
    [SerializeField] private Button openDailyOfferButton;
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
        openIAPOfferPopupButton.onClick.AddListener(OpenIAPOfferPopup);
        openDailyOfferButton.onClick.AddListener(OpenDailyOfferPopup);
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
        switchRouteEvent?.Invoke(ScreenRoute.DailyReward);
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
        switchRouteEvent?.Invoke(ScreenRoute.CoinChallenge);
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

    private void ShowSalePack()
    {
        openRemoveAdPopupButton.gameObject.SetActive(false);
        openDailyOfferButton.gameObject.SetActive(true);
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
            buttonsCanvasGroup.interactable = false;

            _openUnlockScrewBoxesPopupButtonTween = Tween.LocalPositionX(buttonsCanvasGroupRT, 1.5f * GamePersistentVariable.canvasSize.x, duration: 0.3f);

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
