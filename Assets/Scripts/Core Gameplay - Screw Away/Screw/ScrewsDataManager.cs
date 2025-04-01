using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BirdSort2;
using UnityEngine;
using static GameEnum;

public class ScrewsDataManager : MonoBehaviour
{
    [SerializeField] private BaseScrew[] _screws;
    
    void Awake()
    {

        // StartCoroutine(PeriodicallySaving());
    }

    void OnDestroy()
    {
    }

    private IEnumerator PeriodicallySaving()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(3f);

        while (true)
        {
            yield return waitForSeconds;

            SaveLevelData();
        }
    }

    private async void LoadLevelData()
    {
        LevelProgressData levelProgressData = DataUtility.Load<LevelProgressData>($"{GameConstants.SCREWS_DATA}_{UseProfile.CurrentLevel}", new LevelProgressData());

        // No data for current level
        if (levelProgressData.ScrewsData == null)
        {
            GamePlayController_BirdSort.Instance.level.screwManager.SpawnFirstScrewBoxes();

            return;
        }

        ScrewData[] screwsData = levelProgressData.ScrewsData;
        ScrewBoxData[] screwBoxesData = levelProgressData.ScrewBoxesData;

        for (int i = 0; i < screwBoxesData.Length; i++)
        {
            GamePlayController_BirdSort.Instance.level.screwBoxManager.SpawnScrewBox(screwBoxesData[i].Faction, screwBoxesData[i].IsLocked);
        }

        await Task.Delay(2000);

        Dictionary<BaseScrew, ScrewData> screwsWithData = new Dictionary<BaseScrew, ScrewData>();

        for (int i = 0; i < _screws.Length; i++)
        {
            for (int j = 0; j < screwsData.Length; j++)
            {
                BaseScrew screw = _screws[i];
                ScrewData screwData = screwsData[j];

                if (screwData.ScrewId == screw.ScrewId)
                {
                    screwsWithData.Add(screw, screwData);

                    break;
                }
            }
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
                ScrewBoxSlot screwBoxSlot = GamePlayController_BirdSort.Instance.level.screwBoxManager.CheckAvailableScrewBoxes(screw.Faction);

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
    }

    private ScrewBoxData[] GetScrewBoxesData()
    {
        ScrewBoxData[] screwBoxesData = new ScrewBoxData[GamePlayController_BirdSort.Instance.level.screwBoxManager.ScrewBoxs.Length];

        for (int i = 0; i < GamePlayController_BirdSort.Instance.level.screwBoxManager.ScrewBoxs.Length; i++)
        {
            screwBoxesData[i] = new ScrewBoxData();

            if (GamePlayController_BirdSort.Instance.level.screwBoxManager.ScrewBoxs[i] == null)
            {
                screwBoxesData[i].IsLocked = true;

                continue;
            }

            screwBoxesData[i].Faction = GamePlayController_BirdSort.Instance.level.screwBoxManager.ScrewBoxs[i].Faction;
            screwBoxesData[i].IsLocked = GamePlayController_BirdSort.Instance.level.screwBoxManager.ScrewBoxs[i].IsLocked;
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
            screwsData[i].IsDone = _screws[i].IsDone;
            screwsData[i].IsInScrewPort = _screws[i].IsInScrewPort;

            if (!_screws[i].gameObject.activeSelf)
            {
                screwsData[i].IsDestroyed = true;
            }
        }

        ScrewBoxData[] screwBoxesData = GetScrewBoxesData();

        LevelProgressData levelProgressData = new LevelProgressData();

        levelProgressData.ScrewsData = screwsData;
        levelProgressData.ScrewBoxesData = screwBoxesData;

        DataUtility.SaveAsync($"{GameConstants.SCREWS_DATA}_{UseProfile.CurrentLevel}", levelProgressData);
    }

    private void OnAllLevelScrewsObservered(BaseScrew[] screws)
    {
        _screws = screws;

        LoadLevelData();
    }
}
