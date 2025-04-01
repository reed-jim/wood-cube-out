using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BirdSort2;
using UnityEditor;
using UnityEngine;
using static GameEnum;

public class ScrewManager : MonoBehaviour
{
    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private LevelDataContainer levelDataContainer;
    [SerializeField] private LevelObserver levelObserver;

    [Header("CUSTOMIZE")]
    [SerializeField] private float delayShowPopupWin;

    #region PRIVATE FIELD
    [SerializeField]public List<BaseScrew> _screws;
    [SerializeField]private int _totalScrewObserved;
    [SerializeField]private int _totalScrew;
    [SerializeField] private LevelDifficultyConfiguration _currentLevelDifficultyConfiguration;
    public bool _inited;
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
    public int numOutsideScrew;
    public RaycastHit lastScrewSelectedSurfaceHit;
    #endregion

    #region EVENT
    public static event Action winLevelEvent;
    public static event Action<bool> enableBoosterEvent;
    #endregion

    #region LIFE CYCLE
    void Awake()
    {
        MultiPhaseLevelManager.switchPhaseEvent += SpawnFirstScrewBoxesNewPhase;
        // StartCoroutine(QueueSpawningScrewBoxes());
    }

    void OnDestroy()
    {
        MultiPhaseLevelManager.switchPhaseEvent -= SpawnFirstScrewBoxesNewPhase;
    }
    #endregion

    public void Init()
    {
        _screws = new List<BaseScrew>();

        _totalScrewObserved = 0;
        
        _currentLevelDifficultyConfiguration = LevelDifficultyCalculatorUltility.GenerateScrewBoxDifficultyTemplate(UseProfile.CurrentLevel);

        _totalScrewBoxesSpawned = 0;
        _remainingSpawnScrewBoxCommands = 0;
        _isHandlingSpawningScrewBox = false;
        _remainingHandleOutsideScrewCommands = 0;
        _isHandlingOutsideScrews = false;
        _inited = true;
        StartCoroutine(HandlingOutsideScrews());
    }

    #region SCREW
    public void SetLevelScrewNumber(int screwNumber)
    {
        _totalScrew = screwNumber;
        _totalScrewBoxes = _totalScrew / GameConstants.NUMBER_SLOT_PER_SCREW_BOX;

       GamePlayController_BirdSort.Instance.gameScene.UpdateRemainingScrew(_totalScrew, _totalScrew);
    }

    public void AddScrew(BaseScrew screw)
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
            foreach (var i in GamePlayController_BirdSort.Instance.level.pieces)
            {
                i.ScaleBeforeLevel();
            }
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
        for (int i = 0; i < GamePlayController_BirdSort.Instance.level.screwBoxManager.ScrewPorts.Count; i++)
        {
            ScrewBoxSlot screwBoxSlot = GamePlayController_BirdSort.Instance.level.screwBoxManager.ScrewPorts[i];

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
            if (_screws[i].IsDone || _screws[i].IsLocked)
            {
                continue;
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
                difficultyLevelByFactions[faction] += 999;

                continue;
            }

            numObstacleEachScrewEachFaction[faction] = numObstacleEachScrewEachFaction[faction].OrderBy(item => item).ToList();

            int totalObstaclesOfThreeMostOpenedScrews = numObstacleEachScrewEachFaction[faction].Take(3).Sum();

            difficultyLevelByFactions[faction] += totalObstaclesOfThreeMostOpenedScrews;
        }

        // for (int i = 0; i < _screws.Count; i++)
        // {
        //     if (_screws[i].IsDone || _screws[i].IsLocked)
        //     {
        //         continue;
        //     }

        //     GameFaction faction = _screws[i].Faction;

        //     int numberBlockObject = _screws[i].NumberBlockingObjects;

        //     if (numberBlockObject == 0)
        //     {
        //         if (totalNotBlockedScrewByFaction.ContainsKey(faction))
        //         {
        //             totalNotBlockedScrewByFaction[faction]++;
        //         }
        //         else
        //         {
        //             totalNotBlockedScrewByFaction.Add(faction, 1);
        //         }
        //     }

        //     difficultyLevelByFactions[faction] += numberBlockObject;

        //     // FOR Tutorial
        //     difficultyLevelByFactions[faction] += _screws[i].ScrewDifficultyModifier;
        // }

        // // TRIPLE SAME FACTION + NO BLOCKED
        // int totalTripleSameFactionNoBlocked = 0;

        // foreach (var faction in totalNotBlockedScrewByFaction.Keys)
        // {
        //     // Debug.Log(faction + "/" + 3 * (totalNotBlockedScrewByFaction[faction] / GameConstants.NUMBER_SLOT_PER_SCREW_BOX) * GameConstants.SCREW_FACTION.Length);
        //     int numTriple = totalNotBlockedScrewByFaction[faction] / GameConstants.NUMBER_SLOT_PER_SCREW_BOX;

        //     difficultyLevelByFactions[faction] -= 3 * numTriple * GameConstants.SCREW_FACTION.Length;

        //     if (numTriple > 0)
        //     {
        //         totalTripleSameFactionNoBlocked++;
        //     }
        // }

        // if (totalTripleSameFactionNoBlocked == 0)
        // {
        //     foreach (var faction in totalNotBlockedScrewByFaction.Keys)
        //     {
        //         difficultyLevelByFactions[faction] -= GameConstants.SCREW_FACTION.Length;
        //     }
        // }

        // DECREASE Chance of factions that are already available in screw boxes
        for (int i = 0; i < GamePlayController_BirdSort.Instance.level.screwBoxManager.ScrewBoxs.Length; i++)
        {
            if (GamePlayController_BirdSort.Instance.level.screwBoxManager.ScrewBoxs[i] == null || GamePlayController_BirdSort.Instance.level.screwBoxManager.ScrewBoxs[i].IsLocked)
            {
                continue;
            }

            // avoid get factions that are already available in screw boxes when difficulty is max
            if (levelPhase != null)
            {
                if (levelPhase == GameConstants.SCREW_FACTION.Length - 1)
                {
                    difficultyLevelByFactions[GamePlayController_BirdSort.Instance.level.screwBoxManager.ScrewBoxs[i].Faction] -= 999;
                }
                else
                {
                    difficultyLevelByFactions[GamePlayController_BirdSort.Instance.level.screwBoxManager.ScrewBoxs[i].Faction] += 999;
                }
            }
            else
            {
                difficultyLevelByFactions[GamePlayController_BirdSort.Instance.level.screwBoxManager.ScrewBoxs[i].Faction] += 999;
            }
        }

        // Modify chance of factions of screw port
        for (int i = 0; i < GamePlayController_BirdSort.Instance.level.screwBoxManager.ScrewPorts.Count; i++)
        {
            if (GamePlayController_BirdSort.Instance.level.screwBoxManager.ScrewPorts[i].IsFilled)
            {
                difficultyLevelByFactions[GamePlayController_BirdSort.Instance.level.screwBoxManager.ScrewPorts[i].Screw.Faction] += _currentLevelDifficultyConfiguration.ScrewPortFactionDifficultyModifier;
            }
        }

        return difficultyLevelByFactions;
    }

    public void OnScrewLoosed()
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

        GamePlayController_BirdSort.Instance.gameScene.UpdateRemainingScrew(remainingScrew, _totalScrew);

        if (remainingScrew == 0)
        {
            enableBoosterEvent?.Invoke(false);

            await Task.Delay((int)(delayShowPopupWin * 1000));

            GamePlayController_BirdSort.Instance.level.Win();
        }

        levelObserver.Progress = 1 - (float)remainingScrew / _totalScrew;
    }
    #endregion

    #region MANAGE SCREW BOX
    public async void SpawnFirstScrewBoxes()
    {
        GameFaction[] sortedFactionByBlockedObject = await GetSortedFactionsByBlockedObject();

        // Spawn 2 default screw boxes
        for (int i = 0; i < GameConstants.DEFAULT_INITIAL_NUMBER_SCREW_BOXES; i++)
        {
            GameFaction faction = sortedFactionByBlockedObject[i];

            GamePlayController_BirdSort.Instance.level.screwBoxManager.SpawnScrewBox(faction);
        }

        _totalScrewBoxesSpawned += GameConstants.DEFAULT_INITIAL_NUMBER_SCREW_BOXES;

        GamePlayController_BirdSort.Instance.level.screwBoxManager.SpawnAdsScrewBoxes();
    }

    public async void SpawnFirstScrewBoxesNewPhase(int phase)
    {
        await Task.Delay(2000);

        GameFaction[] sortedFactionByBlockedObject = await GetSortedFactionsByBlockedObject();

        int numberScrewBoxToSpawn = 0;

        for (int i = 0; i < GamePlayController_BirdSort.Instance.level.screwBoxManager.ScrewBoxs.Length; i++)
        {
            if (GamePlayController_BirdSort.Instance.level.screwBoxManager.ScrewBoxs[i] == null)
            {
                numberScrewBoxToSpawn++;
            }
        }

        for (int i = 0; i < numberScrewBoxToSpawn; i++)
        {
            GameFaction faction = sortedFactionByBlockedObject[i];

            GamePlayController_BirdSort.Instance.level.screwBoxManager.SpawnScrewBox(faction);
        }
    }

    public void SpawnNewScrewBox()
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

        GamePlayController_BirdSort.Instance.level.screwBoxManager.SpawnScrewBox(faction);

        _totalScrewBoxesSpawned++;
        // _remainingSpawnScrewBoxCommands--;

        // _isHandlingSpawningScrewBox = false;
    }

    public void OnScrewBoxUnlocked(ScrewBox screwBox)
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

    public void AssignFactionForNewAdsScrewBox(ScrewBox screwBox)
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

        Dictionary<GameFaction, int> screwPortAvailableByFaction = GamePlayController_BirdSort.Instance.level.screwBoxManager.GetScrewPortAvailableByFaction();
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
                _screws[i].CountBlockingObjects(isIncludeHiddenScrew: false);
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

        // // SIMPLE ALGORITHM FOR DIFFICULTY BASED ON PROGRESS
        // if (progress < 0.86f)
        // {
        //     difficultyLevel = 0;
        // }
        // else
        // {
        //     difficultyLevel = GameConstants.SCREW_FACTION.Length - 1;
        // }

        Dictionary<GameFaction, int> screwPortAvailableByFaction = GamePlayController_BirdSort.Instance.level.screwBoxManager.GetScrewPortAvailableByFaction();
        Dictionary<GameFaction, int> remainingScrewByFaction = GetRemainingScrewByFaction();
        Dictionary<GameFaction, int> totalBlockObjectsByFaction = GetDifficultyLevelByFaction(difficultyLevel);

        GameFaction[] factionSortedByDifficulty = totalBlockObjectsByFaction.OrderBy(item => item.Value).Select(item => item.Key).ToArray();

        GameFaction nextFaction = GameFaction.None;

        bool isFound = false;

        // Debug.Log("---------------------------------------");

        // for (int i = 0; i < factionSortedByDifficulty.Length; i++)
        // {
        //     GameFaction faction = factionSortedByDifficulty[i];

        //     string debugString = $"{faction} Remaining: {remainingScrewByFaction[faction]}";

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
    #region HANDLE SCREWS THAT ARE OUTSIDE WITH NO SCREW BOX
    public void OnScrewBoxSpawned(ScrewBox screwBox)
    {
        _remainingHandleOutsideScrewCommands++;
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

                    ScrewBoxSlot screwBoxSlot = GamePlayController_BirdSort.Instance.level.screwBoxManager.CheckAvailableScrewBoxes(faction, isIncludeScrewPorts: false);

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

                        numOutsideScrew--;
                    }
                }
            }
            catch (System.Exception e)
            {

            }
        }

        if (isFound)
        {
            GamePlayController_BirdSort.Instance.gameScene.boxForOutsideScrew.UpdateNumber();
        }

        // _remainingHandleOutsideScrewCommands--;
        _isHandlingOutsideScrews = false;
    }
    #endregion
    #endregion
}
