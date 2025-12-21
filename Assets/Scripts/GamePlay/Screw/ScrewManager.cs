using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NamPhuThuy;
using NamPhuThuy.GamePlay;
using PrimeTween;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using static GameEnum;

namespace NamPhuThuy.GamePlay
{
    public class ScrewManager : MonoBehaviour
    {
        [SerializeField] private ScrewBoxManager screwBoxManager;

        [Header("SCRIPTABLE OBJECT")]
        [SerializeField] private IntVariable currentLevel;
        [SerializeField] private LevelObserver levelObserver;

        [Header("CUSTOMIZE")]
        [SerializeField] private float delayShowPopupWin;
        [SerializeField] private LevelDifficultyConfiguration currentLevelDifficultyConfig;

        [Header("Managers")]
        [SerializeField] private LockScrewColorLevelManager _lockScrewColorLevelManager;
        [SerializeField] private RustyScrewSpecialLevelManager _rustyScrewSpecialLevelManager;
        [SerializeField] private LockScrewBoxManager _lockScrewBoxManager;


        #region PRIVATE FIELD
        private List<Tween> _tweens;
        private List<BaseScrew> _screws;
        private int _totalScrewObserved;
        private int _totalScrew;
        private Coroutine _handlingOutsideScrewCoroutine;
        private Coroutine _autoTestingCoroutine;

        #endregion

        #region QUEUE SPAWN SCREW BOXES
        private int _remainingSpawnScrewBoxCommands;
        private bool _isHandlingSpawningScrewBox;
        #endregion

        #region QUEUE OUTSIDE SCREWS
        private bool _isHandlingOutsideScrews;
        #endregion

        #region REMOTE CONFIG
        private bool _isReducedDifficultyAfterRewardedAds;
        #endregion

        #region EVENT
        public static event Action<GameFaction> spawnScrewBoxEvent;
        public static event Action spawnAdsScrewBoxesEvent;
        public static event Action winLevelEvent;
        public static event Action<BaseScrew[]> setScrewsEvent;
        public static event Action<int, int> updateRemainingScrewEvent;
        public static event Action<bool> enableBoosterEvent;
        public static event Action levelSetupCompletedEvent;
        public static event Action updateOutsideScrewNumberEvent;
        public static event Action<BaseScrew[]> switchScrewsToSpecialTypeCompleteEvent;
        #endregion

        #region LIFE CYCLE


        #region Very important - PIECE DEPENDENCY DIAGRAM
        Dictionary<int, List<Transform>> obstaclesOfPieces = new Dictionary<int, List<Transform>>();
        #endregion

        void Awake()
        {
            LevelLoader.startLevelEvent += OnLevelStart;
            LevelLoader.setLevelScrewNumberEvent += SetLevelScrewNumber;
            BaseScrew.addScrewToListEvent += AddScrew;
            ScrewBox.spawnNewScrewBoxEvent += SpawnNewScrewBox;
            ScrewBox.setFactionForScrewBoxEvent += AssignFactionForNewAdsScrewBox;
            ScrewBox.screwBoxUnlockedEvent += OnScrewBoxUnlocked;
            BaseScrew.screwLoosenedEvent += OnScrewLoosed;
            ScrewsDataManager.spawnFreshLevelScrewBoxesEvent += SpawnFirstScrewBoxes;
            MultiPhaseLevelManager.switchPhaseEvent += SpawnFirstScrewBoxesNewPhase;
            ScrewBoxManager.screwBoxSpawnedEvent += OnScrewBoxSpawned;
            BasicObjectPart.pieceCompleteEvent += OnPieceCompleted;
            DebugPopup.startAutoTestLevelEvent += StartAutoTesting;
            LockScrewColorLevelManager.switchToLockedScrewsEvent += SwitchToLockedScrews;
            LockScrewColorLevelManager.lockScrewColorManagerInitEvent += OnLockScrewColorManagerInit;

            RustyScrewSpecialLevelManager.switchToRustyScrewsEvent += SwitchToRustyScrews;
            RustyScrewSpecialLevelManager.rustyScrewManagerInitEvent += OnRustyScrewManagerInit;

            LockScrewBoxManager.lockScrewBoxManagerInitEvent += OnLockScrewBoxInit;

            _tweens = new List<Tween>();

            // StartCoroutine(QueueSpawningScrewBoxes());
            // StartCoroutine(HandlingOutsideScrews());

            _isReducedDifficultyAfterRewardedAds = RemoteConfigController.GetBoolConfig(FirebaseConfig.IS_REDUCE_DIFFICULTY_AFTER_REWARDED_AD, true);
        }




        void OnDestroy()
        {
            LevelLoader.startLevelEvent -= OnLevelStart;
            LevelLoader.setLevelScrewNumberEvent -= SetLevelScrewNumber;
            BaseScrew.addScrewToListEvent -= AddScrew;
            ScrewBox.spawnNewScrewBoxEvent -= SpawnNewScrewBox;
            ScrewBox.setFactionForScrewBoxEvent -= AssignFactionForNewAdsScrewBox;
            ScrewBox.screwBoxUnlockedEvent -= OnScrewBoxUnlocked;
            BaseScrew.screwLoosenedEvent -= OnScrewLoosed;
            ScrewsDataManager.spawnFreshLevelScrewBoxesEvent -= SpawnFirstScrewBoxes;
            MultiPhaseLevelManager.switchPhaseEvent -= SpawnFirstScrewBoxesNewPhase;
            ScrewBoxManager.screwBoxSpawnedEvent -= OnScrewBoxSpawned;
            BasicObjectPart.pieceCompleteEvent -= OnPieceCompleted;
            DebugPopup.startAutoTestLevelEvent -= StartAutoTesting;
            LockScrewColorLevelManager.switchToLockedScrewsEvent -= SwitchToLockedScrews;
            RustyScrewSpecialLevelManager.switchToRustyScrewsEvent -= SwitchToRustyScrews;

            CommonUtil.StopAllTweens(_tweens);
        }
        #endregion

        private void OnLevelStart()
        {
            // DebugLogger.LogSimple();
            _screws = new List<BaseScrew>();

            _totalScrewObserved = 0;

            currentLevelDifficultyConfig = LevelDifficultyCalculatorUltility.GenerateScrewBoxDifficultyTemplate(currentLevel.Value);
            currentLevelDifficultyConfig.ScrewPortFactionDifficultyModifier =
                RemoteConfigController.GetIntConfig(FirebaseConfig.SCREW_PORT_FACTION_DIFFICULTY_MODIFIER, 0);

            ScrewBoxManager.TotalScrewBoxesSpawned = 0;

            _remainingSpawnScrewBoxCommands = 0;
            _isHandlingSpawningScrewBox = false;
            _isHandlingOutsideScrews = false;

            GamePersistentVariable.numScrewBoxUnlocked = 0;
            GamePersistentVariable.numOutsideScrew = 0;

            if (_handlingOutsideScrewCoroutine != null)
            {
                StopCoroutine(_handlingOutsideScrewCoroutine);
            }

            _handlingOutsideScrewCoroutine = StartCoroutine(HandlingOutsideScrews());
        }

        #region SCREW
        private void SetLevelScrewNumber(int screwNumber)
        {
            // DebugLogger.LogSimple();
            _totalScrew = screwNumber;
            ScrewBoxManager.TotalScrewBoxes = _totalScrew / GameConstants.NUMBER_SLOT_PER_SCREW_BOX;

            updateRemainingScrewEvent?.Invoke(_totalScrew, _totalScrew);
        }

        private void AddScrew(BaseScrew screw)
        {
            // DebugLogger.LogSimple();
            if (_screws == null)
            {
                _screws = new List<BaseScrew>();
            }

            screw.name = $"Screw 3D {_screws.Count}";

            _screws.Add(screw);

            _totalScrewObserved++;

            if (_totalScrewObserved == _totalScrew)
            {
                // TO SAVE/LOAD LEVEL PROGRESS, uncomment this
                setScrewsEvent?.Invoke(_screws.ToArray());

                levelSetupCompletedEvent?.Invoke();

                if (_tweens == null)
                {
                    _tweens = new List<Tween>();
                }

                _tweens.Add(Tween.Delay(0.1f).OnComplete(() =>
                {
                    obstaclesOfPieces = GamePersistentVariable.pieceDependencies;
                }));
            }
        }

        private Dictionary<GameFaction, int> GetRemainingScrewByFaction()
        {
            Dictionary<GameFaction, int> remainingScrewByFaction = new Dictionary<GameFaction, int>();

            for (int i = 0; i < GameConstants.SCREW_FACTION.Length; i++)
            {
                remainingScrewByFaction.Add(GameConstants.SCREW_FACTION[i], 0);
            }

            for (int i = 0; i < _screws.Count; i++)
            {
                if (_screws[i].IsLocked) { continue; }
                if (_screws[i].IsDone) { continue; }

                remainingScrewByFaction[_screws[i].Faction]++;
            }
            return remainingScrewByFaction;
        }

        private async Task<Dictionary<GameFaction, int>> GetDifficultyLevelByFaction(int? levelPhase = null)
        {
            // RULE: The more difficulty point, the less chance to spawn for a faction

            /*Dictionary<GameFaction, int> difficultyLevelByFactions = InitializeFactionDictionaries();
            Dictionary<GameFaction, List<int>> numObstacleEachScrewEachFaction = InitializeObstacleTrackingDictionaries();*/

            Dictionary<GameFaction, int> difficultyLevelByFactions = new Dictionary<GameFaction, int>();
            Dictionary<GameFaction, List<int>> numObstacleEachScrewEachFaction = new Dictionary<GameFaction, List<int>>();

            for (int i = 0; i < GameConstants.SCREW_FACTION.Length; i++)
            {
                difficultyLevelByFactions.Add(GameConstants.SCREW_FACTION[i], 0);
                numObstacleEachScrewEachFaction.Add(GameConstants.SCREW_FACTION[i], new List<int>());
            }

            // Find the number of obstacles for each screw in each faction.
            for (int i = 0; i < _screws.Count; i++)
            {
                if (_screws[i] == null)
                {
                    continue;
                }

                if (_screws[i].IsDone || _screws[i].IsLocked)
                {
                    continue;
                }

                if (i > 0 && i % 16 == 0)
                {
                    await Task.Delay(20);
                }

                GameFaction faction = _screws[i].Faction;

                // Find closest obstacle
                Transform obstaclePiece = null;

                float screwRadius = _screws[i].transform.lossyScale.x * 0.45f * 0.779f * _screws[i].MeshFilter.sharedMesh.bounds.size.x;

                Vector3 pointOne = _screws[i].transform.position + _screws[i].transform.lossyScale.z * 1f * 0.779f * _screws[i].MeshFilter.sharedMesh.bounds.size.z * _screws[i].transform.forward;

                Vector3 pointTwo = pointOne + 10f * _screws[i].transform.forward;

                Collider[] hits = new Collider[16];

                Physics.OverlapCapsuleNonAlloc(pointOne, pointTwo, screwRadius, hits, LayerMask.GetMask("Default"));

                for (int j = 0; j < hits.Length; j++)
                {
                    if (hits[j] == null) { continue; }

                    if (hits[j].transform.GetInstanceID() == _screws[i].transform.parent.GetInstanceID())
                    { continue; }

                    IObjectPart objectPart = hits[j].transform.GetComponent<IObjectPart>();

                    if (objectPart == null) { continue; }

                    int instanceId = hits[j].transform.GetInstanceID();
                    if (obstaclesOfPieces.ContainsKey(instanceId))
                    {
                        obstaclePiece = hits[j].transform;

                        break;
                    }
                }

                if (obstaclePiece != null)
                {
                    int numScrewLeftToReachCurrentScrew = obstaclePiece.childCount;

                    if (obstaclesOfPieces.ContainsKey(obstaclePiece.GetInstanceID()))
                    {
                        foreach (var obstacle in obstaclesOfPieces[obstaclePiece.GetInstanceID()])
                        {
                            numScrewLeftToReachCurrentScrew += obstacle.childCount;
                        }

                        numObstacleEachScrewEachFaction[faction].Add(numScrewLeftToReachCurrentScrew);
                    }
                    else
                    {
                        numObstacleEachScrewEachFaction[faction].Add(0);
                    }
                }
                else
                {
                    numObstacleEachScrewEachFaction[faction].Add(0);
                }

                // FOR Tutorial
                difficultyLevelByFactions[faction] += _screws[i].ScrewDifficultyModifier;
            }

            List<GameFaction> factions = new List<GameFaction>(numObstacleEachScrewEachFaction.Keys);

            // Calculate difficulty of each faction based on total obstacles of first three most "blocked-less" screws
            foreach (var faction in factions)
            {
                // not valid faction
                if (numObstacleEachScrewEachFaction[faction].Count < 3)
                {
                    difficultyLevelByFactions[faction] += 99999;
                    continue;
                }

                // if (_rustyScrewSpecialLevelManager != null && _rustyScrewSpecialLevelManager.IsFactionLocked(faction))
                // {
                //     difficultyLevelByFactions[faction] += 9900;

                //     continue;
                // }

                // if (_lockScrewColorLevelManager != null && _lockScrewColorLevelManager.IsFactionLocked(faction))
                // {
                //     difficultyLevelByFactions[faction] += 9900;

                //     continue;
                // }

                // if (_lockScrewBoxManager != null && _lockScrewBoxManager.IsFactionLocked(faction))
                // {
                //     float progressPercent = GetLevelCompletePercent();

                //     if (progressPercent <= 40)
                //         difficultyLevelByFactions[faction] += 9900;
                //     else
                //     {
                //         difficultyLevelByFactions[faction] -= 999;
                //     }

                //     continue;
                // }

                numObstacleEachScrewEachFaction[faction] = numObstacleEachScrewEachFaction[faction].OrderBy(item => item).ToList();

                int totalObstaclesOfThreeMostOpenedScrews = numObstacleEachScrewEachFaction[faction].Take(3).Sum();

                difficultyLevelByFactions[faction] += totalObstaclesOfThreeMostOpenedScrews;
            }

            // DECREASE Chance of factions that are already available in screw boxes
            for (int i = 0; i < screwBoxManager.ScrewBoxs.Length; i++)
            {
                if (screwBoxManager.ScrewBoxs[i] == null) { continue; }
                if (screwBoxManager.ScrewBoxs[i].IsLocked) { continue; }

                // avoid get factions that are already available in screw boxes when difficulty is max
                if (levelPhase != null)
                {
                    if (levelPhase == GameConstants.SCREW_FACTION.Length - 1)
                    {
                        difficultyLevelByFactions[screwBoxManager.ScrewBoxs[i].Faction] -= 999;
                    }
                    else
                    {
                        difficultyLevelByFactions[screwBoxManager.ScrewBoxs[i].Faction] += 999;
                    }
                }
                else
                {
                    difficultyLevelByFactions[screwBoxManager.ScrewBoxs[i].Faction] += 9999;
                }
            }

            return difficultyLevelByFactions;

            Dictionary<GameFaction, int> InitializeFactionDictionaries()
            {
                var difficultyLevelByFactions = new Dictionary<GameFaction, int>();

                for (int i = 0; i < GameConstants.SCREW_FACTION.Length; i++)
                {
                    difficultyLevelByFactions.Add(GameConstants.SCREW_FACTION[i], 0);
                }

                return difficultyLevelByFactions;
            }

            Dictionary<GameFaction, List<int>> InitializeObstacleTrackingDictionaries()
            {
                var numObstacleEachScrewEachFaction = new Dictionary<GameFaction, List<int>>();

                for (int i = 0; i < GameConstants.SCREW_FACTION.Length; i++)
                {
                    numObstacleEachScrewEachFaction.Add(GameConstants.SCREW_FACTION[i], new List<int>());
                }

                return numObstacleEachScrewEachFaction;
            }
        }

        private struct ScrewDetectionParameters
        {
            public Vector3 PointOne;
            public Vector3 PointTwo;
            public float Radius;
        }

        private void OnScrewLoosed(BaseScrew screw)
        {
            // DebugLogger.LogSimple();
            CheckWin();
        }

        private async void CheckWin()
        {
            Dictionary<GameFaction, int> remainingScrewByFaction = new Dictionary<GameFaction, int>();

            for (int i = 0; i < GameConstants.SCREW_FACTION.Length; i++)
            {
                remainingScrewByFaction.Add(GameConstants.SCREW_FACTION[i], 0);
            }

            for (int i = 0; i < _screws.Count; i++)
            {
                if (_screws[i].IsDone)
                {
                    continue;
                }

                remainingScrewByFaction[_screws[i].Faction]++;
            }

            int remainingScrew = remainingScrewByFaction.Sum(item => item.Value);

            updateRemainingScrewEvent?.Invoke(remainingScrew, _totalScrew);

            if (remainingScrew == 0)
            {
                GamePersistentVariable.numOutsideScrew = 0;

                updateOutsideScrewNumberEvent?.Invoke();
                enableBoosterEvent?.Invoke(false);

                await Task.Delay((int)(delayShowPopupWin * 1000));

                winLevelEvent?.Invoke();
            }

            levelObserver.Progress = 1 - (float)remainingScrew / _totalScrew;
        }

        // Add this method inside the ScrewManager class
        public float GetLevelCompletePercent()
        {
            DebugLogger.LogSimple();
            if (_screws == null || _totalScrew <= 0) return 0f;

            int done = 0;
            for (int i = 0; i < _screws.Count; i++)
            {
                var s = _screws[i];
                if (s != null && s.IsDone) done++;
            }

            return (done / (float)_totalScrew);
        }

        // Optional: if you prefer using the existing progress calculation
        public float GetLevelCompletePercentFast()
        {
            // CalculateCurrentProgress returns 0..1
            float p = 0f;
            try { p = CalculateCurrentProgress(); } catch { }
            return Mathf.Clamp01(p) * 100f;
        }

        #endregion

        #region AUTO TESTING
        private void StartAutoTesting()
        {
            // DebugLogger.LogSimple();
            if (_autoTestingCoroutine != null)
            {
                StopCoroutine(_autoTestingCoroutine);
            }

            GamePersistentVariable.isAutoTestingLevel = true;

            _autoTestingCoroutine = StartCoroutine(AutoTesting());
        }

        private IEnumerator AutoTesting()
        {
            // DebugLogger.LogSimple();
            WaitForSeconds waitForSeconds = new WaitForSeconds(0.05f);

            while (true)
            {
                List<BaseScrew> validScrews = new List<BaseScrew>();

                for (int i = 0; i < _screws.Count; i++)
                {
                    if (_screws[i].IsDone)
                    {
                        continue;
                    }

                    if (_screws[i].IsOutsideWithNoScrewBox || _screws[i].IsInScrewPort)
                    {
                        continue;
                    }

                    else
                    {
                        validScrews.Add(_screws[i]);
                    }
                }

                int randomIndex = UnityEngine.Random.Range(0, validScrews.Count);

                if (validScrews.Count > 0)
                {
                    validScrews[randomIndex].Select();
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
        public async void SpawnFirstScrewBoxes()
        {
            // DebugLogger.LogSimple();
            GameFaction[] sortedFactionByBlockedObject = await GetSortedFactionsByBlockedObject();

            // Spawn 2 default screw boxes
            for (int i = 0; i < GameConstants.DEFAULT_INITIAL_NUMBER_SCREW_BOXES; i++)
            {
                GameFaction faction = sortedFactionByBlockedObject[i];

                // DebugLogger.LogSimple(message: $"Trigger spawnScrewBoxEvent");
                spawnScrewBoxEvent?.Invoke(faction);
            }

            ScrewBoxManager.TotalScrewBoxesSpawned += GameConstants.DEFAULT_INITIAL_NUMBER_SCREW_BOXES;

            // DebugLogger.LogSimple(message: $"Trigger spawnAdsScrewBoxesEvent");
            spawnAdsScrewBoxesEvent?.Invoke();
        }

        private async void SpawnFirstScrewBoxesNewPhase(int phase)
        {
            // DebugLogger.LogSimple();
            await Task.Delay(2000);
            // DebugLogger.LogSimple(message: $"Continue after 2 secs");

            GameFaction[] sortedFactionByBlockedObject = await GetSortedFactionsByBlockedObject();

            int numberScrewBoxToSpawn = 0;

            for (int i = 0; i < screwBoxManager.ScrewBoxs.Length; i++)
            {
                if (screwBoxManager.ScrewBoxs[i] == null)
                {
                    numberScrewBoxToSpawn++;
                }
            }

            for (int i = 0; i < numberScrewBoxToSpawn; i++)
            {
                GameFaction faction = sortedFactionByBlockedObject[i];

                // DebugLogger.LogSimple(message: $"Trigger spawnScrewBoxEvent");
                spawnScrewBoxEvent?.Invoke(faction);
            }
        }

        private async void SpawnNewScrewBox()
        {
            // DebugLogger.LogSimple(); 
            if (ScrewBoxManager.TotalScrewBoxesSpawned >= ScrewBoxManager.TotalScrewBoxes)
            {
                return;
            }

            GameFaction faction = await GetFactionForNewScrewBox();

            if (faction == GameFaction.None)
            {
                return;
            }

            // DebugLogger.LogSimple(message: $"Trigger spawnScrewBoxEvent");
            spawnScrewBoxEvent?.Invoke(faction);

            ScrewBoxManager.TotalScrewBoxesSpawned++;
        }

        private void OnScrewBoxUnlocked(ScrewBox screwBox)
        {
            // DebugLogger.LogSimple();
            ScrewBoxManager.TotalScrewBoxesSpawned++;
        }

        private void AssignFactionForNewAdsScrewBox(ScrewBox screwBox)
        {
            // DebugLogger.LogSimple();
            AssignFactionForNewScrewBox(screwBox, currentLevelDifficultyConfig.AdsScrewBoxDifficultyLevelModifier);
        }

        private async Task AssignFactionForNewScrewBox(ScrewBox screwBox, int difficultyLevelModifier)
        {
            GameFaction faction = await GetFactionForNewScrewBox(difficultyLevelModifier);

            if (faction == GameFaction.None)
            {
                return;
            }

            screwBox.Faction = faction;
        }

        private async Task<GameFaction[]> GetSortedFactionsByBlockedObject()
        {
            int maxNumberBlockingObject = 0;

            for (int i = 0; i < _screws.Count; i++)
            {
                _screws[i].CountBlockingObjects(isIncludeHiddenScrew: true);

                if (_screws[i].NumberBlockingObjects > maxNumberBlockingObject)
                {
                    maxNumberBlockingObject = _screws[i].NumberBlockingObjects;
                }
            }

            Dictionary<GameFaction, int> screwPortAvailableByFaction = screwBoxManager.GetScrewSlotsAvailableByFaction();
            Dictionary<GameFaction, int> remainingScrewByFaction = GetRemainingScrewByFaction();
            Dictionary<GameFaction, int> difficultyLevelByFaction = await GetDifficultyLevelByFaction();

            Dictionary<GameFaction, int> shuffledDifficultyLevelByFaction = ShuffleUtil.ShuffleItemsWithSameValue(difficultyLevelByFaction);

            GameFaction[] factionSortedByDifficulty = shuffledDifficultyLevelByFaction.OrderBy(item => item.Value).Select(item => item.Key).ToArray();

            return factionSortedByDifficulty;
        }


        #endregion

        #region Difficulty Calculate

        private async Task<GameFaction> GetFactionForNewScrewBox(int difficultyLevelModifier = 0)
        {
            GameFaction nextFaction = GameFaction.None;
            float progress = CalculateCurrentProgress();
            // int baseDifficultyLevel = GetCurrentDifficultyPhase(progress);

            int difficultyLevel = 0;
            difficultyLevel = GetCurrentDifficultyPhase(progress);
            difficultyLevel = ApplyDifficultyModifiers(difficultyLevel, difficultyLevelModifier);

            Dictionary<GameFaction, int> screwSlotAvailableByFaction = screwBoxManager.GetScrewSlotsAvailableByFaction();
            Dictionary<GameFaction, int> remainingScrewByFaction = GetRemainingScrewByFaction();
            Dictionary<GameFaction, int> totalBlockObjectsByFaction = await GetDifficultyLevelByFaction(difficultyLevel);

            Dictionary<GameFaction, int> shuffledDifficultyLevelByFaction = ShuffleUtil.ShuffleItemsWithSameValue(totalBlockObjectsByFaction);

            // // Debug difficulty by faction
            // Debug.Log($"=== SAFERIO - Difficulty {difficultyLevel} === ");

            // foreach (var item in shuffledDifficultyLevelByFaction)
            // {
            //     Debug.Log($"SAFERIO {item.Key} - {item.Value}");
            // }
            // //

            GameFaction[] factionSortedByDifficulty = shuffledDifficultyLevelByFaction.OrderBy(item => item.Value).Select(item => item.Key).ToArray();

            // gen screw box for special levels
            if (GamePersistentVariable.currentLevelMode != LevelMode.Normal)
            {
                for (int i = 0; i < screwBoxManager.ScrewBoxs.Length; i++)
                {
                    if (screwBoxManager.ScrewBoxs[i] == null) { continue; }
                    if (screwBoxManager.ScrewBoxs[i].IsLocked) { continue; }

                }
            }

            bool isFound = false;

            for (int i = difficultyLevel; i >= 0; i--)
            {
                GameFaction faction = factionSortedByDifficulty[i];

                if ((remainingScrewByFaction[faction] - screwSlotAvailableByFaction[faction]) >= 3)
                {
                    nextFaction = faction;

                    isFound = true;

                    break;
                }
            }

            // Special Level
            if (GamePersistentVariable.currentLevelMode != LevelMode.Normal)
            {
                float progressPercent = GetLevelCompletePercent();

                if (progressPercent > 1 - RemoteConfigController.GetFloatConfig(FirebaseConfig.specialLevelDifficultyMultiplier, 0.0f))
                {
                    GameFaction lockedFaction = GameFaction.None;

                    if (_rustyScrewSpecialLevelManager != null)
                    {
                        lockedFaction = _rustyScrewSpecialLevelManager.LockedScrewFaction;
                    }

                    if (_lockScrewColorLevelManager != null)
                    {
                        lockedFaction = _lockScrewColorLevelManager.LockedScrewFaction;
                    }

                    if (_lockScrewBoxManager != null)
                    {
                        if (progressPercent <= 0.4f)
                        {
                            lockedFaction = _lockScrewBoxManager.LockedScrewFaction;
                        }
                    }

                    if (lockedFaction != GameFaction.None)
                    {
                        if (shuffledDifficultyLevelByFaction[lockedFaction] < 900)
                        {
                            nextFaction = lockedFaction;
                        }
                    }
                }
            }

            if (!isFound)
            {
                for (int i = difficultyLevel + 1; i < factionSortedByDifficulty.Length; i++)
                {
                    GameFaction faction = factionSortedByDifficulty[i];

                    if ((remainingScrewByFaction[faction] - screwSlotAvailableByFaction[faction]) >= 3)
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

                    if ((remainingScrewByFaction[faction] - screwSlotAvailableByFaction[faction]) < 3)
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

        private float CalculateCurrentProgress()
        {
            int doneScrew = _screws.Count(s => s.IsDone);
            return (float)doneScrew / _totalScrew;
        }

        private int GetCurrentDifficultyPhase(float progress)
        {
            if (currentLevelDifficultyConfig == null) return 0;
            if (currentLevelDifficultyConfig.LevelPhases == null) return 0;


            for (int i = 0; i < currentLevelDifficultyConfig.LevelPhases.Length; i++)
            {
                if (progress <= currentLevelDifficultyConfig.LevelPhases[i].EndProgress)
                    return currentLevelDifficultyConfig.LevelPhases[i].PhaseIndex;
            }
            return currentLevelDifficultyConfig.LevelPhases.Last().PhaseIndex;
        }

        private int ApplyDifficultyModifiers(int currentDifficultyLevel, int modifier)
        {
            int adjustedLevel = currentDifficultyLevel + modifier;
            adjustedLevel = Mathf.Max(adjustedLevel, 0);

            // FREE LEVELS
            int minFreeLevel = RemoteConfigController.GetIntConfig(FirebaseConfig.MIN_FREE_LEVEL, 20);
            if (currentLevel.Value <= minFreeLevel)
                adjustedLevel = 0;

            // HARD MODE
            if (GamePersistentVariable.isHardMode)
                adjustedLevel = currentLevelDifficultyConfig.LevelPhases.Length - 1;

            // SPECIAL LEVEL
            if (GamePersistentVariable.currentLevelMode != LevelMode.Normal)
            {
                adjustedLevel = currentDifficultyLevel;
            }

            // DIFFICULTY REDUCED AFTER REWARDED ADS
            if (_isReducedDifficultyAfterRewardedAds && GamePersistentVariable.numScrewBoxUnlocked >= 1)
                adjustedLevel = 0;

            return adjustedLevel;
        }

        #endregion

        #region HANDLE SCREWS THAT ARE OUTSIDE WITH NO SCREW BOX
        private void OnScrewBoxSpawned(ScrewBox screwBox)
        {
            // DebugLogger.LogSimple();
        }

        private IEnumerator HandlingOutsideScrews()
        {
            WaitForSeconds waitForSeconds = new WaitForSeconds(1f);

            // wait for level loaded
            yield return new WaitForSeconds(3f);

            while (true)
            {
                if (!_isHandlingOutsideScrews)
                {
                    _isHandlingOutsideScrews = true;

                    yield return HandleOutsideScrews();
                }

                yield return waitForSeconds;
            }
        }

        private async Task HandleOutsideScrews()
        {
            if (_screws == null)
            {
                return;
            }

            bool isFound = false;

            for (int i = 0; i < _screws.Count; i++)
            {
                if (i % 16 == 0 && i > 0)
                {
                    await Task.Delay(100);
                }

                try
                {
                    BaseScrew screw = _screws[i];

                    if (screw.IsOutsideWithNoScrewBox && !screw.IsMoving)
                    {
                        GameFaction faction = screw.Faction;

                        ScrewBoxSlot screwBoxSlot = screwBoxManager.CheckAvailableScrewBoxes(faction, isIncludeScrewPorts: false);

                        if (screwBoxSlot != null)
                        {
                            screw.ScrewServiceLocator.screwMeshRenderer.enabled = true;

                            screw.IsInteractable = true;

                            screw.Loose(screw.ScrewId, faction, screwBoxSlot);

                            screw.IsOutsideWithNoScrewBox = false;

                            if (!isFound)
                            {
                                isFound = true;
                            }

                            GamePersistentVariable.numOutsideScrew--;
                        }
                    }
                }
                catch (System.Exception e)
                {

                }
            }

            if (isFound)
            {
                updateOutsideScrewNumberEvent?.Invoke();
            }

            // _remainingHandleOutsideScrewCommands--;
            _isHandlingOutsideScrews = false;
        }
        #endregion

        #region Very Important: PIECE - BLOCK PIECES Dictionary
        private void OnPieceCompleted(int instanceId)
        {
            // DebugLogger.LogSimple();
            obstaclesOfPieces.Remove(instanceId);
        }
        #endregion

        #region SPECIAL LEVEL
        private void SwitchScrewsToSpecialType<T>() where T : BaseScrew
        {
            for (int i = 0; i < _screws.Count; i++)
            {
                _screws[i] = CommonUtil.ReplaceAndCopy<BaseScrew, T>(_screws[i].gameObject);
            }

            switchScrewsToSpecialTypeCompleteEvent?.Invoke(_screws.ToArray());
        }

        private void SwitchToLockedScrews()
        {
            SwitchScrewsToSpecialType<LockedScrew>();
        }

        private void SwitchToRustyScrews()
        {
            SwitchScrewsToSpecialType<RustyScrew>();
        }
        #endregion

        #region Event Listeners

        private void OnRustyScrewManagerInit(RustyScrewSpecialLevelManager obj)
        {
            // DebugLogger.LogSimple(message:$"{obj}");
            _rustyScrewSpecialLevelManager = obj;
        }

        private void OnLockScrewColorManagerInit(LockScrewColorLevelManager obj)
        {
            // DebugLogger.LogSimple(message:$"{obj}");
            _lockScrewColorLevelManager = obj;
        }

        private void OnLockScrewBoxInit(LockScrewBoxManager obj)
        {
            // DebugLogger.LogSimple(message:$"{obj}");
            _lockScrewBoxManager = obj;
        }

        #endregion
    }
}
