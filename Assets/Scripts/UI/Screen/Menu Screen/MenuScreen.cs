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
    [SerializeField] private Button openDailyRewardPopupButton;
    [SerializeField] private RectTransform openRemoveAdPopupButtonRT;
    [SerializeField] private Button luckyWheelButton;
    [SerializeField] private Button weeklyTaskButton;
    [SerializeField] private LeanLocalizedTextMeshProUGUI localizedLevelText;
    [SerializeField] private Image topBarDivider;

    [SerializeField] private AudioListener menuScreenAudioListener;

    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private Vector2Variable canvasSize;

    #region PRIVATE FIELD
    private List<Tween> _tweens;
    private float _initialOpenRemoveAdPopupButtonPositionY;
    private Tween _openRemoveAdPopupButtonTween;
    private Tween _topBarDividerTween;
    #endregion

    public static event Action<ScreenRoute> switchRouteEvent;
    public static event Action<bool> showMenuBlackScreenEvent;

    private void Awake()
    {
        BottomBarItem.switchRouteEvent += OnScreenRouteSwitched;
        SaferioIAPManager.removeAdsEvent += OnAdsRemoved;
        SceneManager.sceneLoaded += OnSceneLoaded;

        localizedLevelText.textTranslatedEvent += OnLevelTextTranslated;

        startGameButton.onClick.AddListener(StartGame);
        openRemoveAdPopupButton.onClick.AddListener(OpenRemoveAdPopup);
        openDailyRewardPopupButton.onClick.AddListener(OpenDailyRewardPopup);
        luckyWheelButton.onClick.AddListener(OpenLuckyWheelScreen);
        weeklyTaskButton.onClick.AddListener(OpenWeeklyTaskScreen);

        _tweens = new List<Tween>();

        _initialOpenRemoveAdPopupButtonPositionY = openRemoveAdPopupButtonRT.localPosition.y;
        topBarDivider.color = ColorUtil.WithAlpha(topBarDivider.color, 0);

        if (UserData.IsRemoveAds)
        {
            openRemoveAdPopupButton.gameObject.SetActive(false);
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

        if (UserData.IsFirstTimeOpenApp)
        {
            await Task.Delay(1500);

            StartGame();
        }

        PreventSpammingStartButton();
    }

    void OnDestroy()
    {
        BottomBarItem.switchRouteEvent -= OnScreenRouteSwitched;
        SaferioIAPManager.removeAdsEvent -= OnAdsRemoved;
        SceneManager.sceneLoaded -= OnSceneLoaded;

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
        Addressables.LoadSceneAsync(GameConstants.GAMEPLAY_SCENE, UnityEngine.SceneManagement.LoadSceneMode.Additive);
    }

    private void OpenRemoveAdPopup()
    {
        switchRouteEvent?.Invoke(ScreenRoute.RemoveAd);
    }

    private void OpenDailyRewardPopup()
    {
        switchRouteEvent?.Invoke(ScreenRoute.DailyReward);
    }

    private void OpenLuckyWheelScreen()
    {
        switchRouteEvent?.Invoke(ScreenRoute.LuckyWheel);
    }

    private void OpenWeeklyTaskScreen()
    {
        switchRouteEvent?.Invoke(ScreenRoute.WeeklyTask);
    }

    private void PlayClickSound()
    {
        AudioSource clickSound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.CLICK_SOUND);

        clickSound.Play();
    }

    private void UpdateCurrentLevelText()
    {
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

    private void ShowOpenRemoveAdPopupButtonOnScreenRouteSwitch(ScreenRoute screenRoute)
    {
        if (UserData.IsRemoveAds)
        {
            return;
        }

        CommonUtil.StopTween(_openRemoveAdPopupButtonTween);

        if (screenRoute != ScreenRoute.IAPShop)
        {
            _openRemoveAdPopupButtonTween = Tween.LocalPositionY(openRemoveAdPopupButtonRT, _initialOpenRemoveAdPopupButtonPositionY, 0.3f);
        }
        else
        {
            _openRemoveAdPopupButtonTween = Tween.LocalPositionY(
                openRemoveAdPopupButtonRT, openRemoveAdPopupButtonRT.localPosition.y + 0.3f * GamePersistentVariable.canvasSize.y, 0.3f);
        }
    }

    private void ShowTopBarDivider(ScreenRoute screenRoute)
    {
        CommonUtil.StopTween(_topBarDividerTween);

        if (screenRoute == ScreenRoute.IAPShop)
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

            SceneManager.UnloadSceneAsync(GameConstants.HomeScene);
        }
    }
}
