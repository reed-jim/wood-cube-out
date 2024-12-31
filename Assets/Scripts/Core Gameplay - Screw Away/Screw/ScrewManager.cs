using System;
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
    [SerializeField] private LevelDifficultyConfiguration levelDifficultyConfiguration;

    #region PRIVATE FIELD
    private List<BaseScrew> _screws;
    private int _totalScrewObserved;
    private int _totalScrew;
    #endregion

    #region EVENT
    public static event Action<GameFaction> spawnScrewBoxEvent;
    public static event Action spawnAdsScrewBoxesEvent;
    public static event Action winLevelEvent;
    public static event Action<BaseScrew[]> setScrewsEvent;
    #endregion

    #region LIFE CYCLE
    void Awake()
    {
        LevelLoader.startLevelEvent += OnLevelStart;
        LevelLoader.setLevelScrewNumberEvent += SetLevelScrewNumber;
        BaseScrew.addScrewToListEvent += AddScrew;
        ScrewBox.spawnNewScrewBoxEvent += SpawnNewScrewBox;
        ScrewBox.setFactionForScrewBoxEvent += AssignFactionForNewScrewBox;
        BaseScrew.screwLoosenedEvent += CheckWin;
        ScrewsDataManager.spawnFreshLevelScrewBoxesEvent += SpawnFirstScrewBoxes;
        MultiPhaseLevelManager.switchPhaseEvent += SpawnFirstScrewBoxesNewPhase;
    }

    void OnDestroy()
    {
        LevelLoader.startLevelEvent -= OnLevelStart;
        LevelLoader.setLevelScrewNumberEvent -= SetLevelScrewNumber;
        BaseScrew.addScrewToListEvent -= AddScrew;
        ScrewBox.spawnNewScrewBoxEvent -= SpawnNewScrewBox;
        ScrewBox.setFactionForScrewBoxEvent -= AssignFactionForNewScrewBox;
        BaseScrew.screwLoosenedEvent -= CheckWin;
        ScrewsDataManager.spawnFreshLevelScrewBoxesEvent -= SpawnFirstScrewBoxes;
        MultiPhaseLevelManager.switchPhaseEvent -= SpawnFirstScrewBoxesNewPhase;
    }
    #endregion

    private void OnLevelStart()
    {
        _screws = new List<BaseScrew>();

        _totalScrewObserved = 0;
    }

    #region SCREW
    private void SetLevelScrewNumber(int screwNumber)
    {
        _totalScrew = screwNumber;
    }

    private void AddScrew(BaseScrew screw)
    {
        _screws.Add(screw);

        _totalScrewObserved++;

        if (_totalScrewObserved == _totalScrew)
        {
            SpawnFirstScrewBoxes();

            // TO SAVE/LOAD LEVEL PROGRESS
            // setScrewsEvent?.Invoke(_screws.ToArray());
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

    private Dictionary<GameFaction, int> GetDifficultyLevelByFaction()
    {
        Dictionary<GameFaction, int> difficultyLevelByFactions = new Dictionary<GameFaction, int>();
        Dictionary<GameFaction, int> totalNotBlockedScrewByFaction = new Dictionary<GameFaction, int>();

        for (int i = 0; i < GameConstants.SCREW_FACTION.Length; i++)
        {
            difficultyLevelByFactions.Add(GameConstants.SCREW_FACTION[i], 0);
        }

        for (int i = 0; i < _screws.Count; i++)
        {
            if (_screws[i].IsDone || _screws[i].IsLocked)
            {
                continue;
            }

            GameFaction faction = _screws[i].Faction;

            int numberBlockObject = _screws[i].NumberBlockingObjects;

            // Compensate
            if (numberBlockObject == 0)
            {
                // numberBlockObject = -GameConstants.SCREW_FACTION.Length;
                // numberBlockObject = -1;

                if (totalNotBlockedScrewByFaction.ContainsKey(faction))
                {
                    totalNotBlockedScrewByFaction[faction]++;
                }
                else
                {
                    totalNotBlockedScrewByFaction.Add(faction, 1);
                }
            }

            difficultyLevelByFactions[faction] += numberBlockObject;
        }

        foreach (var faction in totalNotBlockedScrewByFaction.Keys)
        {
            difficultyLevelByFactions[faction] -= (totalNotBlockedScrewByFaction[faction] / GameConstants.NUMBER_SLOT_PER_SCREW_BOX) * GameConstants.SCREW_FACTION.Length;
        }

        return difficultyLevelByFactions;
    }

    private void CheckWin()
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

        if (remainingScrew == 0)
        {
            winLevelEvent?.Invoke();
        }
    }
    #endregion

    #region MANAGE SCREW BOX
    private async void SpawnFirstScrewBoxes()
    {
        GameFaction[] sortedFactionByBlockedObject = await GetSortedFactionsByBlockedObject();

        // Spawn 2 default screw boxes
        for (int i = 0; i < 2; i++)
        {
            GameFaction faction = sortedFactionByBlockedObject[i];

            spawnScrewBoxEvent?.Invoke(faction);
        }

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

    private async void SpawnNewScrewBox()
    {
        GameFaction faction = await GetFactionForNewScrewBox();

        if (faction == GameFaction.None)
        {
            return;
        }

        Spawn(faction);

        void Spawn(GameFaction faction)
        {
            spawnScrewBoxEvent?.Invoke(faction);
        }
    }

    private async void AssignFactionForNewScrewBox(ScrewBox screwBox)
    {
        GameFaction faction = await GetFactionForNewScrewBox();

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
            _screws[i].CountBlockingObjects();

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

    private async Task<GameFaction> GetFactionForNewScrewBox()
    {
        float progress;
        int doneScrew = 0;

        // int maxNumberBlockingObject = 0;

        for (int i = 0; i < _screws.Count; i++)
        {
            if (_screws[i].IsDone)
            {
                doneScrew++;
            }
            else
            {
                _screws[i].CountBlockingObjects();

                // if (_screws[i].NumberBlockingObjects > maxNumberBlockingObject)
                // {
                //     maxNumberBlockingObject = _screws[i].NumberBlockingObjects;
                // }
            }

            // await Task.Delay(9);
        }

        progress = (float)doneScrew / _totalScrew;

        int levelDifficulty = 0;

        for (int i = 0; i < levelDifficultyConfiguration.LevelPhases.Length; i++)
        {
            LevelPhase levelPhase = levelDifficultyConfiguration.LevelPhases[i];

            if (progress <= levelPhase.EndProgress)
            {
                levelDifficulty = i;

                break;
            }
        }

        Dictionary<GameFaction, int> screwPortAvailableByFaction = screwBoxManager.GetScrewPortAvailableByFaction();
        Dictionary<GameFaction, int> remainingScrewByFaction = GetRemainingScrewByFaction();
        Dictionary<GameFaction, int> totalBlockObjectsByFaction = GetDifficultyLevelByFaction();

        GameFaction[] factionSortedByDifficulty = totalBlockObjectsByFaction.OrderBy(item => item.Value).Select(item => item.Key).ToArray();

        GameFaction nextFaction = GameFaction.None;

        int difficultyLevel = factionSortedByDifficulty.Length;

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

        for (int i = 0; i < factionSortedByDifficulty.Length; i++)
        {
            GameFaction faction = factionSortedByDifficulty[i];
            int sortedFactionIndex = i;

            if ((remainingScrewByFaction[faction] - screwPortAvailableByFaction[faction]) < 3)
            {
                continue;
            }

            if (levelDifficulty == sortedFactionIndex)
            {
                nextFaction = faction;

                isFound = true;

                break;
            }
        }

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

        // CASE: there are a few screws left, but no screw box spawned and player can't win
        if (!isFound)
        {
            for (int i = 0; i < factionSortedByDifficulty.Length; i++)
            {
                GameFaction faction = factionSortedByDifficulty[i];

                if ((remainingScrewByFaction[faction] - screwPortAvailableByFaction[faction]) > 0 &&
                    (remainingScrewByFaction[faction] - screwPortAvailableByFaction[faction]) < 3)
                {
                    nextFaction = faction;

                    isFound = true;
                }
            }
        }

        return nextFaction;
    }
    #endregion
}
