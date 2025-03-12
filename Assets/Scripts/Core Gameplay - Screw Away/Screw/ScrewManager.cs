using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using static GameEnum;

public class ScrewManager : MonoBehaviour
{
    [SerializeField] private ScrewBoxManager screwBoxManager;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private LevelDataContainer levelDataContainer;
    [SerializeField] private LevelObserver levelObserver;

    [Header("CUSTOMIZE")]
    [SerializeField] private float delayShowPopupWin;

    #region PRIVATE FIELD
    private List<BaseScrew> _screws;
    private int _totalScrewObserved;
    private int _totalScrew;
    [SerializeField] private LevelDifficultyConfiguration _currentLevelDifficultyConfiguration;
    #endregion

    #region QUEUE SPAWN SCREW BOXES
    private int _remainingSpawnScrewBoxCommands;
    private bool _isHandlingSpawningScrewBox;
    #endregion

    #region QUEUE OUTSIDE SCREWS
    private int _totalScrewBoxes;
    private int _totalScrewBoxesSpawned;
    private int _remainingHandleOutsideScrewCommands;
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
    #endregion

    #region LIFE CYCLE
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

        // StartCoroutine(QueueSpawningScrewBoxes());
        StartCoroutine(HandlingOutsideScrews());

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
    }
    #endregion

    private void OnLevelStart()
    {
        _screws = new List<BaseScrew>();

        _totalScrewObserved = 0;

        // _currentLevelDifficultyConfiguration = levelDataContainer.GetLevelDifficultyConfiguration(currentLevel.Value);
        _currentLevelDifficultyConfiguration = LevelDifficultyCalculatorUltility.GenerateScrewBoxDifficultyTemplate(currentLevel.Value);

        _currentLevelDifficultyConfiguration.ScrewPortFactionDifficultyModifier =
            RemoteConfigController.GetIntConfig(FirebaseConfig.SCREW_PORT_FACTION_DIFFICULTY_MODIFIER, 0);

        _totalScrewBoxesSpawned = 0;
        _remainingSpawnScrewBoxCommands = 0;
        _isHandlingSpawningScrewBox = false;
        _remainingHandleOutsideScrewCommands = 0;
        _isHandlingOutsideScrews = false;

        GamePersistentVariable.numScrewBoxUnlocked = 0;
    }

    #region SCREW
    private void SetLevelScrewNumber(int screwNumber)
    {
        _totalScrew = screwNumber;
        _totalScrewBoxes = _totalScrew / GameConstants.NUMBER_SLOT_PER_SCREW_BOX;

        updateRemainingScrewEvent?.Invoke(_totalScrew, _totalScrew);
    }

    private void AddScrew(BaseScrew screw)
    {
        if (_screws == null)
        {
            _screws = new List<BaseScrew>();
        }

        _screws.Add(screw);

        _totalScrewObserved++;

        if (_totalScrewObserved == _totalScrew)
        {
            SpawnFirstScrewBoxes();

            // TO SAVE/LOAD LEVEL PROGRESS
            // setScrewsEvent?.Invoke(_screws.ToArray());

            levelSetupCompletedEvent?.Invoke();
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
            if (_screws[i].IsDone || _screws[i].IsLocked)
            {
                continue;
            }

            remainingScrewByFaction[_screws[i].Faction]++;
        }

        // CHECK SCREW PORTS
        for (int i = 0; i < screwBoxManager.ScrewPorts.Count; i++)
        {
            ScrewBoxSlot screwBoxSlot = screwBoxManager.ScrewPorts[i];

            if (screwBoxSlot.IsFilled)
            {
                remainingScrewByFaction[screwBoxSlot.Screw.Faction]++;
            }
        }

        // int remainingScrew = remainingScrewByFaction.Sum(item => item.Value);

        return remainingScrewByFaction;
    }

    private Dictionary<GameFaction, int> GetDifficultyLevelByFaction(int? levelPhase = null)
    {
        // RULE: The more difficulty point, the less chance to spawn for a faction

        Dictionary<GameFaction, int> difficultyLevelByFactions = new Dictionary<GameFaction, int>();
        Dictionary<GameFaction, int> totalNotBlockedScrewByFaction = new Dictionary<GameFaction, int>();

        Dictionary<GameFaction, List<int>> numObstacleEachScrewEachFaction = new Dictionary<GameFaction, List<int>>();

        for (int i = 0; i < GameConstants.SCREW_FACTION.Length; i++)
        {
            difficultyLevelByFactions.Add(GameConstants.SCREW_FACTION[i], 0);

            numObstacleEachScrewEachFaction.Add(GameConstants.SCREW_FACTION[i], new List<int>());
        }

        // Find the number of obstacles for each screw in each faction.
        for (int i = 0; i < _screws.Count; i++)
        {
            if (_screws[i].IsInScrewPort)
            {

            }
            else
            {
                if (_screws[i].IsDone || _screws[i].IsLocked)
                {
                    continue;
                }
            }

            GameFaction faction = _screws[i].Faction;

            int numberBlockObject = _screws[i].NumberBlockingObjects;

            numObstacleEachScrewEachFaction[faction].Add(numberBlockObject);

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

            numObstacleEachScrewEachFaction[faction] = numObstacleEachScrewEachFaction[faction].OrderBy(item => item).ToList();

            int totalObstaclesOfThreeMostOpenedScrews = numObstacleEachScrewEachFaction[faction].Take(3).Sum();

            difficultyLevelByFactions[faction] += totalObstaclesOfThreeMostOpenedScrews;
        }

        // DECREASE Chance of factions that are already available in screw boxes
        for (int i = 0; i < screwBoxManager.ScrewBoxs.Length; i++)
        {
            if (screwBoxManager.ScrewBoxs[i] == null || screwBoxManager.ScrewBoxs[i].IsLocked)
            {
                continue;
            }

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

        // Modify chance of factions of screw port
        for (int i = 0; i < screwBoxManager.ScrewPorts.Count; i++)
        {
            if (screwBoxManager.ScrewPorts[i].IsFilled)
            {
                // override old value
                difficultyLevelByFactions[screwBoxManager.ScrewPorts[i].Screw.Faction] =
                    _currentLevelDifficultyConfiguration.ScrewPortFactionDifficultyModifier;
            }
        }

        return difficultyLevelByFactions;
    }

    private void OnScrewLoosed()
    {
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
            if (_screws[i].IsDone && !_screws[i].IsInScrewPort)
            {
                continue;
            }

            remainingScrewByFaction[_screws[i].Faction]++;
        }

        int remainingScrew = remainingScrewByFaction.Sum(item => item.Value);

        updateRemainingScrewEvent?.Invoke(remainingScrew, _totalScrew);

        if (remainingScrew == 0)
        {
            enableBoosterEvent?.Invoke(false);

            await Task.Delay((int)(delayShowPopupWin * 1000));

            winLevelEvent?.Invoke();
        }

        levelObserver.Progress = 1 - (float)remainingScrew / _totalScrew;
    }
    #endregion

    #region MANAGE SCREW BOX
    private async void SpawnFirstScrewBoxes()
    {
        GameFaction[] sortedFactionByBlockedObject = await GetSortedFactionsByBlockedObject();

        // Spawn 2 default screw boxes
        for (int i = 0; i < GameConstants.DEFAULT_INITIAL_NUMBER_SCREW_BOXES; i++)
        {
            GameFaction faction = sortedFactionByBlockedObject[i];

            spawnScrewBoxEvent?.Invoke(faction);
        }

        _totalScrewBoxesSpawned += GameConstants.DEFAULT_INITIAL_NUMBER_SCREW_BOXES;

        spawnAdsScrewBoxesEvent?.Invoke();
    }

    private async void SpawnFirstScrewBoxesNewPhase(int phase)
    {
        await Task.Delay(2000);

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

            spawnScrewBoxEvent?.Invoke(faction);
        }
    }

    private void SpawnNewScrewBox()
    {
        if (_totalScrewBoxesSpawned >= _totalScrewBoxes)
        {
            return;
        }

        GameFaction faction = GetFactionForNewScrewBox();

        if (faction == GameFaction.None)
        {
            return;
        }

        spawnScrewBoxEvent?.Invoke(faction);

        _totalScrewBoxesSpawned++;
        // _remainingSpawnScrewBoxCommands--;

        // _isHandlingSpawningScrewBox = false;
    }

    private void OnScrewBoxUnlocked(ScrewBox screwBox)
    {
        _totalScrewBoxesSpawned++;
    }

    private void OnSpawnNewScrewBoxRequested()
    {
        _remainingSpawnScrewBoxCommands++;
    }

    private IEnumerator QueueSpawningScrewBoxes()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.3f);

        while (true)
        {
            if (_remainingSpawnScrewBoxCommands > 0 && _totalScrewBoxesSpawned <= _totalScrewBoxes)
            {
                if (!_isHandlingSpawningScrewBox)
                {
                    _isHandlingSpawningScrewBox = true;

                    SpawnNewScrewBox();
                }
            }

            yield return waitForSeconds;
        }
    }

    private void AssignFactionForNewAdsScrewBox(ScrewBox screwBox)
    {
        AssignFactionForNewScrewBox(screwBox, _currentLevelDifficultyConfiguration.AdsScrewBoxDifficultyLevelModifier);
    }

    private void AssignFactionForNewScrewBox(ScrewBox screwBox, int difficultyLevelModifier)
    {
        GameFaction faction = GetFactionForNewScrewBox(difficultyLevelModifier);

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

        Dictionary<GameFaction, int> screwPortAvailableByFaction = screwBoxManager.GetScrewPortAvailableByFaction();
        Dictionary<GameFaction, int> remainingScrewByFaction = GetRemainingScrewByFaction();
        Dictionary<GameFaction, int> difficultyLevelByFaction = GetDifficultyLevelByFaction();

        Dictionary<GameFaction, int> shuffledDifficultyLevelByFaction = ShuffleUtil.ShuffleItemsWithSameValue(difficultyLevelByFaction);

        GameFaction[] factionSortedByDifficulty = shuffledDifficultyLevelByFaction.OrderBy(item => item.Value).Select(item => item.Key).ToArray();

        return factionSortedByDifficulty;
    }

    private GameFaction GetFactionForNewScrewBox(int difficultyLevelModifier = 0)
    {
        float progress;
        int doneScrew = 0;

        for (int i = 0; i < _screws.Count; i++)
        {
            if (_screws[i].IsDone)
            {
                doneScrew++;
            }
            else
            {
                _screws[i].CountBlockingObjects(isIncludeHiddenScrew: true);
            }
        }

        progress = (float)doneScrew / _totalScrew;

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
            if (GamePersistentVariable.numScrewBoxUnlocked >= 1)
            {
                difficultyLevel = 0;
            }
        }

        // // SIMPLE ALGORITHM FOR DIFFICULTY BASED ON PROGRESS
        // if (progress < 0.86f)
        // {
        //     difficultyLevel = 0;
        // }
        // else
        // {
        //     difficultyLevel = GameConstants.SCREW_FACTION.Length - 1;
        // }

        Dictionary<GameFaction, int> screwPortAvailableByFaction = screwBoxManager.GetScrewPortAvailableByFaction();
        Dictionary<GameFaction, int> remainingScrewByFaction = GetRemainingScrewByFaction();
        Dictionary<GameFaction, int> totalBlockObjectsByFaction = GetDifficultyLevelByFaction(difficultyLevel);

        GameFaction[] factionSortedByDifficulty = totalBlockObjectsByFaction.OrderBy(item => item.Value).Select(item => item.Key).ToArray();

        GameFaction nextFaction = GameFaction.None;

        bool isFound = false;

        // Debug.Log("SAFERIO ---------------------------------------");

        // for (int i = 0; i < factionSortedByDifficulty.Length; i++)
        // {
        //     GameFaction faction = factionSortedByDifficulty[i];

        //     string debugString = $"SAFERIO {faction} Remaining: {remainingScrewByFaction[faction]}";

        //     debugString += $" Available: {screwPortAvailableByFaction[faction]}";
        //     debugString += $" <color=#FF6060>Difficulty: {totalBlockObjectsByFaction[faction]}</color>";

        //     Debug.Log(debugString);
        // }

        for (int i = difficultyLevel; i >= 0; i--)
        {
            GameFaction faction = factionSortedByDifficulty[i];

            if ((remainingScrewByFaction[faction] - screwPortAvailableByFaction[faction]) >= 3)
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

                if ((remainingScrewByFaction[faction] - screwPortAvailableByFaction[faction]) >= 3)
                {
                    nextFaction = faction;

                    isFound = true;

                    break;
                }
            }
        }

        // int currentSortedFactionIndex = 0;

        // for (int i = 0; i < factionSortedByDifficulty.Length; i++)
        // {
        // GameFaction faction = factionSortedByDifficulty[i];

        // if ((remainingScrewByFaction[faction] - screwPortAvailableByFaction[faction]) < 3)
        // {
        //     continue;
        // }

        // if (difficultyLevel == currentSortedFactionIndex)
        // {
        //     nextFaction = faction;

        //     isFound = true;

        //     break;
        // }

        //     currentSortedFactionIndex++;
        // }

        if (!isFound)
        {
            for (int i = 0; i < factionSortedByDifficulty.Length; i++)
            {
                GameFaction faction = factionSortedByDifficulty[i];

                if ((remainingScrewByFaction[faction] - screwPortAvailableByFaction[faction]) < 3)
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
    private void OnScrewBoxSpawned(ScrewBox screwBox)
    {
        // for (int i = 0; i < _screws.Count; i++)
        // {
        //     BaseScrew screw = _screws[i];

        //     if (screw.IsOutsideWithNoScrewBox)
        //     {
        //         GameFaction faction = screw.Faction;

        //         ScrewBoxSlot screwBoxSlot = screwBoxManager.CheckAvailableScrewBoxes(faction, isIncludeScrewPorts: false);

        //         if (screwBoxSlot != null)
        //         {
        //             screwBoxSlot.Fill(screw);

        //             screw.ScrewServiceLocator.screwMeshRenderer.enabled = true;

        //             screw.IsInteractable = true;

        //             screw.Loose(screw.ScrewId, faction, screwBoxSlot);

        //             screw.IsOutsideWithNoScrewBox = false;
        //         }
        //     }
        // }

        _remainingHandleOutsideScrewCommands++;
    }

    private IEnumerator HandlingOutsideScrews()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);

        while (true)
        {
            bool isAnyScrewOutside = false;

            if (_screws != null)
            {
                for (int i = 0; i < _screws.Count; i++)
                {
                    if (_screws[i].IsOutsideWithNoScrewBox)
                    {
                        isAnyScrewOutside = true;

                        break;
                    }
                }
            }

            if (isAnyScrewOutside)
            {
                HandleOutsideScrews();
            }

            yield return waitForSeconds;
        }
    }

    private void HandleOutsideScrews()
    {
        for (int i = 0; i < _screws.Count; i++)
        {
            BaseScrew screw = _screws[i];

            if (screw.IsOutsideWithNoScrewBox && !screw.IsMoving)
            {
                GameFaction faction = screw.Faction;

                ScrewBoxSlot screwBoxSlot = screwBoxManager.CheckAvailableScrewBoxes(faction, isIncludeScrewPorts: false);

                if (screwBoxSlot != null)
                {
                    screwBoxSlot.Fill(screw);

                    screw.ScrewServiceLocator.screwMeshRenderer.enabled = true;

                    screw.IsInteractable = true;

                    screw.Loose(screw.ScrewId, faction, screwBoxSlot);

                    screw.IsOutsideWithNoScrewBox = false;
                }
            }
        }

        _remainingHandleOutsideScrewCommands--;
        _isHandlingOutsideScrews = false;
    }
    #endregion
}
