using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using static GameEnum;

public class ScrewsDataManager : MonoBehaviour
{
    [SerializeField] private BaseScrew[] _screws;

    [SerializeField] private ScrewBoxManager screwBoxManager;

    [SerializeField] private IntVariable currentLevel;

    #region PRIVATE FIELD
    private string _currentSaveLevelDataKey;
    #endregion

    public static event Action spawnFreshLevelScrewBoxesEvent;
    public static event Action<int, int> updateRemainingScrewEvent;
    public static event Action updateOutsideScrewNumberEvent;

    #region TUTORIAL LEVEL 1
    public static event Action endTutorialLevel1Event;
    #endregion

    void Awake()
    {
        ScrewManager.setScrewsEvent += OnAllLevelScrewsObservered;
        GameStateWin.winLevelEvent += OnLevelWon;
        DebugPopup.prevLevelEvent += OnPrevLevel;
        DebugPopup.nextLevelEvent += OnNextLevel;
        DebugPopup.toLevelEvent += OnDebugToLevel;

        StartCoroutine(PeriodicallySaving());
    }

    void OnDestroy()
    {
        ScrewManager.setScrewsEvent -= OnAllLevelScrewsObservered;
        GameStateWin.winLevelEvent -= OnLevelWon;
        DebugPopup.prevLevelEvent -= OnPrevLevel;
        DebugPopup.nextLevelEvent -= OnNextLevel;
        DebugPopup.toLevelEvent -= OnDebugToLevel;
    }

    private IEnumerator PeriodicallySaving()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(2f);

        while (true)
        {
            yield return waitForSeconds;

            if (!GamePersistentVariable.isFreshLevel)
            {
                SaveLevelData();
            }
        }
    }

    private async void LoadLevelData()
    {
        LevelProgressData levelProgressData = DataUtility.Load($"{GameConstants.SCREWS_DATA}_{currentLevel.Value}", new LevelProgressData());

        GamePersistentVariable.isFreshLevel = levelProgressData.ScrewsData == null;

        // No data for current level
        if (levelProgressData.ScrewsData == null)
        {
            spawnFreshLevelScrewBoxesEvent?.Invoke();

            return;
        }

        ScrewData[] screwsData = levelProgressData.ScrewsData;
        ScrewBoxData[] screwBoxesData = levelProgressData.ScrewBoxesData;

        if (screwsData.Length != _screws.Length)
        {
            GamePersistentVariable.isFreshLevel = true;

            spawnFreshLevelScrewBoxesEvent?.Invoke();

            return;
        }

        if (currentLevel.Value == 1)
        {
            endTutorialLevel1Event?.Invoke();
        }

        for (int i = 0; i < screwBoxesData.Length; i++)
        {
            screwBoxManager.SpawnScrewBox(screwBoxesData[i].Faction, screwBoxesData[i].IsLocked);
        }

        await Task.Delay(1000);

        Dictionary<BaseScrew, ScrewData> screwsWithData = new Dictionary<BaseScrew, ScrewData>();

        for (int i = 0; i < _screws.Length; i++)
        {
            screwsWithData.Add(_screws[i], screwsData[i]);
        }

        int totalScrew = _screws.Length;
        int remainingScrew = totalScrew;

        foreach (var screw in screwsWithData.Keys)
        {
            ScrewData screwData = screwsWithData[screw];

            if (screwData == null)
            {
                continue;
            }

            screw.Faction = screwData.Faction;
            screw.ScrewServiceLocator.screwFaction.SetColorByFaction();

            screw.IsMoving = screwData.IsMoving();
            screw.IsDone = screwData.IsDone();
            screw.IsInScrewPort = screwData.IsInScrewPort();
            screw.IsOutsideWithNoScrewBox = screwData.IsOutside();

            if (!screwData.IsActive() && !screwData.IsOutside())
            {
                screw.InvokeBreakJointEvent();

                screw.gameObject.SetActive(false);

                screw.IsDone = true;

                remainingScrew--;

                continue;
            }

            if (screwData.IsDone())
            {
                ScrewBoxSlot screwBoxSlot = ScrewBoxManager.Instance.CheckAvailableScrewBoxes(screw.Faction);

                if (screwBoxSlot != null)
                {
                    screw.LooseImmediately(screwBoxSlot);
                }

                remainingScrew--;
            }

            if (screwData.IsInScrewPort())
            {
                ScrewBoxSlot screwBoxSlot = screwBoxManager.CheckAvailableScrewBoxes(screw.Faction, isIncludeScrewPorts: true);

                screw.LooseImmediately(screwBoxSlot);
            }

            if (screwData.IsOutside())
            {
                screw.ForceUnscrew();
            }
        }

        // UPDATE UI
        updateRemainingScrewEvent?.Invoke(remainingScrew, totalScrew);
        updateOutsideScrewNumberEvent?.Invoke();
    }

    private ScrewBoxData[] GetScrewBoxesData()
    {
        ScrewBoxData[] screwBoxesData = new ScrewBoxData[screwBoxManager.ScrewBoxs.Length];

        for (int i = 0; i < screwBoxManager.ScrewBoxs.Length; i++)
        {
            screwBoxesData[i] = new ScrewBoxData();

            if (screwBoxManager.ScrewBoxs[i] == null)
            {
                screwBoxesData[i].IsLocked = true;

                continue;
            }

            screwBoxesData[i].Faction = screwBoxManager.ScrewBoxs[i].Faction;
            screwBoxesData[i].IsLocked = screwBoxManager.ScrewBoxs[i].IsLocked;
        }

        return screwBoxesData;
    }

    private void SaveLevelData()
    {
        ScrewData[] screwsData = new ScrewData[_screws.Length];

        // foreach (var id in _screwsWithId.Keys)
        // {
        //     ScrewData screwData = new ScrewData();

        //     screwData.ScrewId = _screwsWithId[id].ScrewId;
        //     screwData.Faction = _screwsWithId[id].Faction;
        //     screwData.IsDone = _screwsWithId[id].IsDone;
        //     screwData.IsInScrewPort = _screwsWithId[id].IsInScrewPort;

        //     if (!_screwsWithId[id].gameObject.activeSelf)
        //     {
        //         screwData.IsDestroyed = true;
        //     }

        //     screwsData.Add(id, screwData);
        // }

        for (int i = 0; i < _screws.Length; i++)
        {
            if (_screws[i] == null)
            {
                break;
            }

            screwsData[i] = new ScrewData();

            screwsData[i].ScrewId = _screws[i].ScrewId;
            screwsData[i].Faction = _screws[i].Faction;

            screwsData[i].SetState(
                _screws[i].IsDone, _screws[i].IsMoving, _screws[i].IsOutsideWithNoScrewBox, _screws[i].IsInScrewPort, _screws[i].gameObject.activeInHierarchy);
        }

        ScrewBoxData[] screwBoxesData = GetScrewBoxesData();

        LevelProgressData levelProgressData = new LevelProgressData();

        levelProgressData.ScrewsData = screwsData;
        levelProgressData.ScrewBoxesData = screwBoxesData;

        string key = $"{GameConstants.SCREWS_DATA}_{currentLevel.Value}";

        DataUtility.SaveAsync(key, levelProgressData);

        _currentSaveLevelDataKey = key;
    }

    private void OnAllLevelScrewsObservered(BaseScrew[] screws)
    {
        _screws = screws;

        System.Array.Sort(_screws, (go1, go2) => go2.GetInstanceID().CompareTo(go1.GetInstanceID()));

        if (!GamePersistentVariable.isPreventLoadLevelData)
        {
            LoadLevelData();
        }
        else
        {
            SetFreshLevel(_currentSaveLevelDataKey);

            // GamePersistentVariable.isPreventLoadLevelData = false;
            // GamePersistentVariable.isFreshLevel = true;

            // DataUtility.RemoveKey($"{GameConstants.SCREWS_DATA}_{currentLevel.Value}");
        }
    }

    private void OnLevelWon()
    {
        GamePersistentVariable.isPreventLoadLevelData = false;
        GamePersistentVariable.isFreshLevel = true;

        SetFreshLevel(_currentSaveLevelDataKey);
    }

    #region UTIL
    private void SetFreshLevel(string levelSaveKey)
    {
        GamePersistentVariable.isPreventLoadLevelData = false;
        GamePersistentVariable.isFreshLevel = true;

        string validLevelKey = levelSaveKey;

        if (string.IsNullOrEmpty(validLevelKey))
        {
            validLevelKey = $"{GameConstants.SCREWS_DATA}_{currentLevel.Value}";
        }

        DataUtility.RemoveKey(validLevelKey);
    }
    #endregion

    #region DEBUG
    private void OnPrevLevel()
    {
        string key = $"{GameConstants.SCREWS_DATA}_{currentLevel.Value - 1}";

        SetFreshLevel(key);
    }

    private void OnNextLevel()
    {
        string key = $"{GameConstants.SCREWS_DATA}_{currentLevel.Value + 1}";

        SetFreshLevel(key);
    }

    private void OnDebugToLevel(int level)
    {
        string key = $"{GameConstants.SCREWS_DATA}_{level}";

        SetFreshLevel(key);
    }
    #endregion
}
