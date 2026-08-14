using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugPopup : BasePopup
{
    [SerializeField] private GameObject woodBackground;

    [SerializeField] private TMP_InputField levelInput;
    [SerializeField] private TMP_InputField backgroundColorInput;

    [Header("=== TEST CAMP ===")]
    [SerializeField] private TMP_InputField campTestInput; // nhập idCamp test, vd "testCamp0"
    [SerializeField] private Button applyCampTestButton;

    [SerializeField] private Button hideDebugButton;
    [SerializeField] private Button debugPlayLevelButton;
    [SerializeField] private Button debugNextLevelButton;
    [SerializeField] private Button debugPrevLevelButton;
    [SerializeField] private Button debugWinLevelButton;
    [SerializeField] private Button debugAdsButton;
    [SerializeField] private Button debugRewardsButton;
    [SerializeField] private Button debugAddBoostersButton;
    [SerializeField] private Button debugAddCoinButton;
    [SerializeField] private Button changeBackgroundColorButton;
    [SerializeField] private Button startAutoTestLevelButton;
    [SerializeField] private Button clearAddressablesCacheButton;
    [SerializeField] private Button increaseGameSpeedButton;
    [SerializeField] private Button decreaseGameSpeedButton;
    [SerializeField] private Button hideUIButton;
    [SerializeField] private TMP_Text gameSpeedText;
    [SerializeField] private TMP_Text debugAdsText;
    [SerializeField] private TMP_Text debugRewardsText;

    [SerializeField] private TMP_Text hideUIButtonText;

    [SerializeField] private UserResourcesObserver userResourcesObserver;

    public static event Action<int> toLevelEvent;
    public static event Action nextLevelEvent;
    public static event Action prevLevelEvent;
    public static event Action winLevelEvent;
    public static event Action switchLevelEvent;
    public static event Action startAutoTestLevelEvent;

    protected override void RegisterMoreEvent()
    {
        debugPlayLevelButton.onClick.AddListener(DebugPlayLevel);
        debugNextLevelButton.onClick.AddListener(NextLevel);
        debugPrevLevelButton.onClick.AddListener(PrevLevel);
        debugWinLevelButton.onClick.AddListener(DebugWinLevel);
        debugAdsButton.onClick.AddListener(DebugAdsButton);
        debugRewardsButton.onClick.AddListener(DebugRewardsButton);
        debugAddBoostersButton.onClick.AddListener(AddBooster);
        debugAddCoinButton.onClick.AddListener(AddCoin);
        increaseGameSpeedButton.onClick.AddListener(IncreaseGameSpeed);
        decreaseGameSpeedButton.onClick.AddListener(DecreaseGameSpeed);
        startAutoTestLevelButton.onClick.AddListener(StartAutoTestLevel);
        clearAddressablesCacheButton.onClick.AddListener(ClearAddressablesCache);
        changeBackgroundColorButton.onClick.AddListener(ChangeBackgroundColor);
        hideDebugButton.onClick.AddListener(HideDebug);
        hideUIButton.onClick.AddListener(HideUI);

        if (applyCampTestButton != null)
            applyCampTestButton.onClick.AddListener(ApplyTestCamp);

        gameSpeedText.text = $"Game Speed: x{(int)Time.timeScale}";
    }

    protected override void MoreActionInAwake()
    {
        if (UserData.IsRemoveAds)
        {
            debugAdsText.text = "Enable Ads";
        }
        else
        {
            debugAdsText.text = "Disable Ads";
        }
        bool isEnableRewards = GamePersistentVariable.isEnableRewardedAds;
        if (isEnableRewards)
        {
            debugRewardsText.text = "Disable Rewards";
        }
        else
        {
            debugRewardsText.text = "Enable Rewards";
        }
    }

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);

        bool isEnableRewards = GamePersistentVariable.isEnableRewardedAds;
        if (isEnableRewards)
        {
            debugRewardsText.text = "Disable Rewards";
        }
        else
        {
            debugRewardsText.text = "Enable Rewards";
        }

        if (GameController.Instance.useProfile.IsRemoveAds)
        {
            debugAdsText.text = "Enable Ads";
        }
        else
        {
            debugAdsText.text = "Disable Ads";
        }
    }

    private void DebugPlayLevel()
    {
        int level = int.Parse(levelInput.text);

        toLevelEvent?.Invoke(level);
        switchLevelEvent?.Invoke();

        Hide();
    }

    private void NextLevel()
    {
        nextLevelEvent?.Invoke();
        switchLevelEvent?.Invoke();

        Hide();
    }

    private void PrevLevel()
    {
        prevLevelEvent?.Invoke();
        switchLevelEvent?.Invoke();

        Hide();
    }

    private async void DebugWinLevel()
    {
        Hide();

        await Task.Delay(500);

        winLevelEvent?.Invoke();
    }

    private void StartAutoTestLevel()
    {
        AddCoin();

        AntsFlow.AutoplayManager.Ins.StartAutoplay();

        Hide();
    }

    private void DebugRewardsButton()
    {
        bool isEnableRewards = GamePersistentVariable.isEnableRewardedAds;
        if (isEnableRewards)
        {
            GamePersistentVariable.isEnableRewardedAds = RemoteConfigController.GetBoolConfig(FirebaseConfig.ENABLE_REWARDED_ADS, false);
            debugRewardsText.text = "Enable rewards";
        }
        else
        {
            GamePersistentVariable.isEnableRewardedAds = RemoteConfigController.GetBoolConfig(FirebaseConfig.ENABLE_REWARDED_ADS, true);

            debugRewardsText.text = "Disable Rewards";
        }
        Hide();
    }
    private void DebugAdsButton()
    {
        if (GameController.Instance.useProfile.IsRemoveAds)
        {
            UserData.IsRemoveAds = false;
            GameController.Instance.useProfile.IsRemoveAds = false;

            debugAdsText.text = "Disable Ads";
        }
        else
        {
            UserData.IsRemoveAds = true;
            GameController.Instance.useProfile.IsRemoveAds = true;

            debugAdsText.text = "Enable Ads";
        }
    }

    private void ApplyTestCamp()
    {
        if (campTestInput == null)
        {
            return;
        }

        string campId = campTestInput.text;
        if (string.IsNullOrEmpty(campId))
        {
            Debug.LogWarning("TEST_CAMP_ [DEBUG] Chưa nhập idCamp test.");
            return;
        }

        GameController.Instance.appsFlyerManager.ApplyTestCampById(campId);

        Hide();
    }

    private void HideDebug()
    {
        GameplayScreen.Ins.HideDebug();

        Hide();
    }

    private void HideUI()
    {
        GameplayScreen.Ins.HideUI();

        if (GamePersistentVariable.isHideUI)
        {
            hideUIButtonText.text = $"Show UI";
        }
        else
        {
            hideUIButtonText.text = $"Hide UI";
        }
    }

    private void AddBooster()
    {
        for (int i = 0; i < userResourcesObserver.UserResources.BoosterQuantities.Length; i++)
        {
            userResourcesObserver.UserResources.BoosterQuantities[i] += 999;
        }

        userResourcesObserver.Save();
    }

    private void AddCoin()
    {
        userResourcesObserver.UserResources.CoinQuantity += 9999;

        userResourcesObserver.Save();
    }

    private void ChangeBackgroundColor()
    {
        if (backgroundColorInput.text.Length != 6)
        {
            return;
        }

        Camera camera = Camera.main;

        camera.backgroundColor = ColorUtil.GetColorFromHex(backgroundColorInput.text);

        woodBackground.gameObject.SetActive(false);
    }

    private void ClearAddressablesCache()
    {
        Caching.ClearCache();
    }

    private void IncreaseGameSpeed()
    {
        ChangeGameSpeed(true);
    }

    private void DecreaseGameSpeed()
    {
        ChangeGameSpeed(false);
    }

    private void ChangeGameSpeed(bool isIncrease)
    {
        if (isIncrease)
        {
            if (Time.timeScale < 5)
            {
                Time.timeScale++;
            }
        }
        else
        {
            if (Time.timeScale > 1)
            {
                Time.timeScale--;
            }
        }

        gameSpeedText.text = $"Game Speed: x{(int)Time.timeScale}";
    }
}
