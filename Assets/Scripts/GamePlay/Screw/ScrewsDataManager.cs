using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NamPhuThuy;
using NamPhuThuy.GamePlay;
using PrimeTween;
using UnityEngine;
using static GameEnum;

public class ScrewsDataManager : MonoBehaviour
{
    [SerializeField] private BaseScrew[] _screws;

    [SerializeField] private ScrewBoxManager screwBoxManager;

    [SerializeField] private IntVariable currentLevel;

    #region PRIVATE FIELD
    private List<Tween> _tweens;
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

        _tweens = new List<Tween>();

        // StartCoroutine(PeriodicallySaving());
    }

    void OnDestroy()
    {
        ScrewManager.setScrewsEvent -= OnAllLevelScrewsObservered;
        GameStateWin.winLevelEvent -= OnLevelWon;
        DebugPopup.prevLevelEvent -= OnPrevLevel;
        DebugPopup.nextLevelEvent -= OnNextLevel;
        DebugPopup.toLevelEvent -= OnDebugToLevel;

        CommonUtil.StopAllTweens(_tweens);
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

            if (
                GamePersistentVariable.isFirebaseRemoteConfigLoaded &&
                !GamePersistentVariable.isEnableLoadingLevelInProgress
            )
            {
                break;
            }
        }
    }

    private async void LoadLevelData()
    {
        if (!GamePersistentVariable.isEnableLoadingLevelInProgress)
        {
            DebugLogger.LogSimple(message:$"Trigger spawnFreshLevelScrewBoxesEvent");
            spawnFreshLevelScrewBoxesEvent?.Invoke();

            return;
        }

        LevelProgressData levelProgressData = DataUtility.Load($"{GameConstants.SCREWS_DATA}_{currentLevel.Value}", new LevelProgressData());

        GamePersistentVariable.isFreshLevel = levelProgressData.ScrewsData == null;

        // No data for current level
        if (levelProgressData.ScrewsData == null)
        {
            DebugLogger.LogSimple(message:$"Trigger spawnFreshLevelScrewBoxesEvent");
            spawnFreshLevelScrewBoxesEvent?.Invoke();

            return;
        }

        ScrewData[] screwsData = levelProgressData.ScrewsData;
        ScrewBoxData[] screwBoxesData = levelProgressData.ScrewBoxesData;

        // screwBoxesData = screwBoxesData.OrderBy(item => item.IsLocked).ToArray();

        if (screwsData.Length != _screws.Length)
        {
            GamePersistentVariable.isFreshLevel = true;

            DebugLogger.LogSimple(message:$"Trigger spawnFreshLevelScrewBoxesEvent");
            spawnFreshLevelScrewBoxesEvent?.Invoke();

            return;
        }

        if (currentLevel.Value == 1)
        {
            DebugLogger.LogSimple(message:$"Trigger endTutorialLevel1Event");
            endTutorialLevel1Event?.Invoke();
        }

        for (int i = 0; i < screwBoxesData.Length; i++)
        {
            screwBoxManager.SpawnScrewBox(screwBoxesData[i].Faction, screwBoxesData[i].IsLocked);
        }

        for (int i = 0; i < levelProgressData.NumHolesAdded; i++)
        {
            screwBoxManager.AddMoreScrewPort();
        }

        await Task.Delay(1000);

        Dictionary<BaseScrew, ScrewData> screwsWithData = new Dictionary<BaseScrew, ScrewData>();

        for (int i = 0; i < _screws.Length; i++)
        {
            screwsWithData.Add(_screws[i], screwsData[i]);
        }

        int totalScrew = _screws.Length;
        int remainingScrew = totalScrew;

        GamePersistentVariable.isLoadingOldLevelState = true;

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
                    // screw.Loose(screw.ScrewId, screw.Faction, screwBoxSlot, isCheckObstacles: false);
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

        _tweens.Add(Tween.Delay(2).OnComplete(() =>
        {
            GamePersistentVariable.isLoadingOldLevelState = false;
        }));

        // UPDATE UI
        DebugLogger.LogSimple(message:$"Trigger updateRemainingScrewEvent");
        updateRemainingScrewEvent?.Invoke(remainingScrew, totalScrew);
        
        DebugLogger.LogSimple(message:$"Trigger updateOutsideScrewNumberEvent");
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

        levelProgressData.NumHolesAdded = GamePersistentVariable.numHolesAdded;

        DataUtility.SaveAsync(key, levelProgressData);

        _currentSaveLevelDataKey = key;
    }

    private void OnAllLevelScrewsObservered(BaseScrew[] screws)
    {
        DebugLogger.LogSimple();
        _screws = screws;

        System.Array.Sort(_screws, (go1, go2) => go2.GetInstanceID().CompareTo(go1.GetInstanceID()));

        if (!GamePersistentVariable.isPreventLoadLevelData)
        {
            LoadLevelData();
        }
        else
        {
            SetFreshLevel(_currentSaveLevelDataKey);

            spawnFreshLevelScrewBoxesEvent?.Invoke();

            // GamePersistentVariable.isPreventLoadLevelData = false;
            // GamePersistentVariable.isFreshLevel = true;

            // DataUtility.RemoveKey($"{GameConstants.SCREWS_DATA}_{currentLevel.Value}");
        }
    }

    private void OnLevelWon()
    {
        DebugLogger.LogSimple();
        GamePersistentVariable.isPreventLoadLevelData = false;
        GamePersistentVariable.isFreshLevel = true;

        SetFreshLevel(_currentSaveLevelDataKey);
    }

    #region UTIL
    private void SetFreshLevel(string levelSaveKey)
    {
        DebugLogger.LogSimple();
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
        DebugLogger.LogSimple();
        string key = $"{GameConstants.SCREWS_DATA}_{currentLevel.Value - 1}";

        SetFreshLevel(key);
    }

    private void OnNextLevel()
    {
        DebugLogger.LogSimple();
        string key = $"{GameConstants.SCREWS_DATA}_{currentLevel.Value + 1}";

        SetFreshLevel(key);
    }

    private void OnDebugToLevel(int level)
    {
        DebugLogger.LogSimple();
        string key = $"{GameConstants.SCREWS_DATA}_{level}";

        SetFreshLevel(key);
    }
    #endregion
}
