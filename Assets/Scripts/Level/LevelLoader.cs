using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static GameEnum;

public class LevelLoader : MonoBehaviour
{
    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private LevelDataContainer levelDataContainer;
    [SerializeField] private LevelBoosterObserver levelBoosterObserver;

    #region PRIVATE FIELD
    private List<Tween> _tweens;
    private List<Tween> _autoCenterLevelTweens;
    private Camera _mainCamera;
    private int _maxLevel;
    private BasicObjectPart[] _pieces;
    private Transform _currentLevelContainer;
    private Transform _pivotAdjust;
    private bool _isNotAutoCenterLevel;
    private bool _isSetLevelGradientBound;
    private bool _isNewLoopLevel;
    private int _nextLevel;
    private bool _isLoopLevel;
    #endregion

    #region Memory Management
    AsyncOperationHandle<GameObject> _loadLevelHandle;
    #endregion

    #region EVENT
    public static event Action startLevelEvent;
    public static event Action setLevelNumberEvent;
    public static event Action<int> setLevelScrewNumberEvent;
    public static event Action<int> setMultiPhaseLevelScrewNumberEvent;
    public static event Action showSceneTransitionEvent;
    public static event Action<ScreenRoute> switchRouteEvent;
    public static event Action setIsLevelLoadedEvent;
    public static event Action<bool> enableSwipingEvent;
    public static event Action<float> setTargetOrthographicSizeEvent;
    public static event Action<bool> showLoadLevelErrorEvent;
    #endregion

    private void Awake()
    {
        PausePopup.replayLevelEvent += Replay;
        LosePopup.replayLevelEvent += Replay;
        ReplayPopup.replayLevelEvent += Replay;
        DebugPopup.toLevelEvent += GoLevel;
        DebugPopup.nextLevelEvent += NextLevel;
        DebugPopup.prevLevelEvent += PrevLevel;
        WinPopup.nextLevelEvent += NextLevel;
        WinPopup.goLevelEvent += GoLevel;
        LosePopup.skipLevelEvent += NextLevel;
        LoadLevelErrorScene.retryLoadLevelEvent += LoadLevel;
        ChooseDifficultyPopup.hardModeSuggestionClosedEvent += OnHardModeSuggestionClosed;

        _tweens = new List<Tween>();
        _autoCenterLevelTweens = new List<Tween>();

        _mainCamera = Camera.main;

        currentLevel.Load();

        _maxLevel = levelDataContainer.MaxLevel;

        GoLevel(currentLevel.Value);

        GamePersistentVariable.hasPlayerReachedGameplay = true;
    }

    void OnDestroy()
    {
        PausePopup.replayLevelEvent -= Replay;
        LosePopup.replayLevelEvent -= Replay;
        ReplayPopup.replayLevelEvent -= Replay;
        DebugPopup.toLevelEvent -= GoLevel;
        DebugPopup.nextLevelEvent -= NextLevel;
        DebugPopup.prevLevelEvent -= PrevLevel;
        WinPopup.nextLevelEvent -= NextLevel;
        WinPopup.goLevelEvent -= GoLevel;
        LosePopup.skipLevelEvent -= NextLevel;
        LoadLevelErrorScene.retryLoadLevelEvent -= LoadLevel;
        ChooseDifficultyPopup.hardModeSuggestionClosedEvent -= OnHardModeSuggestionClosed;

        CommonUtil.StopAllTweens(_tweens);
        CommonUtil.StopAllTweens(_autoCenterLevelTweens);
    }

    #region LEVEL
    private async void LoadLevel()
    {
        // // NO INTERNET
        // if (Application.internetReachability == NetworkReachability.NotReachable)
        // {
        //     showLoadLevelErrorEvent?.Invoke(true);

        //     return;
        // }
        // else
        // {
        //     showLoadLevelErrorEvent?.Invoke(false);
        // }
        // //

        int nextLevel = 1;

        bool isLoopLevel = false;

        if (!UserData.IsLoopLevel)
        {
            if (currentLevel.Value <= _maxLevel)
            {
                nextLevel = currentLevel.Value;
            }
            else
            {
                nextLevel = GetLoopLevel();

                await UserData.SetIsLoopLevel(true);
                await UserData.SetIsLastLevelBeforeLoop(_maxLevel);

                isLoopLevel = true;
            }
        }
        else
        {
            if (currentLevel.Value - UserData.NumLevelLoop >= _maxLevel)
            {
                nextLevel = GetLoopLevel();
            }
            else
            {
                nextLevel = currentLevel.Value - UserData.NumLevelLoop;
            }

            isLoopLevel = true;
        }

        // // NOT LOOP
        // if (currentLevel.Value > _maxLevel)
        // {
        //     isLoopLevel = true;
        // }

        // if (isLoopLevel)
        // {
        //     showSceneTransitionEvent?.Invoke();
        //     setIsLevelLoadedEvent?.Invoke();
        //     switchRouteEvent?.Invoke(ScreenRoute.LevelComingSoon);

        //     return;
        // }
        // // NOT LOOP

        nextLevel = Mathf.Clamp(nextLevel, 1, _maxLevel);

        GamePersistentVariable.levelPrefab = nextLevel;

        setLevelNumberEvent?.Invoke();

        nextLevel = RemoteConfigLevelManager.GetReplacedLevelIndex(nextLevel);

        string levelPrefab = $"Level {nextLevel}";

        // // REMOTE LEVELS
        // if (nextLevel > 10)
        // {
        //     if (!GamePersistentVariable.isRemoteLevelsDownloaded)
        //     {
        //         showLoadLevelErrorEvent?.Invoke(true);

        //         return;
        //     }
        //     else
        //     {
        //         showLoadLevelErrorEvent?.Invoke(false);
        //     }
        // }

        // Memory Management
        Resources.UnloadUnusedAssets();

        if (_loadLevelHandle.IsValid())
        {
            Addressables.Release(_loadLevelHandle);
        }
        //

        _loadLevelHandle = Addressables.LoadAssetAsync<GameObject>(levelPrefab);

        _loadLevelHandle.Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject level = Instantiate(op.Result, transform);

                _currentLevelContainer = level.transform;

                FitCameraForLevel(_currentLevelContainer);

                BaseItem[] items = TransformUtil.GetComponentsFromAllChildren<BaseItem>(_currentLevelContainer).ToArray();

                ItemManager.Instance.AssignItems(items);
                GameTimeManager.Instance.StartCountingTime();

                if (isLoopLevel)
                {
                    // RandomizeLevelModelColor(level);
                }
                else
                {
                    SaferioTracking.TrackLevelStart(currentLevel.Value);
                }

                _nextLevel = nextLevel;
                _isLoopLevel = isLoopLevel;

                // _tweens.Add(Tween.Delay(3).OnComplete(() =>
                // {
                //     ShowSuggestion(nextLevel, isLoopLevel);
                // }));
                // SuggestHardMode();

                // #region EVENT ITEMS
                // GamePersistentVariable.coinChallengeItemGainedInLevel = 0;

                // CoinChallengeGameplayUI.Instance.SetNumber();
                // #endregion

                GamePersistentVariable.isLevelLost = false;
            }
        };

        showSceneTransitionEvent?.Invoke();
    }

    private async void GoLevel(int level)
    {
        await Task.Delay(500);

        if (gameObject.transform.childCount > 0)
        {
            Destroy(gameObject.transform.GetChild(0).gameObject);
        }

        currentLevel.Value = level;

        currentLevel.Save();

        levelBoosterObserver.Reset();

        startLevelEvent?.Invoke();

        LoadLevel();

        GamePersistentVariable.isLevelDirty = false;
    }

    private void NextLevel()
    {
        GoLevel(currentLevel.Value + 1);
    }

    private void PrevLevel()
    {
        GoLevel(currentLevel.Value - 1);
    }

    private void Replay()
    {
        GamePersistentVariable.isPreventLoadLevelData = true;

        GoLevel(currentLevel.Value);
    }
    #endregion

    #region LEVEL SETUP
    private void FitCameraForLevel(Transform level)
    {
        BaseShelf[] grills = TransformUtil.GetComponentsFromAllChildren<BaseShelf>(level).ToArray();

        Vector2 grillSize = grills[0].Size;

        Vector2 levelSize;

        float minX = grills.Min(item => item.transform.position.x - 0.5f * grillSize.x);
        float maxX = grills.Max(item => item.transform.position.x + 0.5f * grillSize.x);
        float minY = grills.Min(item => item.transform.position.y - 0.5f * grillSize.y);
        float maxY = grills.Max(item => item.transform.position.y + 0.5f * grillSize.y);

        levelSize.x = maxX - minX;
        levelSize.y = maxY - minY;

        float expectedWorldWidth = levelSize.x / 0.95f;
        float expectedWorldHeight = levelSize.y / 0.7f;

        float orthographicSizeBasedOnWidth = expectedWorldWidth / (2f * Camera.main.aspect);
        float orthographicSizeBasedOnHeight = expectedWorldHeight / 2f;

        float finalOrthographicSize;

        if (orthographicSizeBasedOnWidth > orthographicSizeBasedOnHeight)
        {
            finalOrthographicSize = orthographicSizeBasedOnWidth;
        }
        else
        {
            finalOrthographicSize = orthographicSizeBasedOnHeight;
        }

        _mainCamera.orthographicSize = finalOrthographicSize;

        MoveLevelPivot(level, minY, maxY);
    }

    private void MoveLevelPivot(Transform level, float minY, float maxY)
    {
        Vector2 center = new Vector2();

        center.y = (0.4f * maxY + minY) / 2;

        level.transform.position = level.transform.position.ChangeY(-center.y);
    }

    private void SetFitCameraOrthographicSize(Vector3 expectedLevelSize)
    {
        float maxExpectedLevelSize = expectedLevelSize.x;

        if (expectedLevelSize.y > maxExpectedLevelSize)
        {
            maxExpectedLevelSize = expectedLevelSize.y;
        }

        if (expectedLevelSize.z > maxExpectedLevelSize)
        {
            maxExpectedLevelSize = 1f * expectedLevelSize.z;
        }

        // the higher targetOrthographicSize, the smaller model
        float targetOrthographicSize;

        targetOrthographicSize = 1.5f * expectedLevelSize.y;

        if (maxExpectedLevelSize > expectedLevelSize.y)
        {
            float ratio = (maxExpectedLevelSize / expectedLevelSize.y);

            targetOrthographicSize *= (1 + 0.25f * ratio * ratio);
        }

        setTargetOrthographicSizeEvent?.Invoke(targetOrthographicSize);

        GamePersistentVariable.currentTargetOrthographicSize = targetOrthographicSize;
    }

    private bool AreRoughlySimilar(float a, float b)
    {
        if (a < b)
        {
            if (a > 0.75f * b)
            {
                return true;
            }
        }
        else
        {
            if (b > 0.75f * a)
            {
                return true;
            }
        }

        return false;
    }

    private void SetLevelGradientMinMaxIn(Vector3 levelCenter, float minWorldY, float maxWorldY)
    {
        float halfDistance = (maxWorldY - minWorldY) / 2;
        float shaderMinY = levelCenter.y - halfDistance;
        float shaderMaxY = levelCenter.y + halfDistance;

        shaderMinY = Mathf.Min(shaderMinY, 0);
        shaderMaxY = Mathf.Max(shaderMaxY, 1);

        if (!_isSetLevelGradientBound)
        {
            if (_pieces != null)
            {
                for (int i = 0; i < _pieces.Length; i++)
                {
                    if (_pieces[i] != null)
                    {
                        _pieces[i].PieceMaterialPropertyBlock.SetWorldBound(shaderMinY, shaderMaxY);
                    }
                }

                _isSetLevelGradientBound = true;
            }
        }
    }
    #endregion

    #region BOOSTER SUGGESTION
    private void ShowSuggestion(int nextLevel, bool isLoopLevel)
    {
        int randomSuggestionIndex = UnityEngine.Random.Range(0, 2);

        if (randomSuggestionIndex == 0)
        {
            if (IsShowDoubleScrewBoxesSuggestion(nextLevel))
            {
                DelayShowDoubleScrewBoxesSuggestion();
            }
            else
            {
                if (IsShowIAPSuggestion(nextLevel, isLoopLevel))
                {
                    DelayShowIAPSuggestion();
                }
            }
        }
        else
        {
            if (IsShowIAPSuggestion(nextLevel, isLoopLevel))
            {
                DelayShowIAPSuggestion();
            }
            else
            {
                if (IsShowDoubleScrewBoxesSuggestion(nextLevel))
                {
                    DelayShowDoubleScrewBoxesSuggestion();
                }
            }
        }

        // if (IsShowBoosterSuggestion(nextLevel, isLoopLevel))
        // {
        //     DelayShowBoosterSuggest();
        // }
    }

    private bool IsShowBoosterSuggestion(int level, bool isLoopLevel)
    {
        // if (isLoopLevel)
        // {
        //     return false;
        // }

        bool remoteConfigIsShowBoosterSuggestion = RemoteConfigController.GetBoolConfig(FirebaseConfig.IS_SHOW_BOOSTER_SUGGESTION, false);
        int numLevelShowBoosterSuggestion = RemoteConfigController.GetIntConfig(FirebaseConfig.LEVEL_INTERVAL_SHOW_BOOSTER_SUGGESTION, 2);

        if (!remoteConfigIsShowBoosterSuggestion)
        {
            return false;
        }

        if (level <= 3 || level % numLevelShowBoosterSuggestion != 0)
        {
            return false;
        }

        return true;
    }

    private bool IsShowDoubleScrewBoxesSuggestion(int level)
    {
        bool remoteConfigIsShowBoosterSuggestion = RemoteConfigController.GetBoolConfig(FirebaseConfig.IS_SHOW_DOUBLE_SCREW_BOXES_SUGGESTION, false);
        int numLevelShowBoosterSuggestion = RemoteConfigController.GetIntConfig(FirebaseConfig.LEVEL_INTERVAL_SHOW_BOOSTER_SUGGESTION, 1);

        if (!remoteConfigIsShowBoosterSuggestion)
        {
            return false;
        }

        if (DataUtility.Load(GameConstants.IS_UNLOCKED_ALL_SCREW_BOXES, false))
        {
            return false;
        }

        if (level <= 3 || level % numLevelShowBoosterSuggestion != 0)
        {
            return false;
        }

        if (!GamePersistentVariable.isEnableRewardedAds)
        {
            return false;
        }

        return true;
    }

    private bool IsShowIAPSuggestion(int level, bool isLoopLevel)
    {
        bool isBoughtHotDeal = DataUtility.Load(GameConstants.IS_BOUGHT_HOT_DEAL, false);

        if (isBoughtHotDeal)
        {
            return false;
        }

        bool remoteConfigIsShowIAPSuggestion = RemoteConfigController.GetBoolConfig(FirebaseConfig.IS_SHOW_IAP_SUGGESTION, true);
        int minLevelShow = RemoteConfigController.GetIntConfig(FirebaseConfig.MIN_LEVEL_SHOW_IAP_SUGGESTION, 10);
        int numLevelShowBoosterSuggestion = RemoteConfigController.GetIntConfig(FirebaseConfig.LEVEL_INTERVAL_SHOW_BOOSTER_SUGGESTION, 2);

        if (!remoteConfigIsShowIAPSuggestion)
        {
            return false;
        }

        if (level < minLevelShow || level % numLevelShowBoosterSuggestion != 0)
        {
            return false;
        }

        return true;
    }

    private void DelayShowBoosterSuggest()
    {
        CommonUtil.StopAllTweens(_tweens);

        _tweens.Add(Tween.Delay(3).OnComplete(() =>
        {
            switchRouteEvent?.Invoke(ScreenRoute.BoosterSuggest);
        }));
    }

    private void DelayShowDoubleScrewBoxesSuggestion()
    {
        CommonUtil.StopAllTweens(_tweens);

        switchRouteEvent?.Invoke(ScreenRoute.UnlockDoubleScrewBoxesSuggestion);

        _tweens.Add(Tween.Delay(1).OnComplete(() =>
        {
            switchRouteEvent?.Invoke(ScreenRoute.UnlockDoubleScrewBoxesSuggestion);
        }));
    }

    private void DelayShowIAPSuggestion()
    {
        CommonUtil.StopAllTweens(_tweens);

        switchRouteEvent?.Invoke(ScreenRoute.IAPSuggestion);

        _tweens.Add(Tween.Delay(1).OnComplete(() =>
        {
            switchRouteEvent?.Invoke(ScreenRoute.IAPSuggestion);
        }));
    }
    #endregion

    #region HARD MODE
    private void SuggestHardMode()
    {
        _tweens.Add(Tween.Delay(3).OnComplete(() =>
        {
            switchRouteEvent?.Invoke(ScreenRoute.ChooseDifficulty);
        }));
    }

    private void OnHardModeSuggestionClosed()
    {
        ShowSuggestion(_nextLevel, _isLoopLevel);
    }
    #endregion

    #region UTIL
    private int GetLoopLevel()
    {
        int currentLoopLevel = DataUtility.Load(GameConstants.CURRENT_LEVEL_LOOP, -1);

        _isNewLoopLevel = currentLoopLevel == -1;

        if (currentLoopLevel == -1)
        {
            int randomLoopLevel = UnityEngine.Random.Range(6, _maxLevel);

            DataUtility.Save(GameConstants.CURRENT_LEVEL_LOOP, randomLoopLevel);

            return randomLoopLevel;
        }
        else
        {
            return currentLoopLevel;
        }
    }

    void ShuffleList<T>(List<T> list)
    {
        System.Random rand = new System.Random();
        int n = list.Count;

        while (n > 1)
        {
            n--;
            int k = rand.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    #endregion
}
