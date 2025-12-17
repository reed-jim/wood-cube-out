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

    [SerializeField] private Button debugPlayLevelButton;
    [SerializeField] private Button debugNextLevelButton;
    [SerializeField] private Button debugPrevLevelButton;
    [SerializeField] private Button debugWinLevelButton;
    [SerializeField] private Button debugAdsButton;
    [SerializeField] private Button debugAddBoostersButton;
    [SerializeField] private Button debugAddCoinButton;
    [SerializeField] private Button changeBackgroundColorButton;
    [SerializeField] private Button startAutoTestLevelButton;
    [SerializeField] private Button clearAddressablesCacheButton;
    [SerializeField] private TMP_Text debugAdsText;

    [SerializeField] private Button easyButton;
    [SerializeField] private Button normalButton;
    [SerializeField] private Button hardButton;
    
    [Header("Gallery")]
    [SerializeField] private Button showPictureNewButton;

    [SerializeField] private UserResourcesObserver userResourcesObserver;

    #region Events

    public static event Action<int> toLevelEvent;
    public static event Action nextLevelEvent;
    public static event Action prevLevelEvent;
    public static event Action winLevelEvent;
    public static event Action startAutoTestLevelEvent;
    public static event Action<float> updateCoinTextEvent;
    public static event Action<float> changeDifficultyEvent;

    #endregion

    protected override void RegisterMoreEvent()
    {
        debugPlayLevelButton.onClick.AddListener(DebugPlayLevel);
        debugNextLevelButton.onClick.AddListener(NextLevel);
        debugPrevLevelButton.onClick.AddListener(PrevLevel);
        debugWinLevelButton.onClick.AddListener(DebugWinLevel);
        debugAdsButton.onClick.AddListener(DebugAdsButton);
        debugAddBoostersButton.onClick.AddListener(AddBooster);
        debugAddCoinButton.onClick.AddListener(AddCoin);
        startAutoTestLevelButton.onClick.AddListener(StartAutoTestLevel);
        clearAddressablesCacheButton.onClick.AddListener(ClearAddressablesCache);

        easyButton.onClick.AddListener(() => ChangeDifficulty(0));
        normalButton.onClick.AddListener(() => ChangeDifficulty(0.5f));
        hardButton.onClick.AddListener(() => ChangeDifficulty(0.7f));

        changeBackgroundColorButton.onClick.AddListener(ChangeBackgroundColor);
        
        showPictureNewButton.onClick.AddListener(OnClickShowPictureNew);
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
    }

    #region Functions

    private void DebugPlayLevel()
    {
        int level = int.Parse(levelInput.text);

        toLevelEvent?.Invoke(level);

        Hide();
    }

    private void NextLevel()
    {
        nextLevelEvent?.Invoke();

        Hide();
    }

    private void PrevLevel()
    {
        prevLevelEvent?.Invoke();

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

        startAutoTestLevelEvent?.Invoke();

        Hide();
    }

    private async void DebugAdsButton()
    {
        if (UserData.IsRemoveAds)
        {
            UserData.IsRemoveAds = false;

            debugAdsText.text = "Disable Ads";
        }
        else
        {
            UserData.IsRemoveAds = true;

            debugAdsText.text = "Enable Ads";
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
        updateCoinTextEvent?.Invoke(userResourcesObserver.UserResources.CoinQuantity);
    }

    #endregion

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

    private void ChangeDifficulty(float difficulty)
    {
        changeDifficultyEvent?.Invoke(difficulty);
    }

    #region GALLERY

    private void OnClickShowPictureNew()
    {
        GamePersistentVariable.currentPictureId_PictureNew = 1;
        SwitchRoute(GameEnum.ScreenRoute.PictureNew, new object[] {1});
    }

    #endregion

    
}
