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

    public static event Action spawnFreshLevelScrewBoxesEvent;
    public static event Action<int, int> updateRemainingScrewEvent;
    public static event Action updateOutsideScrewNumberEvent;

    private bool _isOldDataLoaded;


    void Awake()
    {
        ScrewManager.setScrewsEvent += OnAllLevelScrewsObservered;

        StartCoroutine(PeriodicallySaving());
    }

    void OnDestroy()
    {
        ScrewManager.setScrewsEvent -= OnAllLevelScrewsObservered;
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
            _isOldDataLoaded = true;

            return;
        }

        ScrewData[] screwsData = levelProgressData.ScrewsData;
        ScrewBoxData[] screwBoxesData = levelProgressData.ScrewBoxesData;

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

            if (!screwData.IsActive())
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

            // if (screwData.IsDone)
            // {
            //     ScrewBoxSlot screwBoxSlot = screwBoxManager.CheckAvailableScrewBoxes(screw.Faction, isIncludeScrewPorts: true);

            //     screw.Loose(screw.ScrewId, screw.Faction, screwBoxSlot);
            // }
        }

        // UPDATE UI
        updateRemainingScrewEvent?.Invoke(remainingScrew, totalScrew);
        updateOutsideScrewNumberEvent?.Invoke();

        _isOldDataLoaded = true;
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

        DataUtility.SaveAsync($"{GameConstants.SCREWS_DATA}_{currentLevel.Value}", levelProgressData);
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
            GamePersistentVariable.isPreventLoadLevelData = false;
            GamePersistentVariable.isFreshLevel = true;
        }
    }
}
