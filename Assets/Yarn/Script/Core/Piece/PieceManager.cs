using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NamPhuThuy.Common;
using PrimeTween;
using UnityEngine;
using static GameEnum;
using DebugLogger = NamPhuThuy.Common.DebugLogger;

public class PieceManager : MonoBehaviour
{
    [SerializeField] private SpoolManager pieceBoxManager;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private LevelDataContainer levelDataContainer;
    [SerializeField] private LevelObserver levelObserver;

    [Header("CUSTOMIZE")]
    [SerializeField] private float delayShowPopupWin;

    #region PRIVATE FIELD
    private List<Tween> _tweens;
    private List<BasePiece> _pieces;
    private int _totalPieceObserved;
    private int _totalPiece;
    private Coroutine _handlingOutsidePieceCoroutine;
    private Coroutine _autoTestingCoroutine;
    [SerializeField] private LevelDifficultyConfiguration _currentLevelDifficultyConfiguration;
    #endregion

    #region QUEUE SPAWN SCREW BOXES
    private int _remainingSpawnSpoolBoxCommands;
    private bool _isHandlingSpawningSpoolBox;
    #endregion

    #region QUEUE OUTSIDE SCREWS
    private int _totalSpoolBoxes;
    private int _totalSpoolBoxesSpawned;
    private int _remainingHandleOutsidePieceCommands;
    private bool _isHandlingOutsidePieces;
    #endregion

    #region REMOTE CONFIG
    private bool _isReducedDifficultyAfterRewardedAds;
    #endregion

    #region EVENT
    public static event Action<GameFaction> spawnSpoolBoxEvent;
    public static event Action spawnAdsSpoolBoxesEvent;
    public static event Action winLevelEvent;
    public static event Action<BasePiece[]> setPiecesEvent;
    public static event Action<int, int> updateRemainingPieceEvent;
    public static event Action<bool> enableBoosterEvent;
    public static event Action levelSetupCompletedEvent;
    public static event Action updateOutsidePieceNumberEvent;
    public static event Action<int, GameFaction> magicPickPieceEvent;
    #endregion

    #region LIFE CYCLE

    #region Very important - PIECE DEPENDENCY DIAGRAM
    Dictionary<int, List<Transform>> obstaclesOfPieces = new Dictionary<int, List<Transform>>();
    #endregion

    void Awake()
    {
        YarnLevelLoader.startLevelEvent += OnLevelStart;
        YarnLevelLoader.setLevelScrewNumberEvent += SetLevelPieceNumber;
        BasePiece.addPieceToListEvent += AddPiece;
        SpoolBox.spawnNewSpoolBoxEvent += SpawnNewSpoolBox;
        SpoolBox.setFactionForSpoolBoxEvent += AssignFactionForNewAdsSpoolBox;
        SpoolBox.spoolBoxUnlockedEvent += OnSpoolBoxUnlocked;
        BasePiece.pieceLoosenedEvent += OnPieceLoosed;
        ScrewsDataManager.spawnFreshLevelScrewBoxesEvent += SpawnFirstSpoolBoxes;
        MultiPhaseLevelManager.switchPhaseEvent += SpawnFirstSpoolBoxesNewPhase;
        SpoolManager.spoolBoxSpawnedEvent += OnSpoolBoxSpawned;
        BasicObjectPart.pieceCompleteEvent += OnPieceCompleted;
        DebugPopup.startAutoTestLevelEvent += StartAutoTesting;
        BoosterUI.magicPickPieceEvent += MagicPick;
        // DebugPopup.changeDifficultyEvent += ChangeDifficulty;

        _tweens = new List<Tween>();

        // StartCoroutine(QueueSpawningSpoolBoxes());
        // StartCoroutine(HandlingOutsidePieces());

        _isReducedDifficultyAfterRewardedAds = RemoteConfigController.GetBoolConfig(FirebaseConfig.IS_REDUCE_DIFFICULTY_AFTER_REWARDED_AD, true);

        _totalPiece = 51;
    }

    void OnDestroy()
    {
        YarnLevelLoader.startLevelEvent -= OnLevelStart;
        YarnLevelLoader.setLevelScrewNumberEvent -= SetLevelPieceNumber;
        BasePiece.addPieceToListEvent -= AddPiece;
        SpoolBox.spawnNewSpoolBoxEvent -= SpawnNewSpoolBox;
        SpoolBox.setFactionForSpoolBoxEvent -= AssignFactionForNewAdsSpoolBox;
        SpoolBox.spoolBoxUnlockedEvent -= OnSpoolBoxUnlocked;
        BasePiece.pieceLoosenedEvent -= OnPieceLoosed;
        ScrewsDataManager.spawnFreshLevelScrewBoxesEvent -= SpawnFirstSpoolBoxes;
        MultiPhaseLevelManager.switchPhaseEvent -= SpawnFirstSpoolBoxesNewPhase;
        SpoolManager.spoolBoxSpawnedEvent -= OnSpoolBoxSpawned;
        BasicObjectPart.pieceCompleteEvent -= OnPieceCompleted;
        DebugPopup.startAutoTestLevelEvent -= StartAutoTesting;
        BoosterUI.magicPickPieceEvent -= MagicPick;
        // DebugPopup.changeDifficultyEvent -= ChangeDifficulty;

        CommonUtil.StopAllTweens(_tweens);
    }
    #endregion

    private void OnLevelStart()
    {
        _pieces = new List<BasePiece>();

        _totalPieceObserved = 0;

        _currentLevelDifficultyConfiguration = LevelDifficultyCalculatorUltility.GenerateScrewBoxDifficultyTemplate(currentLevel.Value);
        _currentLevelDifficultyConfiguration.ScrewPortFactionDifficultyModifier =
            RemoteConfigController.GetIntConfig(FirebaseConfig.SCREW_PORT_FACTION_DIFFICULTY_MODIFIER, 0);

        _totalSpoolBoxesSpawned = 0;
        _remainingSpawnSpoolBoxCommands = 0;
        _isHandlingSpawningSpoolBox = false;
        _remainingHandleOutsidePieceCommands = 0;
        _isHandlingOutsidePieces = false;

        GamePersistentVariable.numSpoolBoxUnlocked = 0;
        GamePersistentVariable.numOutsideScrew = 0;

        if (_handlingOutsidePieceCoroutine != null)
        {
            StopCoroutine(_handlingOutsidePieceCoroutine);
        }

        _handlingOutsidePieceCoroutine = StartCoroutine(HandlingOutsidePieces());
    }

    #region SCREW
    private void SetLevelPieceNumber(int pieceNumber)
    {
        _totalPiece = pieceNumber;
        _totalSpoolBoxes = _totalPiece / GameConstants.NUMBER_SLOT_PER_SCREW_BOX;

        updateRemainingPieceEvent?.Invoke(_totalPiece, _totalPiece);
    }

    private void AddPiece(BasePiece piece)
    {
        if (_pieces == null)
        {
            _pieces = new List<BasePiece>();
        }

        piece.name = $"Piece {_pieces.Count}";

        _pieces.Add(piece);

        _totalPieceObserved++;

        if (_totalPieceObserved == _totalPiece)
        {
            // TO SAVE/LOAD LEVEL PROGRESS, uncomment this
            setPiecesEvent?.Invoke(_pieces.ToArray());

            levelSetupCompletedEvent?.Invoke();

            if (_tweens == null)
            {
                _tweens = new List<Tween>();
            }

            _tweens.Add(Tween.Delay(0.1f).OnComplete(() =>
            {
                obstaclesOfPieces = GamePersistentVariable.pieceDependencies;
            }));

            SpawnFirstSpoolBoxes();
        }
    }

    private Dictionary<GameFaction, int> GetRemainingPieceByFaction()
    {
        Dictionary<GameFaction, int> remainingPieceByFaction = new Dictionary<GameFaction, int>();

        for (int i = 0; i < GameConstants.SCREW_FACTION.Length; i++)
        {
            remainingPieceByFaction.Add(GameConstants.SCREW_FACTION[i], 0);
        }

        for (int i = 0; i < _pieces.Count; i++)
        {
            // if (_pieces[i].IsLocked)
            // {
            //     continue;
            // }

            if (_pieces[i].IsDone)
            {
                continue;
            }

            remainingPieceByFaction[_pieces[i].Faction]++;
        }

        return remainingPieceByFaction;
    }

    private async Task<Dictionary<GameFaction, int>> GetDifficultyLevelByFaction(int? levelPhase = null)
    {
        // RULE: The more difficulty point, the less chance to spawn for a faction

        Dictionary<GameFaction, int> difficultyLevelByFactions = new Dictionary<GameFaction, int>();
        Dictionary<GameFaction, int> totalNotBlockedPieceByFaction = new Dictionary<GameFaction, int>();

        Dictionary<GameFaction, List<int>> numObstacleEachPieceEachFaction = new Dictionary<GameFaction, List<int>>();

        for (int i = 0; i < GameConstants.SCREW_FACTION.Length; i++)
        {
            difficultyLevelByFactions.Add(GameConstants.SCREW_FACTION[i], 0);

            numObstacleEachPieceEachFaction.Add(GameConstants.SCREW_FACTION[i], new List<int>());
        }

        // Find the number of obstacles for each piece in each faction.
        for (int i = 0; i < _pieces.Count; i++)
        {
            if (_pieces[i] == null)
            {
                continue;
            }

            // if (_pieces[i].IsDone || _pieces[i].IsLocked)
            // {
            //     continue;
            // }

            if (_pieces[i].IsDone)
            {
                continue;
            }

            if (i > 0 && i % 16 == 0)
            {
                await Task.Delay(20);
            }

            GameFaction faction = _pieces[i].Faction;

            // manual difficulty adjustment
            difficultyLevelByFactions[faction] += _pieces[i].pieceDifficultyModifier;

            int numCoveredPieces = 0;

            BasePiece parentPiece = _pieces[i].transform.parent.GetComponent<BasePiece>();

            if (parentPiece == null)
            {
                numCoveredPieces = 0;
            }
            else
            {
                BasePiece[] insidePieces = TransformUtil.GetComponentsFromAllChildren<BasePiece>(parentPiece.transform).ToArray();

                for (int j = 0; j < insidePieces.Length; j++)
                {
                    if (insidePieces[j] == _pieces[i])
                    {
                        numCoveredPieces = j;

                        break;
                    }
                }
            }

            numObstacleEachPieceEachFaction[faction].Add(numCoveredPieces);

            continue;

            // // Find closest obstacle
            // Transform obstaclePiece = null;

            // float pieceRadius = _pieces[i].transform.lossyScale.x * 0.45f * 0.779f * _pieces[i].MeshFilter.sharedMesh.bounds.size.x;

            // Vector3 pointOne =
            //     _pieces[i].transform.position + _pieces[i].transform.lossyScale.z * 1f * 0.779f * _pieces[i].MeshFilter.sharedMesh.bounds.size.z * _pieces[i].transform.forward;
            // Vector3 pointTwo = pointOne + 10f * _pieces[i].transform.forward;

            // Collider[] hits = new Collider[16];

            // Physics.OverlapCapsuleNonAlloc(pointOne, pointTwo, pieceRadius, hits, LayerMask.GetMask("Default"));

            // if (hits != null)
            // {
            //     for (int j = 0; j < hits.Length; j++)
            //     {
            //         if (hits[j] == null)
            //         {
            //             continue;
            //         }

            //         if (hits[j].transform.GetInstanceID() == _pieces[i].transform.parent.GetInstanceID())
            //         {
            //             continue;
            //         }

            //         IObjectPart objectPart = hits[j].transform.GetComponent<IObjectPart>();

            //         if (objectPart != null)
            //         {
            //             int instanceId = hits[j].transform.GetInstanceID();

            //             if (obstaclesOfPieces.ContainsKey(instanceId))
            //             {
            //                 obstaclePiece = hits[j].transform;

            //                 break;
            //             }
            //         }
            //     }
            // }
            // //

            // if (obstaclePiece != null)
            // {
            //     int numPieceLeftToReachCurrentPiece = obstaclePiece.childCount;

            //     if (obstaclesOfPieces.ContainsKey(obstaclePiece.GetInstanceID()))
            //     {
            //         foreach (var obstacle in obstaclesOfPieces[obstaclePiece.GetInstanceID()])
            //         {
            //             numPieceLeftToReachCurrentPiece += obstacle.childCount;
            //         }

            //         numObstacleEachPieceEachFaction[faction].Add(numPieceLeftToReachCurrentPiece);
            //     }
            //     else
            //     {
            //         numObstacleEachPieceEachFaction[faction].Add(0);
            //     }
            // }
            // else
            // {
            //     numObstacleEachPieceEachFaction[faction].Add(0);
            // }

            // // FOR Tutorial
            // difficultyLevelByFactions[faction] += _pieces[i].PieceDifficultyModifier;
        }

        List<GameFaction> factions = new List<GameFaction>(numObstacleEachPieceEachFaction.Keys);

        // Calculate difficulty of each faction based on total obstacles of first three most "blocked-less" pieces
        foreach (var faction in factions)
        {
            // not valid faction
            if (numObstacleEachPieceEachFaction[faction].Count < 3)
            {
                difficultyLevelByFactions[faction] += 99999;

                continue;
            }

            numObstacleEachPieceEachFaction[faction] = numObstacleEachPieceEachFaction[faction].OrderBy(item => item).ToList();

            int totalObstaclesOfThreeMostOpenedPieces = numObstacleEachPieceEachFaction[faction].Take(3).Sum();

            difficultyLevelByFactions[faction] += totalObstaclesOfThreeMostOpenedPieces;
        }

        // DECREASE Chance of factions that are already available in piece boxes
        for (int i = 0; i < pieceBoxManager.SpoolBoxs.Length; i++)
        {
            if (pieceBoxManager.SpoolBoxs[i] == null || pieceBoxManager.SpoolBoxs[i].IsLocked)
            {
                continue;
            }

            // avoid get factions that are already available in piece boxes when difficulty is max
            if (levelPhase != null)
            {
                if (levelPhase == GameConstants.SCREW_FACTION.Length - 1)
                {
                    difficultyLevelByFactions[pieceBoxManager.SpoolBoxs[i].Faction] -= 999;
                }
                else
                {
                    difficultyLevelByFactions[pieceBoxManager.SpoolBoxs[i].Faction] += 999;
                }
            }
            else
            {
                difficultyLevelByFactions[pieceBoxManager.SpoolBoxs[i].Faction] += 9999;
            }
        }

        // // Modify chance of factions of piece port
        // for (int i = 0; i < pieceBoxManager.PiecePorts.Count; i++)
        // {
        //     if (pieceBoxManager.PiecePorts[i].IsFilled)
        //     {
        //         // override old value
        //         difficultyLevelByFactions[pieceBoxManager.PiecePorts[i].Piece.Faction] =
        //             _currentLevelDifficultyConfiguration.PiecePortFactionDifficultyModifier;
        //     }
        // }

        return difficultyLevelByFactions;
    }

    private void OnPieceLoosed()
    {
        CheckWin();
    }

    private async void CheckWin()
    {
        Dictionary<GameFaction, int> remainingPieceByFaction = new Dictionary<GameFaction, int>();

        for (int i = 0; i < GameConstants.SCREW_FACTION.Length; i++)
        {
            remainingPieceByFaction.Add(GameConstants.SCREW_FACTION[i], 0);
        }

        for (int i = 0; i < _pieces.Count; i++)
        {
            if (_pieces[i].IsDone)
            {
                continue;
            }

            remainingPieceByFaction[_pieces[i].Faction]++;
        }

        int remainingPiece = remainingPieceByFaction.Sum(item => item.Value);

        updateRemainingPieceEvent?.Invoke(remainingPiece, _totalPiece);

        if (remainingPiece == 0)
        {
            GamePersistentVariable.numOutsideScrew = 0;

            updateOutsidePieceNumberEvent?.Invoke();
            enableBoosterEvent?.Invoke(false);

            await Task.Delay((int)(delayShowPopupWin * 1000));

            winLevelEvent?.Invoke();
        }

        levelObserver.Progress = 1 - (float)remainingPiece / _totalPiece;
    }
    #endregion

    #region AUTO TESTING
    private void StartAutoTesting()
    {
        if (_autoTestingCoroutine != null)
        {
            StopCoroutine(_autoTestingCoroutine);
        }

        GamePersistentVariable.isAutoTestingLevel = true;

        _autoTestingCoroutine = StartCoroutine(AutoTesting());
    }

    private IEnumerator AutoTesting()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.1f);

        while (true)
        {
            List<BasePiece> validPieces = new List<BasePiece>();

            for (int i = 0; i < _pieces.Count; i++)
            {
                if (_pieces[i] == null)
                {
                    continue;
                }

                if (!_pieces[i].gameObject.activeSelf)
                {
                    continue;
                }

                if (_pieces[i].IsDone)
                {
                    continue;
                }

                if (_pieces[i].IsOutsideWithNoSpoolBox || _pieces[i].IsInPiecePort)
                {
                    continue;
                }

                else
                {
                    validPieces.Add(_pieces[i]);
                }
            }

            int randomIndex = UnityEngine.Random.Range(0, validPieces.Count);

            if (validPieces.Count > 0)
            {
                validPieces[randomIndex].Select();
            }
            else
            {
                break;
            }

            yield return waitForSeconds;
        }
    }
    #endregion

    #region MANAGE SCREW BOX
    public async void SpawnFirstSpoolBoxes()
    {
        GameFaction[] sortedFactionByBlockedObject = await GetSortedFactionsByBlockedObject();

        // Spawn 2 default piece boxes
        for (int i = 0; i < GameConstants.DEFAULT_INITIAL_NUMBER_SCREW_BOXES; i++)
        {
            GameFaction faction = sortedFactionByBlockedObject[i];

            spawnSpoolBoxEvent?.Invoke(faction);
        }

        _totalSpoolBoxesSpawned += GameConstants.DEFAULT_INITIAL_NUMBER_SCREW_BOXES;

        spawnAdsSpoolBoxesEvent?.Invoke();
    }

    private async void SpawnFirstSpoolBoxesNewPhase(int phase)
    {
        await Task.Delay(2000);

        GameFaction[] sortedFactionByBlockedObject = await GetSortedFactionsByBlockedObject();

        int numberSpoolBoxToSpawn = 0;

        for (int i = 0; i < pieceBoxManager.SpoolBoxs.Length; i++)
        {
            if (pieceBoxManager.SpoolBoxs[i] == null)
            {
                numberSpoolBoxToSpawn++;
            }
        }

        for (int i = 0; i < numberSpoolBoxToSpawn; i++)
        {
            GameFaction faction = sortedFactionByBlockedObject[i];

            spawnSpoolBoxEvent?.Invoke(faction);
        }
    }

    private async void SpawnNewSpoolBox()
    {
        if (_totalSpoolBoxesSpawned >= _totalSpoolBoxes)
        {
            return;
        }

        GameFaction faction = await GetFactionForNewSpoolBox();

        if (faction == GameFaction.None)
        {
            return;
        }

        spawnSpoolBoxEvent?.Invoke(faction);

        _totalSpoolBoxesSpawned++;
        // _remainingSpawnSpoolBoxCommands--;

        // _isHandlingSpawningSpoolBox = false;
    }

    private void OnSpoolBoxUnlocked(SpoolBox pieceBox)
    {
        _totalSpoolBoxesSpawned++;
    }

    private void AssignFactionForNewAdsSpoolBox(SpoolBox pieceBox)
    {
        AssignFactionForNewSpoolBox(pieceBox, _currentLevelDifficultyConfiguration.AdsScrewBoxDifficultyLevelModifier);
    }

    private async Task AssignFactionForNewSpoolBox(SpoolBox pieceBox, int difficultyLevelModifier)
    {
        GameFaction faction = await GetFactionForNewSpoolBox(difficultyLevelModifier);

        if (faction == GameFaction.None)
        {
            return;
        }

        pieceBox.Faction = faction;
    }

    private async Task<GameFaction[]> GetSortedFactionsByBlockedObject()
    {
        int maxNumberBlockingObject = 0;

        for (int i = 0; i < _pieces.Count; i++)
        {
            _pieces[i].CountBlockingObjects(isIncludeHiddenPiece: true);

            if (_pieces[i].NumberBlockingObjects > maxNumberBlockingObject)
            {
                maxNumberBlockingObject = _pieces[i].NumberBlockingObjects;
            }
        }

        Dictionary<GameFaction, int> piecePortAvailableByFaction = pieceBoxManager.GetPiecePortAvailableByFaction();
        Dictionary<GameFaction, int> remainingPieceByFaction = GetRemainingPieceByFaction();
        Dictionary<GameFaction, int> difficultyLevelByFaction = await GetDifficultyLevelByFaction();

        Dictionary<GameFaction, int> shuffledDifficultyLevelByFaction = ShuffleUtil.ShuffleItemsWithSameValue(difficultyLevelByFaction);

        GameFaction[] factionSortedByDifficulty = shuffledDifficultyLevelByFaction.OrderBy(item => item.Value).Select(item => item.Key).ToArray();

        return factionSortedByDifficulty;
    }

    private async Task<GameFaction> GetFactionForNewSpoolBox(int difficultyLevelModifier = 0)
    {
        float progress;
        int donePiece = 0;

        for (int i = 0; i < _pieces.Count; i++)
        {
            if (_pieces[i].IsDone)
            {
                donePiece++;
            }
        }

        progress = (float)donePiece / _totalPiece;

        int difficultyLevel = 0;

        for (int i = 0; i < _currentLevelDifficultyConfiguration.LevelPhases.Length; i++)
        {
            LevelPhase levelPhase = _currentLevelDifficultyConfiguration.LevelPhases[i];

            if (progress <= levelPhase.EndProgress)
            {
                difficultyLevel = levelPhase.PhaseIndex;

                break;
            }
        }

        difficultyLevel += difficultyLevelModifier;
        difficultyLevel = Mathf.Max(difficultyLevel, 0);

        // DIFFICULTY REDUCED AFTER REWARDED ADS
        if (_isReducedDifficultyAfterRewardedAds)
        {
            if (GamePersistentVariable.numSpoolBoxUnlocked >= 1)
            {
                difficultyLevel = 0;
            }
        }

        // // FREE LEVELS
        // int minFreeLevel = RemoteConfigController.GetIntConfig(FirebaseConfig.MIN_FREE_LEVEL, 20);

        // if (currentLevel.Value <= minFreeLevel)
        // {
        //     difficultyLevel = 0;
        // }

        // HARD MODE
        if (GamePersistentVariable.isHardMode)
        {
            difficultyLevel = _currentLevelDifficultyConfiguration.LevelPhases.Length - 1;
        }

        Dictionary<GameFaction, int> piecePortAvailableByFaction = pieceBoxManager.GetPiecePortAvailableByFaction();
        DebugLogger.LogDictionary(piecePortAvailableByFaction, title: "piecePortAvailableByFaction");

        Dictionary<GameFaction, int> remainingPieceByFaction = GetRemainingPieceByFaction();
        DebugLogger.LogDictionary(remainingPieceByFaction, title: "remainingPieceByFaction");

        Dictionary<GameFaction, int> totalBlockObjectsByFaction = await GetDifficultyLevelByFaction(difficultyLevel);
        DebugLogger.LogDictionary(totalBlockObjectsByFaction, title: "totalBlockObjectsByFaction");

        Dictionary<GameFaction, int> shuffledDifficultyLevelByFaction = ShuffleUtil.ShuffleItemsWithSameValue(totalBlockObjectsByFaction);
        DebugLogger.LogDictionary(shuffledDifficultyLevelByFaction, title: "shuffledDifficultyLevelByFaction");

        GameFaction[] factionSortedByDifficulty = shuffledDifficultyLevelByFaction.OrderBy(item => item.Value).Select(item => item.Key).ToArray();

        GameFaction nextFaction = GameFaction.None;

        bool isFound = false;

        // // DEBUG
        // Debug.Log("SAFERIO --------------------------------------- " + difficultyLevel);

        // for (int i = 0; i < factionSortedByDifficulty.Length; i++)
        // {
        //     GameFaction faction = factionSortedByDifficulty[i];

        //     string debugString = $"SAFERIO {faction} Remaining: {remainingPieceByFaction[faction]}";

        //     debugString += $" Available: {piecePortAvailableByFaction[faction]}";
        //     debugString += $" <color=#FF6060>Difficulty: {totalBlockObjectsByFaction[faction]}</color>";

        //     Debug.Log(debugString);
        // }

        for (int i = difficultyLevel; i >= 0; i--)
        {
            GameFaction faction = factionSortedByDifficulty[i];

            if ((remainingPieceByFaction[faction] - piecePortAvailableByFaction[faction]) >= 3)
            {
                nextFaction = faction;

                isFound = true;

                break;
            }
        }

        if (!isFound)
        {
            for (int i = difficultyLevel + 1; i < factionSortedByDifficulty.Length; i++)
            {
                GameFaction faction = factionSortedByDifficulty[i];

                if ((remainingPieceByFaction[faction] - piecePortAvailableByFaction[faction]) >= 3)
                {
                    nextFaction = faction;

                    isFound = true;

                    break;
                }
            }
        }

        if (!isFound)
        {
            for (int i = 0; i < factionSortedByDifficulty.Length; i++)
            {
                GameFaction faction = factionSortedByDifficulty[i];

                if ((remainingPieceByFaction[faction] - piecePortAvailableByFaction[faction]) < 3)
                {
                    continue;
                }
                else
                {
                    nextFaction = faction;

                    isFound = true;

                    break;
                }
            }
        }

        return nextFaction;
    }
    #endregion

    #region HANDLE SCREWS THAT ARE OUTSIDE WITH NO SCREW BOX
    private void OnSpoolBoxSpawned(SpoolBox pieceBox)
    {
        _remainingHandleOutsidePieceCommands++;
    }

    private IEnumerator HandlingOutsidePieces()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f);

        // wait for level loaded
        yield return new WaitForSeconds(3f);

        while (true)
        {
            if (!_isHandlingOutsidePieces)
            {
                _isHandlingOutsidePieces = true;

                yield return HandleOutsidePieces();
            }

            yield return waitForSeconds;
        }
    }

    private async Task HandleOutsidePieces()
    {
        if (_pieces == null)
        {
            return;
        }

        bool isFound = false;

        for (int i = 0; i < _pieces.Count; i++)
        {
            if (i % 16 == 0 && i > 0)
            {
                await Task.Delay(100);
            }

            try
            {
                BasePiece piece = _pieces[i];

                if (piece.IsOutsideWithNoSpoolBox && !piece.PieceServiceLocator.pieceProperty.IsRewinding)
                {
                    GameFaction faction = piece.Faction;

                    SpoolBoxSlot pieceBoxSlot = pieceBoxManager.CheckAvailableSpoolBoxes(faction, isIncludePiecePorts: false);

                    if (pieceBoxSlot != null)
                    {
                        piece.PieceServiceLocator.pieceMeshRenderer.enabled = true;

                        piece.IsInteractable = true;

                        GamePersistentVariable.numOutsideScrew--;

                        piece.LooseFromOutside(piece.gameObject.GetInstanceID(), faction, pieceBoxSlot);

                        piece.IsOutsideWithNoSpoolBox = false;

                        if (!isFound)
                        {
                            isFound = true;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {

            }
        }

        if (isFound)
        {
            updateOutsidePieceNumberEvent?.Invoke();
        }

        // _remainingHandleOutsidePieceCommands--;
        _isHandlingOutsidePieces = false;
    }
    #endregion

    #region Very Important: PIECE - BLOCK PIECES Dictionary
    private void OnPieceCompleted(int instanceId)
    {
        obstaclesOfPieces.Remove(instanceId);
    }
    #endregion

    #region Test Difficulty 
    private void ChangeDifficulty(float difficulty)
    {
        // GamePersistentVariable.testDifficultyMultiplier = difficulty;

        _currentLevelDifficultyConfiguration = LevelDifficultyCalculatorUltility.GenerateScrewBoxDifficultyTemplate(currentLevel.Value);
    }
    #endregion

    #region MAGIC PICK
    private void MagicPick()
    {
        GameFaction[] validFactions = SpoolManager.Instance.SpoolBoxs
            .Where(item => item != null)
            .Where(item => !item.IsLocked && !item.IsInTransition)
            .Select(item => item.Faction).ToArray();

        BasePiece selectedPiece = _pieces.First(
            item => !item.IsDone &&
            !item.IsInPiecePort &&
            !item.PieceServiceLocator.pieceProperty.IsRewinding &&
            validFactions.Contains(item.Faction)
        );

        if (selectedPiece != null)
        {
            magicPickPieceEvent?.Invoke(selectedPiece.gameObject.GetInstanceID(), selectedPiece.Faction);
        }
    }
    #endregion
}
