using BirdSort2;
using System.Collections;
using UnityEditor.Build.Pipeline;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Đặt trong Connect scene (GameObject riêng hoặc gắn vào GameManager của Connect).
/// Lắng nghe TileManager.winLevelEvent / loseLevelEvent.
/// Gọi trực tiếp Setup() từ các popup của BirdSort và dùng SetConnectCallbacks để override nút.
/// </summary>
public class ConnectPopupManager : MonoBehaviour
{
    public static ConnectPopupManager Instance { get; private set; }

    [Header("Scene Names")]
    private string connectSceneName = "Ants Flow";
     private string birdSortHomeName = "HomeScene"; // Scene BirdSort Home

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInitialize()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("ConnectPopupManager");
            Instance = go.AddComponent<ConnectPopupManager>();
            DontDestroyOnLoad(go);
        }
    }



    private void OnEnable()
    {
        DebugPopup.winLevelEvent += OnWin;
        DebugPopup.loseLevelEvent += OnLose;
        GameStateWin.winLevelEvent += OnWin;
        TileManager.winLevelEvent  += OnWin;
        TileManager.loseLevelEvent += OnLose;
        GameplayScreen.switchRouteEvent += OnSwitchRoute;
        LoseBox.replayLevelEvent += OnLoseRetry;
    }

    private void OnDisable()
    {
        DebugPopup.winLevelEvent -= OnWin;
        DebugPopup.loseLevelEvent -= OnLose;
        GameStateWin.winLevelEvent -= OnWin;
        TileManager.winLevelEvent  -= OnWin;
        TileManager.loseLevelEvent -= OnLose;
        GameplayScreen.switchRouteEvent -= OnSwitchRoute;
        LoseBox.replayLevelEvent -= OnLoseRetry;
    }

    private void OnSwitchRoute(GameEnum.ScreenRoute route)
    {
        if (route == GameEnum.ScreenRoute.Pause)
        {
            ShowPause();
        }
    }

    // ── Win ──────────────────────────────────────────────────────────
    private void OnWin()
    {
        Debug.Log("[DEBUG_WIN] ConnectPopupManager.OnWin() triggered!");
        int star = 3;
        int timePlay = 3;

        // 1. Cập nhật các Class quản lý của BirdSort
        StartChestController.CurrentStar += star;
        LevelChestController.CurrentLevelChest++;

        if (BirdSortGameController.Instance != null)
        {
            Debug.Log("[DEBUG_WIN] BirdSortGameController.Instance exists. Updating profile and conditions...");
            try
            {
                UseProfile.CurrentLevel += 1;
                if (UseProfile.CurrentLevel > Context.MAX_LEVEL)
                    UseProfile.CurrentLevel = Context.MAX_LEVEL;
                BirdSortGameController.Instance.useProfile.CurrentLevelPlay = UseProfile.CurrentLevel;
                
                if (GamePlayController_BirdSort.Instance != null)
                {
                    GamePlayController_BirdSort.Instance.challengeGameplayController.WinLevel();
                    GamePlayController_BirdSort.Instance.eventSaleController.WinLevel();
                }
                
                StartCoroutine(WinHandle());
                BirdSortGameController.Instance.AnalyticsController?.LogLevelComplet(UseProfile.CurrentLevel - 1, (int)timePlay);
                BirdSortGameController.Instance.musicManager?.PauseCoolDownSoundSource();
                
                if (UseProfile.CurrentLevel - 1 == 1)
                {
                    LogFunnelController.LogFunnel("start_game", "finish_level_1", 3, UseProfile.CurrentLevel);
                }
                else if (UseProfile.CurrentLevel - 1 > 1 && UseProfile.CurrentLevel - 1 <= 10)
                {
                    LogFunnelController.LogFunnel($"level_{UseProfile.CurrentLevel - 1}", $"finish_level_{UseProfile.CurrentLevel - 1}", LogFunnelController.GetFunnelPiority($"level_{UseProfile.CurrentLevel - 1}") + 1, UseProfile.CurrentLevel);
                }

                BirdSortGameController.Instance.skillUserController?.AddValueCondition(ConditionChangeSkill.TypeCondition.StarAfterPassLevel, star);

                
                var loseLevelWithTime = BirdSortGameController.Instance.skillUserController?.GetCondition(ConditionChangeSkill.TypeCondition.LoseLevelWithTime);
                if (loseLevelWithTime != null)
                    loseLevelWithTime.EndCondition();
                var numberLoseLevel = BirdSortGameController.Instance.skillUserController?.GetCondition(ConditionChangeSkill.TypeCondition.NumberLoseLevel);
                if (numberLoseLevel != null)
                    numberLoseLevel.EndCondition();
                var timeWaitingLevel = BirdSortGameController.Instance.skillUserController?.GetCondition(ConditionChangeSkill.TypeCondition.TimeWaitingLevel);
                if (timeWaitingLevel != null)
                    timeWaitingLevel.EndCondition();
                LoseLevelWithTimeCondition.isLosed = false;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[DEBUG_WIN] Error during BirdSort update: {ex}");
            }
        }
        else
        {
            Debug.LogWarning("[DEBUG_WIN] BirdSortGameController.Instance is NULL!");
        }

       

        AdvanceBirdSortLevel();

        Debug.Log("[DEBUG_WIN] Instantiating and Showing WinBox...");
        var winBox = WinBox.Setup();
        if (winBox == null)
        {
            Debug.LogError("[DEBUG_WIN] WinBox.Setup() returned null!");
        }
        else
        {
            SetAsOverlay(winBox);
            winBox.Show(star: star, time: timePlay);
            Debug.Log("[DEBUG_WIN] WinBox Show completed.");
        }
    }


    public virtual IEnumerator WinHandle()
    {
        yield return new WaitForSeconds(0.75f);
        //Hiện popup win
        BirdSortGameController.Instance.musicManager.PlayWinSound();
        int star = 3;
        //if (percentTimeRemming >= 0.7f)//Segment
        //{
        //    star = 3;
        //}
        //else if (percentTimeRemming < 0.7f && percentTimeRemming >= 0.3f)
        //{
        //    star = 2;
        //}
        //else
        //{
        //    star = 1;
        //}
        //starWin = star;
        StartChestController.CurrentStar += star;
        LevelChestController.CurrentLevelChest++;

        Debug.Log("win");


        //    WinBox.Setup().Show();
        // Scene scene = SceneManager.GetActiveScene();
        // SceneManager.LoadScene(scene.name);
    }

    // ── Lose ─────────────────────────────────────────────────────────
    private void OnLose()
    {
        var loseBox = LoseBox.SetupLegacy();
        SetAsOverlay(loseBox);
    
        //var reviceBox = ReviveBox.Setup();
        //SetAsOverlay(reviceBox);

        // TileManager.Instance.Lose(type: 0);
    }




    // ── Button Callbacks ──────────────────────────────────────────────
    private void OnWinNext()
    {
        ConnectBridge.IsLaunchedFromBirdSort = true;
        if (BirdSortGameController.Instance != null)
            BirdSortGameController.Instance.LoadScene(SceneName.GAME_PLAY);
        else
            SceneManager.LoadScene(SceneName.GAME_PLAY);
    }

    private void OnLoseRetry()
    {
        // Không advance level, reload lại Connect
        ConnectBridge.IsLaunchedFromBirdSort = true;
        if (BirdSortGameController.Instance != null)
            BirdSortGameController.Instance.LoadScene(connectSceneName);
        else
            SceneManager.LoadScene(connectSceneName);
    }

    private void GoHome(bool isWin)
    {
        ConnectBridge.IsLaunchedFromBirdSort = false;

        if (BirdSortGameController.Instance != null)
            BirdSortGameController.Instance.LoadScene(SceneName.GAME_PLAY);
        else
            SceneManager.LoadScene(birdSortHomeName);
    }

    // ── Helpers ───────────────────────────────────────────────────────
    private void AdvanceBirdSortLevel()
    {
        int nextLevel = ConnectBridge.BirdSortLevelOnEnterConnect + 1;
        ConnectBridge.BirdSortLevelOnEnterConnect = nextLevel;
        UseProfile.CurrentLevel = nextLevel;
        if (BirdSortGameController.Instance != null)
            BirdSortGameController.Instance.useProfile.CurrentLevelPlay = nextLevel;
    }

    // ── Public API ────────────────────────────────────────────────────
    /// <summary>Gọi từ nút Pause trong Connect scene.</summary>
    public void ShowPause()
    {
        var pauseBox = PasueBox.Setup();
        SetAsOverlay(pauseBox);
        // pauseBox.SetConnectCallbacks(
        //     onResume: null, // Đã tự đóng trong PasueBox
        //     onHome: () => GoHome(isWin: false)
        // );
    }

    public void ShowRemoveAd()
    {
        var removeAdsBox = BigRemoveAdsBox.Setup();
        SetAsOverlay(removeAdsBox);
    }

    private void SetAsOverlay(Component popupComponent)
    {
        Canvas canvas = popupComponent.GetComponent<Canvas>();
        if (canvas == null)
            canvas = popupComponent.GetComponentInChildren<Canvas>(true);

        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
        }
    }
}
