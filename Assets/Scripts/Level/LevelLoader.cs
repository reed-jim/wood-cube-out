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

public class LevelLoader : MonoBehaviour
{
    [Header("Clone to calculate level rotation")]
    [SerializeField] private Transform cloneToCalculateRotation;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private LevelDataContainer levelDataContainer;
    [SerializeField] private LevelBoosterObserver levelBoosterObserver;
    [SerializeField] private ObjectPartMaterialContainer objectPartMaterialContainer;

    #region PRIVATE FIELD
    private List<Tween> _tweens;
    private List<Tween> _autoCenterLevelTweens;
    private LevelDifficultyConfiguration _currentLevelDifficultyConfiguration;
    private int _maxLevel;
    private BasicObjectPart[] _pieces;
    private Transform _currentLevelContainer;
    private Transform _pivotAdjust;
    private bool _isNotAutoCenterLevel;
    private bool _isSetLevelGradientBound;
    private bool _isNewLoopLevel;
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
    #endregion

    private void Awake()
    {
        PausePopup.replayLevelEvent += Replay;
        LosePopup.replayLevelEvent += Replay;
        DebugPopup.toLevelEvent += GoLevel;
        DebugPopup.nextLevelEvent += NextLevel;
        DebugPopup.prevLevelEvent += PrevLevel;
        WinPopup.nextLevelEvent += NextLevel;
        WinPopup.goLevelEvent += GoLevel;
        LosePopup.skipLevelEvent += NextLevel;
        BasicObjectPart.pieceCompleteEvent += OnPieceCompleted;

        _tweens = new List<Tween>();
        _autoCenterLevelTweens = new List<Tween>();

        currentLevel.Load();

        _maxLevel = levelDataContainer.MaxLevel;

        GoLevel(currentLevel.Value);
    }

    void OnDestroy()
    {
        PausePopup.replayLevelEvent -= Replay;
        LosePopup.replayLevelEvent -= Replay;
        DebugPopup.toLevelEvent -= GoLevel;
        DebugPopup.nextLevelEvent -= NextLevel;
        DebugPopup.prevLevelEvent -= PrevLevel;
        WinPopup.nextLevelEvent -= NextLevel;
        WinPopup.goLevelEvent -= GoLevel;
        LosePopup.skipLevelEvent -= NextLevel;
        BasicObjectPart.pieceCompleteEvent -= OnPieceCompleted;

        CommonUtil.StopAllTweens(_tweens);
        CommonUtil.StopAllTweens(_autoCenterLevelTweens);
    }

    #region LEVEL
    private async Task LoadLevel()
    {
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
        // if (isLoopLevel)
        // {
        //     showSceneTransitionEvent?.Invoke();
        //     setIsLevelLoadedEvent?.Invoke();
        //     switchRouteEvent?.Invoke(ScreenRoute.LevelComingSoon);

        //     return;
        // }

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

        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(levelPrefab);

        handle.Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject level = Instantiate(op.Result, transform);

                _currentLevelContainer = level.transform;

                #region ASSIGN CLONE TO CALC ROTATION
                try
                {
                    level.GetComponent<RotateObject>().cloneToCalculateRotation = cloneToCalculateRotation;
                }
                catch (System.Exception e) { }
                #endregion

                // FIND DEPENDENCY
                // delay for pieces scaling before level
                List<BasicObjectPart> pieces = TransformUtil.GetComponentsFromAllChildren<BasicObjectPart>(level.transform);

                _pieces = pieces.ToArray();

                Dictionary<int, List<Transform>> obstaclesOfPieces = LevelDifficultyCalculatorUltility.FindPieceDependency(pieces.ToArray());
                //

                if (level.GetComponent<LevelTransformManualModifier>() == null)
                {
                    // level.transform.position = new Vector3(0, -0.03f * GamePersistentVariable.screenSizeWorld.y, 10);
                    level.transform.position = Vector3.zero;
                    // level.transform.rotation = Quaternion.Euler(new Vector3(-30, -45, 30));
                    level.transform.rotation = Quaternion.Euler(new Vector3(-15, -45, 15));
                }

                // // GENERATE CONVEX COLLIDERS
                // for (int i = 0; i < pieces.Count; i++)
                // {
                //     BasicObjectPart piece = pieces[i];
                //     MeshCollider pieceCollider = piece.GetComponent<MeshCollider>();
                //     MeshFilter pieceMeshFilter = piece.GetComponent<MeshFilter>();

                //     if (!pieceCollider.convex)
                //     {
                //         piece.AddComponent<CoACD>().RunACD(pieceMeshFilter.mesh);

                //         Destroy(pieceCollider);
                //     }
                // }



                // AUTO CENTER LEVEL
                _isNotAutoCenterLevel = false;

                foreach (BasicObjectPart obj in pieces)
                {
                    MeshFilter meshFilter = obj.GetComponent<MeshFilter>();

                    if (Vector3.Distance(meshFilter.mesh.bounds.center, Vector3.zero) > 0.03f)
                    {
                        _isNotAutoCenterLevel = true;

                        break;
                    }
                }

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

                    foreach (BasicObjectPart piece in pieces)
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

                    GamePersistentVariable.levelOffset = new Vector3(0, 1f, 0);

                    _isSetLevelGradientBound = false;

                    Vector3 center = GetCenterOfObjectsWithBoundsAndPivot(pieces.ToArray(), offset: GamePersistentVariable.levelOffset);

                    _pivotAdjust.localPosition = -center / _currentLevelContainer.transform.localScale.x;
                }
                // 



                _currentLevelDifficultyConfiguration = LevelDifficultyCalculatorUltility.GenerateScrewBoxDifficultyTemplate(currentLevel.Value);

                // ASSIGN SCREW FACTIONS
                if (currentLevel.Value == 1)
                {
                    AutoAssignScrewFactionLevelManualGroup(level);
                }
                else
                {
                    AutoAssignScrewFaction(level);
                }

                AutoAssignScrewFactionMultiPhaseLevel(level);

                if (isLoopLevel)
                {
                    RandomizeLevelModelColor(level);
                }
                else
                {
                    SaferioTracking.TrackLevelStart(currentLevel.Value);
                }

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
        BaseScrew[] screws = TransformUtil.GetComponentsFromAllChildren<BaseScrew>(level.transform).ToArray();

        int numScrew = screws.Length;

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
            screwFactionGroups[i] = screws[i].GetComponent<ScrewFactionGroupManual>();
        }

        // screwFactionGroups = screwFactionGroups.OrderBy(item => item.Group).ToArray();

        for (int i = 0; i < numScrew; i++)
        {
            screws[i].ScrewId = i;
            screws[i].Faction = factionPool[screwFactionGroups[i].Group];

            screws[i].ScrewServiceLocator.screwFaction.SetColorByFaction();
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
    private async void OnPieceCompleted(int pieceInstanceId)
    {
        // if (_isNotAutoCenterLevel)
        // {
        //     return;
        // }

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

            CommonUtil.StopAllTweens(_autoCenterLevelTweens);

            _autoCenterLevelTweens.Add(Tween.LocalPosition(_pivotAdjust, newCenter, duration: 0.3f));
        }
        catch (System.Exception)
        {

        }
    }

    Vector3 GetCenterOfObjectsWithBoundsAndPivot(BasicObjectPart[] objects, Vector3 offset)
    {
        if (objects.Length == 0)
        {
            return Vector3.zero;
        }

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

        foreach (BasicObjectPart obj in objects)
        {
            if (!obj.transform.IsChildOf(_pivotAdjust))
            {
                continue;
            }

            MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
            MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();

            Vector3 objectPosition = (obj.transform.position + meshRenderer.bounds.center) / 2;

            objectPosition = obj.transform.position;

            float boundXMin = obj.transform.position.x - 0.5f * obj.BoundSize.x;
            float boundXMax = obj.transform.position.x + 0.5f * obj.BoundSize.x;
            float boundYMin = obj.transform.position.y - 0.5f * obj.BoundSize.y;
            float boundYMax = obj.transform.position.y + 0.5f * obj.BoundSize.y;
            float boundZMin = obj.transform.position.z - 0.5f * obj.BoundSize.z;
            float boundZMax = obj.transform.position.z + 0.5f * obj.BoundSize.z;

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
        bool isScaleByHeight = false;

        float maxExpectedLevelSize = expectedLevelSize.x;

        if (expectedLevelSize.y > 1.05f * maxExpectedLevelSize)
        {
            maxExpectedLevelSize = expectedLevelSize.y;

            isScaleByHeight = true;
        }

        if (expectedLevelSize.z > maxExpectedLevelSize)
        {
            maxExpectedLevelSize = expectedLevelSize.z;

            isScaleByHeight = false;
        }

        Camera mainCamera = Camera.main;

        float expectedScreenWidth = 1.5f * maxExpectedLevelSize;
        float expectedScreenHeight = 3f * maxExpectedLevelSize;

        if (!isScaleByHeight)
        {
            expectedScreenHeight = expectedScreenWidth / mainCamera.aspect;
        }

        float targetOrthographicSize = expectedScreenHeight / 2f;

        // 3 dimesion equal
        if (Mathf.Abs(expectedLevelSize.x - expectedLevelSize.y) < 0.5f)
        {
            targetOrthographicSize *= 1.3f;
        }

        setTargetOrthographicSizeEvent?.Invoke(targetOrthographicSize);
    }

    private void SetLevelGradientMinMaxIn(Vector3 levelCenter, float minWorldY, float maxWorldY)
    {
        float halfDistance = (maxWorldY - minWorldY) / 2;
        float shaderMinY = levelCenter.y - halfDistance;
        float shaderMaxY = levelCenter.y + halfDistance;

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

        _tweens.Add(Tween.Delay(3).OnComplete(() =>
        {
            switchRouteEvent?.Invoke(ScreenRoute.UnlockDoubleScrewBoxesSuggestion);
        }));
    }

    private void DelayShowIAPSuggestion()
    {
        CommonUtil.StopAllTweens(_tweens);

        _tweens.Add(Tween.Delay(3).OnComplete(() =>
        {
            switchRouteEvent?.Invoke(ScreenRoute.IAPSuggestion);
        }));
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
