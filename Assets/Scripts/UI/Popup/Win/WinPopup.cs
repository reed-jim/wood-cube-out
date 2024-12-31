using System;
using System.Threading.Tasks;
using Lean.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class WinPopup : BasePopup
{
    [SerializeField] private Button continueButton;
    [SerializeField] private Button returnHomeButton;
    [SerializeField] private RectTransform coinContainer;
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private LeanLocalizedTextMeshProUGUI localizedLevelCompleted;

    [SerializeField] private IntVariable currentLevel;

    public static event Action<int> goLevelEvent;
    public static event Action nextLevelEvent;
    public static event Action<Vector3, int> collectCoinEvent;

    protected override void RegisterMoreEvent()
    {
        GameStateWin.winLevelEvent += OnLevelWinAsync;
        DebugPopup.winLevelEvent += OnLevelWinAsync;

        continueButton.onClick.AddListener(Continue);
        returnHomeButton.onClick.AddListener(ReturnHome);
    }

    protected override void UnregisterMoreEvent()
    {
        GameStateWin.winLevelEvent -= OnLevelWinAsync;
        DebugPopup.winLevelEvent -= OnLevelWinAsync;
    }

    private async void OnLevelWinAsync()
    {
        currentLevel.Value++;

        Show();

        await Task.Delay(500);

        SetLevelCompleted();

        AudioSource winSound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.WIN_SOUND);

        winSound.Play();

        CollectCoin(500);
    }

    // private async void OnEnable()
    // {
    //     await Task.Delay(500);

    //     SetLevelCompleted();

    //     AudioSource winSound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.WIN_SOUND);

    //     winSound.Play();
    // }

    public void SetLevelCompleted()
    {
        // localizedLevelCompleted.TranslationName = GameConstants.LEVEL_COMPLETED;

        localizedLevelCompleted.UpdateTranslationWithParameter(GameConstants.LEVEL_PARAMETER, $"{currentLevel.Value}");
    }

    private async void CollectCoin(int numCoin)
    {
        int _currentCoin = 0;

        while (_currentCoin < numCoin)
        {
            _currentCoin += 100;

            coinText.text = $"{_currentCoin}";

            await Task.Delay(100);
        }
    }

    private async void Continue()
    {
        collectCoinEvent?.Invoke(coinContainer.localPosition - new Vector3(0, coinContainer.sizeDelta.y), 500);

        await Task.Delay(3000);

        Hide();

        goLevelEvent?.Invoke(currentLevel.Value);
    }

    private void ReturnHome()
    {
        Addressables.LoadSceneAsync(GameConstants.MENU_SCENE);
    }
}
