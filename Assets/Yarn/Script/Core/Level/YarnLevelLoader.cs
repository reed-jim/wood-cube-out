using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.RemoteConfig;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static GameEnum;

public class YarnLevelLoader : MonoBehaviour
{
    [Header("Clone to calculate level rotation")]
    [SerializeField] private Transform cloneToCalculateRotation;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private LevelDataContainer levelDataContainer;
    [SerializeField] private LevelBoosterObserver levelBoosterObserver;
    [SerializeField] private ObjectPartMaterialContainer objectPartMaterialContainer;

    [Header("Textured Material Level")]
    [SerializeField] private Material texturedMaterial;

    #region PRIVATE FIELD
    private Camera _mainCamera;
    private List<Tween> _tweens;
    private List<Tween> _autoCenterLevelTweens;
    private LevelDifficultyConfiguration _currentLevelDifficultyConfiguration;
    private int _maxLevel;
    private BasePiece[] _pieces;
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
        BasePiece.pieceLoosenedEvent += OnPieceCompleted;
        LoadLevelErrorScene.retryLoadLevelEvent += LoadLevel;
        ChooseDifficultyPopup.hardModeSuggestionClosedEvent += OnHardModeSuggestionClosed;

        _tweens = new List<Tween>();
        _autoCenterLevelTweens = new List<Tween>();

        currentLevel.Load();

        _maxLevel = levelDataContainer.MaxLevel;

        _mainCamera = Camera.main;

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
        BasePiece.pieceLoosenedEvent -= OnPieceCompleted;
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

        // A/B TEST LEVEL 4
        if (nextLevel == 4)
        {
            int level4Variant = RemoteConfigController.GetIntConfig(FirebaseConfig.LEVEL_4_VARIANT, 0);

            if (level4Variant == 1)
            {
                levelPrefab = $"Level {nextLevel} - Variant 1";
            }
            else if (level4Variant == 2)
            {
                levelPrefab = $"Level {nextLevel} - Variant 2";
            }
        }

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

        _loadLevelHandle = Addressables.InstantiateAsync(levelPrefab, transform);

        _loadLevelHandle.Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject level = op.Result;

                _currentLevelContainer = level.transform;

                _pieces = TransformUtil.GetComponentsArrayFromAllChildren<BasePiece>(level.transform);

                YarnLevelDifficultyManager.Instance.GenerateAddionalFaction(level.transform, _pieces);

                _pieces = TransformUtil.GetComponentsArrayFromAllChildren<BasePiece>(level.transform);

                setLevelScrewNumberEvent?.Invoke(_pieces.Length);

                // // AUTO CENTER LEVEL
                // _isNotAutoCenterLevel = false;

                // foreach (BasicObjectPart obj in pieces)
                // {
                //     MeshFilter meshFilter = obj.GetComponent<MeshFilter>();

                //     if (Vector3.Distance(meshFilter.mesh.bounds.center, Vector3.zero) > 0.03f)
                //     {
                //         _isNotAutoCenterLevel = true;

                //         break;
                //     }
                // }

                _isNotAutoCenterLevel = false;

                level.transform.position = Vector3.zero;

                if (!_isNotAutoCenterLevel)
                {
                    for (int i = 0; i < level.transform.childCount; i++)
                    {
                        Transform pivotAdjust = level.transform.GetChild(i);

                        if (pivotAdjust.name.Contains("Pivot Adjust"))
                        {
                            _pivotAdjust = pivotAdjust;

                            _pivotAdjust.transform.position = Vector3.zero;
                        }
                    }

                    foreach (BasePiece piece in _pieces)
                    {
                        if (_pivotAdjust == null)
                        {
                            _pivotAdjust = new GameObject().transform;
                            _pivotAdjust.name = "Pivot Adjust";
                            _pivotAdjust.SetParent(_currentLevelContainer);

                            _pivotAdjust.transform.localPosition = Vector3.zero;
                            _pivotAdjust.transform.localEulerAngles = Vector3.zero;
                            _pivotAdjust.transform.localScale = Vector3.one;
                        }

                        if (!piece.transform.IsChildOf(_pivotAdjust))
                        {
                            piece.transform.SetParent(_pivotAdjust);
                        }
                    }

                    GamePersistentVariable.levelOffset = new Vector3(0, 0.3f, 0);

                    _isSetLevelGradientBound = false;

                    Vector3 center = GetCenterOfObjectsWithBoundsAndPivot(_pieces, offset: GamePersistentVariable.levelOffset);

                    _pivotAdjust.localPosition = -center / _currentLevelContainer.transform.localScale.x;
                }
                // 

                // texturedMaterial.SetVector("_MainTextureOffset", new Vector2(0, 0));

                // if (isLoopLevel)
                // {
                //     RandomizeLevelModelColor(level);
                // }
                // else
                // {
                //     SaferioTracking.TrackLevelStart(currentLevel.Value);
                // }

                // _nextLevel = nextLevel;
                // _isLoopLevel = isLoopLevel;

                // _tweens.Add(Tween.Delay(3).OnComplete(() =>
                // {
                //     ShowSuggestion(nextLevel, isLoopLevel);
                // }));
                // // SuggestHardMode();

                // #region EVENT ITEMS
                // GamePersistentVariable.coinChallengeItemGainedInLevel = 0;

                // CoinChallengeGameplayUI.Instance.SetNumber();
                // #endregion

                // GamePersistentVariable.isLevelLost = false;
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

    #region ASSIGN SCREW FACTION
    private void AutoAssignScrewFactionMultiPhaseLevel(GameObject level)
    {
        MultiPhaseScrew[] screws = TransformUtil.GetComponentsFromAllChildren<MultiPhaseScrew>(level.transform).ToArray();

        Dictionary<int, List<GameFaction>> remainingFactionForScrewsByPhase = new Dictionary<int, List<GameFaction>>();

        Dictionary<int, int> currentFactionByPhase = new Dictionary<int, int>();

        for (int i = 0; i < screws.Length; i++)
        {
            int phase = screws[i].Phase;

            if (!remainingFactionForScrewsByPhase.ContainsKey(phase))
            {
                currentFactionByPhase.Add(phase, 0);

                remainingFactionForScrewsByPhase.Add(phase, new List<GameFaction>() { GameConstants.SCREW_FACTION[currentFactionByPhase[phase]] });
            }
            else
            {
                remainingFactionForScrewsByPhase[phase].Add(GameConstants.SCREW_FACTION[currentFactionByPhase[phase]]);

                if (i > 0 && (i + 1) % 3 == 0)
                {
                    currentFactionByPhase[phase]++;

                    if (currentFactionByPhase[phase] >= GameConstants.SCREW_FACTION.Length)
                    {
                        currentFactionByPhase[phase] = 0;
                    }
                }
            }
        }

        for (int i = 0; i < screws.Length; i++)
        {
            int phase = screws[i].Phase;

            int randomIndex = UnityEngine.Random.Range(0, remainingFactionForScrewsByPhase[phase].Count);

            screws[i].ScrewId = i;
            screws[i].Faction = remainingFactionForScrewsByPhase[phase][randomIndex];

            screws[i].ScrewServiceLocator.screwFaction.SetColorByFaction();

            remainingFactionForScrewsByPhase[phase].RemoveAt(randomIndex);
        }

        setMultiPhaseLevelScrewNumberEvent?.Invoke(screws.Length);
    }

    private void AutoAssignScrewFactionLevelManualGroup(GameObject level)
    {
        BasePiece[] pieces = TransformUtil.GetComponentsFromAllChildren<BasePiece>(level.transform).ToArray();

        int numScrew = pieces.Length;

        int numTripleScrews = numScrew / 3;

        List<GameFaction> factionPool = new List<GameFaction>();

        for (int i = 0; i < numTripleScrews; i++)
        {
            GameFaction randomFaction = GameConstants.SCREW_FACTION[UnityEngine.Random.Range(0, GameConstants.SCREW_FACTION.Length)];

            factionPool.Add(randomFaction);
        }

        ScrewFactionGroupManual[] screwFactionGroups = new ScrewFactionGroupManual[numScrew];

        for (int i = 0; i < numScrew; i++)
        {
            screwFactionGroups[i] = pieces[i].GetComponent<ScrewFactionGroupManual>();
        }

        // screwFactionGroups = screwFactionGroups.OrderBy(item => item.Group).ToArray();

        for (int i = 0; i < numScrew; i++)
        {
            pieces[i].PieceId = i;
            pieces[i].Faction = factionPool[screwFactionGroups[i].Group];

            pieces[i].PieceServiceLocator.pieceProperty.SetColorByFaction();
        }

        setLevelScrewNumberEvent?.Invoke(numScrew);
    }

    private void AutoAssignScrewFaction(GameObject level)
    {
        BaseScrew[] screws = TransformUtil.GetComponentsFromAllChildren<BaseScrew>(level.transform).ToArray();

        int numScrew = screws.Length;

        List<BaseScrew> noBlockedScrews = new List<BaseScrew>();
        List<BaseScrew> blockedScrews = new List<BaseScrew>();

        for (int i = 0; i < screws.Length; i++)
        {
            int numBlockedObject = screws[i].CountBlockingObjects(isIncludeHiddenScrew: true);

            if (numBlockedObject == 0)
            {
                noBlockedScrews.Add(screws[i]);
            }
            else
            {
                blockedScrews.Add(screws[i]);
            }
        }

        // SHUFFLE SCREW LISTS
        ShuffleList(noBlockedScrews);
        ShuffleList(blockedScrews);

        List<BaseScrew> shuffledScrews = new List<BaseScrew>();

        float ratioGeneratingSameFactionForTripleNoBlockedScrewStartGame = _currentLevelDifficultyConfiguration.RatioGeneratingSameFactionForTripleNoBlockedScrewStartGame;

        // float ratioGeneratingSameFactionForTripleNoBlockedScrewStartGame =
        //     levelDataContainer.LevelsData[currentLevel.Value].LevelDifficultyConfiguration.RatioGeneratingSameFactionForTripleNoBlockedScrewStartGame;

        int numNoBlockedScrewsToGetSameFaction = (int)(ratioGeneratingSameFactionForTripleNoBlockedScrewStartGame * noBlockedScrews.Count);
        int numRemainingScrews = numScrew - numNoBlockedScrewsToGetSameFaction;

        // EASY SCREWS
        for (int i = 0; i < numNoBlockedScrewsToGetSameFaction; i++)
        {
            BaseScrew nextScrew = noBlockedScrews.First();

            shuffledScrews.Add(nextScrew);

            noBlockedScrews.Remove(nextScrew);
        }

        // REMAINING SCREWS
        int currentIndex = 0;

        for (int i = 0; i < numRemainingScrews; i++)
        {
            BaseScrew nextScrew;

            if (currentIndex % (GameConstants.NUMBER_SLOT_PER_SCREW_BOX - 1) == 0)
            {
                if (noBlockedScrews.Count > 0)
                {
                    nextScrew = noBlockedScrews.First();

                    noBlockedScrews.Remove(nextScrew);
                }
                else
                {
                    nextScrew = blockedScrews.First();

                    blockedScrews.Remove(nextScrew);
                }
            }
            else
            {
                if (blockedScrews.Count > 0)
                {
                    nextScrew = blockedScrews.First();

                    blockedScrews.Remove(nextScrew);
                }
                else
                {
                    nextScrew = noBlockedScrews.First();

                    noBlockedScrews.Remove(nextScrew);
                }
            }

            shuffledScrews.Add(nextScrew);

            currentIndex++;
        }

        int currentFaction = 0;

        List<GameFaction> remainingFactionForBlockedScrews = new List<GameFaction>();

        for (int i = 0; i < shuffledScrews.Count; i++)
        {
            remainingFactionForBlockedScrews.Add(GameConstants.SCREW_FACTION[currentFaction]);

            if (i > 0 && (i + 1) % 3 == 0)
            {
                currentFaction++;

                if (currentFaction >= GameConstants.SCREW_FACTION.Length)
                {
                    currentFaction = 0;
                }
            }
        }

        for (int i = 0; i < shuffledScrews.Count; i++)
        {
            GameFaction nextFaction = remainingFactionForBlockedScrews.First();

            shuffledScrews[i].ScrewId = i;
            shuffledScrews[i].Faction = nextFaction;

            shuffledScrews[i].ScrewServiceLocator.screwFaction.SetColorByFaction();

            remainingFactionForBlockedScrews.Remove(nextFaction);
        }

        // RAMDOMIZE
        // for (int i = 0; i < shuffledScrews.Count; i++)
        // {
        //     int randomIndex = UnityEngine.Random.Range(0, remainingFactionForBlockedScrews.Count);

        //     shuffledScrews[i].ScrewId = i;
        //     shuffledScrews[i].Faction = remainingFactionForBlockedScrews[randomIndex];

        //     shuffledScrews[i].ScrewServiceLocator.screwFaction.SetColorByFaction();

        //     remainingFactionForBlockedScrews.RemoveAt(randomIndex);
        // }

        setLevelScrewNumberEvent?.Invoke(shuffledScrews.Count);
    }
    #endregion

    #region AUTO CENTER LEVEL
    private async void OnPieceCompleted()
    {
        await Task.Delay(500);

        try
        {
            Vector3 currentPosition = _pivotAdjust.transform.position;
            Vector3 currentRotation = _pivotAdjust.transform.eulerAngles;

            _pivotAdjust.transform.position = Vector3.zero;
            _pivotAdjust.transform.eulerAngles = Vector3.zero;

            Vector3 center = GetCenterOfObjectsWithBoundsAndPivot(_pieces, offset: GamePersistentVariable.levelOffset);

            _pivotAdjust.transform.position = currentPosition;
            _pivotAdjust.transform.eulerAngles = currentRotation;

            Vector3 newCenter = -center / _currentLevelContainer.transform.localScale.x;

            // await Task.Delay(500);

            CommonUtil.StopAllTweens(_autoCenterLevelTweens);

            _autoCenterLevelTweens.Add(Tween.LocalPosition(_pivotAdjust, newCenter, duration: 0.3f, ease: Ease.InOutSine));
        }
        catch (System.Exception e)
        {

        }
    }

    Vector3 GetCenterOfObjectsWithBoundsAndPivot(BasePiece[] objects, Vector3 offset)
    {
        if (objects.Length == 0)
        {
            return Vector3.zero;
        }

        bool isValid = false;

        float minX = float.MaxValue;
        float maxX = float.MinValue;

        float minY = float.MaxValue;
        float maxY = float.MinValue;

        float minZ = float.MaxValue;
        float maxZ = float.MinValue;

        // temp set pivot adjust
        Vector3 currentPosition = _pivotAdjust.transform.position;
        Vector3 currentRotation = _pivotAdjust.transform.eulerAngles;

        _pivotAdjust.transform.position = Vector3.zero;
        _pivotAdjust.transform.eulerAngles = Vector3.zero;

        transform.rotation = Quaternion.identity;

        float pieceLossyScale = objects[0].transform.lossyScale.x;

        foreach (BasePiece obj in objects)
        {
            if (!obj.transform.IsChildOf(_pivotAdjust))
            {
                continue;
            }

            if (!obj.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (!isValid)
            {
                isValid = true;
            }

            MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
            MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();

            Vector3 objectPosition = obj.transform.position + meshFilter.mesh.bounds.center * pieceLossyScale;

            float boundXMin = objectPosition.x - 0.5f * obj.PieceServiceLocator.pieceProperty.BoundSize.x;
            float boundXMax = objectPosition.x + 0.5f * obj.PieceServiceLocator.pieceProperty.BoundSize.x;
            float boundYMin = objectPosition.y - 0.5f * obj.PieceServiceLocator.pieceProperty.BoundSize.y;
            float boundYMax = objectPosition.y + 0.5f * obj.PieceServiceLocator.pieceProperty.BoundSize.y;
            float boundZMin = objectPosition.z - 0.5f * obj.PieceServiceLocator.pieceProperty.BoundSize.z;
            float boundZMax = objectPosition.z + 0.5f * obj.PieceServiceLocator.pieceProperty.BoundSize.z;

            if (boundXMin < minX)
            {
                minX = boundXMin;
            }

            if (boundXMax > maxX)
            {
                maxX = boundXMax;
            }

            if (boundYMin < minY)
            {
                minY = boundYMin;
            }

            if (boundYMax > maxY)
            {
                maxY = boundYMax;
            }

            if (boundZMin < minZ)
            {
                minZ = boundZMin;
            }

            if (boundZMax > maxZ)
            {
                maxZ = boundZMax;
            }
        }

        if (!isValid)
        {
            return Vector3.zero;
        }

        _pivotAdjust.transform.position = currentPosition;
        _pivotAdjust.transform.eulerAngles = currentRotation;

        // Set Shader World Bound
        Vector3 currentCenter = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2) + offset;
        Vector3 newCenter = -currentCenter / _currentLevelContainer.transform.localScale.x;

        if (!_isSetLevelGradientBound)
        {
            SetLevelGradientMinMaxIn(newCenter, minY, maxY);

            SetFitCameraOrthographicSize(new Vector3(maxX - minX, maxY - minY, maxZ - minZ));
        }

        // float halfDistance = (maxY - minY) / 2;
        // float shaderMinY = newCenter.y - halfDistance;
        // float shaderMaxY = newCenter.y + halfDistance;

        // if (!_isSetLevelGradientBound)
        // {
        //     if (_pieces != null)
        //     {
        //         for (int i = 0; i < _pieces.Length; i++)
        //         {
        //             if (_pieces[i] != null)
        //             {
        //                 _pieces[i].PieceMaterialPropertyBlock.SetWorldBound(shaderMinY, shaderMaxY);
        //             }
        //         }

        //         _isSetLevelGradientBound = true;
        //     }
        // }

        return new Vector3((minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2) + offset;
    }
    #endregion

    #region LEVEL SETUP
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

        targetOrthographicSize = 1.35f * expectedLevelSize.y;

        // if (maxExpectedLevelSize > expectedLevelSize.y)
        // {
        //     float ratio = (maxExpectedLevelSize / expectedLevelSize.y);

        //     targetOrthographicSize *= (1 + 0.25f * ratio * ratio);
        // }

        if (targetOrthographicSize > _mainCamera.orthographicSize || targetOrthographicSize < 2)
        {
            return;
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
        // float halfDistance = (maxWorldY - minWorldY) / 2;
        // float shaderMinY = levelCenter.y - halfDistance;
        // float shaderMaxY = levelCenter.y + halfDistance;

        // shaderMinY = Mathf.Min(shaderMinY, 0);
        // shaderMaxY = Mathf.Max(shaderMaxY, 1);

        // if (!_isSetLevelGradientBound)
        // {
        //     if (_pieces != null)
        //     {
        //         for (int i = 0; i < _pieces.Length; i++)
        //         {
        //             if (_pieces[i] != null)
        //             {
        //                 _pieces[i].PieceMaterialPropertyBlock.SetWorldBound(shaderMinY, shaderMaxY);
        //             }
        //         }

        //         _isSetLevelGradientBound = true;
        //     }
        // }
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

    private void RandomizeLevelModelColor(GameObject level)
    {
        BasicObjectPart[] objectParts = TransformUtil.GetComponentsFromAllChildren<BasicObjectPart>(level.transform).ToArray();
        Renderer[] objectPartRenderers = new Renderer[objectParts.Length];

        // MATERIALS
        List<Material> materialPool = objectPartMaterialContainer.Materials.ToList();

        List<Material> piecesMaterials = new List<Material>();

        for (int i = 0; i < objectParts.Length; i++)
        {
            objectPartRenderers[i] = objectParts[i].GetComponent<MeshRenderer>();
        }

        Material material = null;

        float randomTilingX = UnityEngine.Random.Range(0, 129) / 129f;
        float randomTilingY = UnityEngine.Random.Range(0, 129) / 129f;

        bool isUsingTexturedMaterial = false;

        if (objectPartRenderers[0].sharedMaterial.name == "Wood Texture Master")
        {
            isUsingTexturedMaterial = true;
        }

        if (isUsingTexturedMaterial)
        {
            for (int i = 0; i < objectParts.Length; i++)
            {
                objectPartRenderers[i].sharedMaterial.SetVector("_MainTextureOffset", new Vector2(randomTilingX, randomTilingY));
            }

            return;
        }

        // Use gradient materials
        for (int i = 0; i < objectParts.Length; i++)
        {
            if (!materialPool.Contains(objectPartRenderers[i].sharedMaterial))
            {
                return;
            }

            if (!piecesMaterials.Contains(objectPartRenderers[i].sharedMaterial))
            {
                piecesMaterials.Add(objectPartRenderers[i].sharedMaterial);
            }
        }

        Dictionary<Material, int> newMaterialIndexes = new Dictionary<Material, int>();

        const int NUM_GRADIENT_MATERIAL = 9;

        // Random new material for each material index
        foreach (var pieceMaterial in piecesMaterials)
        {
            newMaterialIndexes.Add(
                pieceMaterial, UnityEngine.Random.Range(0, materialPool.Count - NUM_GRADIENT_MATERIAL));
        }

        for (int i = 0; i < objectParts.Length; i++)
        {
            int newMaterialIndex = newMaterialIndexes[objectPartRenderers[i].sharedMaterial];

            objectPartRenderers[i].sharedMaterial = materialPool[newMaterialIndex];
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
