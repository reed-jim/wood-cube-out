using System;
using System.Collections.Generic;
using System.Linq;
using NamPhuThuy;
using NamPhuThuy.GamePlay;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;
using static GameEnum;

public class ScrewBoxManager : MonoBehaviour
{
    public static ScrewBoxManager Instance;

    [SerializeField] private ScrewBox[] screwBoxs;
    [SerializeField] private List<ScrewBoxSlot> screwPorts;
    [SerializeField] private Transform screwPortsContainer;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private ScrewBoxesObserver screwBoxesObserver;

    [Header("CUSTOMIZE")]
    [SerializeField] private int maxScrewBox;

    public ScrewBox[] ScrewBoxs
    {
        get => screwBoxs;
    }

    public List<ScrewBoxSlot> ScrewPorts
    {
        get => screwPorts;
    }

    #region PRIVATE FIELD
    private List<Tween> _tweens;
    private float _screwBoxPositionY = 10;
    private bool _isSpawnScrewBoxFirstTimeInLevel;
    private float _screwBoxDistance;
    private bool _isScrewPortsShaking;
    private bool _isCheckingLose;
    #endregion

    #region EVENT
    public static event Action<int, GameFaction, ScrewBoxSlot> looseScrewEvent;
    public static event Action<int, GameFaction, ScrewBoxSlot> looseScrewWithoutCheckingVisibilityEvent;
    public static event Action<int, GameFaction, ScrewBoxSlot> looseScrewWithoutCheckingObstaclesEvent;
    public static event Action<ScrewBox> screwBoxSpawnedEvent;
    public static event Action<ScrewBoxLocked> screwBoxLockSpawnedEvent;
    public static event Action loseLevelEvent;
    public static event Action<int> screwPortsClearedEvent;
    public static event Action<bool> enableBoosterEvent;
    #endregion

    #region LIFE CYCLE
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);

            return;
        }
        else
        {
            Instance = this;
        }

        LevelLoader.startLevelEvent += OnLevelStart;
        BaseScrew.selectScrewEvent += OnScrewSelected;
        BaseScrew.autoSelectScrewEvent += OnScrewAutoSelected;
        BaseScrew.screwMovedCompletedEvent += MoveSingleScrewFromHoleToScrewBox;
        ScrewManager.spawnScrewBoxEvent += SpawnScrewBox;
        ScrewBoxCameraManager.setCameraEvent += OnScrewBoxCameraSet;
        ScrewManager.spawnAdsScrewBoxesEvent += SpawnAdsScrewBoxes;
        ScrewBox.screwBoxCompletedEvent += OnScrewBoxCompleted;
        ScrewBox.screwBoxUnlockedEvent += OnScrewBoxUnlocked;
        ScrewBoxLocked.lockedScrewBoxTaskDoneEvent += OnScrewBoxUnlocked;
        BoosterUI.addMoreScrewPortEvent += AddMoreScrewPort;
        BoosterUI.clearAllScrewPortsEvent += ClearAllScrewPorts;
        BoosterUI.shakeScrewPortsEvent += ShakeAllScrewPorts;
        BasicObjectPart.loosenScrewOnObjectBrokenEvent += LoosenScrewOnObjectBroken;
        RevivePopup.reviveEvent += OnRevived;
        BuyBoosterPopup.unlockScrewBoxEvent += OnRewardedAdUnlockScrewBoxCompleted;
        LockScrewBoxManager.requestLockedScrewBox += SpawnLockedScrewBox;

        _tweens = new List<Tween>();
    }

    void OnDestroy()
    {
        LevelLoader.startLevelEvent -= OnLevelStart;
        BaseScrew.selectScrewEvent -= OnScrewSelected;
        BaseScrew.autoSelectScrewEvent -= OnScrewAutoSelected;
        BaseScrew.screwMovedCompletedEvent -= MoveSingleScrewFromHoleToScrewBox;
        ScrewBoxCameraManager.setCameraEvent -= OnScrewBoxCameraSet;
        ScrewManager.spawnScrewBoxEvent -= SpawnScrewBox;
        ScrewManager.spawnAdsScrewBoxesEvent -= SpawnAdsScrewBoxes;
        ScrewBox.screwBoxCompletedEvent -= OnScrewBoxCompleted;
        ScrewBox.screwBoxUnlockedEvent -= OnScrewBoxUnlocked;
        ScrewBoxLocked.lockedScrewBoxTaskDoneEvent -= OnScrewBoxUnlocked;
        BoosterUI.addMoreScrewPortEvent -= AddMoreScrewPort;
        BoosterUI.clearAllScrewPortsEvent -= ClearAllScrewPorts;
        BoosterUI.shakeScrewPortsEvent -= ShakeAllScrewPorts;
        BasicObjectPart.loosenScrewOnObjectBrokenEvent -= LoosenScrewOnObjectBroken;
        RevivePopup.reviveEvent -= OnRevived;
        BuyBoosterPopup.unlockScrewBoxEvent -= OnRewardedAdUnlockScrewBoxCompleted;
        LockScrewBoxManager.requestLockedScrewBox -= SpawnLockedScrewBox;

        CommonUtil.StopAllTweens(_tweens);

        ReturnScrewBoxesToPool();
    }
    #endregion

    private void ReturnScrewBoxesToPool()
    {
        DebugLogger.LogSimple();
        if (screwBoxs == null) return;

        for (int i = 0; i < screwBoxs.Length; i++)
        {
            if (screwBoxs[i] == null) continue;
            for (int j = 0; j < screwBoxs[i].ScrewBoxSlots.Length; j++)
            {
                if (screwBoxs[i].ScrewBoxSlots[j].Screw == null) continue;

                screwBoxs[i].ScrewBoxSlots[j].IsFilled = false;
                Destroy(screwBoxs[i].ScrewBoxSlots[j].Screw.gameObject);
            }

            ObjectPoolingEverything.ReturnToPool(GameConstants.SCREW_BOX, screwBoxs[i].gameObject, screwBoxs[i].transform.parent);
        }
    }

    private void ResetScrewBoxes()
    {
        DebugLogger.LogSimple();
        ReturnScrewBoxesToPool();

        screwBoxs = new ScrewBox[maxScrewBox];

        // RESET SCREW PORTS
        for (int i = 0; i < screwPorts.Count; i++)
        {
            if (screwPorts[i].IsFilled)
            {
                screwPorts[i].IsFilled = false;

                Destroy(screwPorts[i].Screw.gameObject);

                screwPorts[i].Screw = null;
            }
        }

        GamePersistentVariable.numHolesAdded = 0;

        int toRemoveStartIndex = 0;
        int toRemoveCount = 0;

        for (int i = 0; i < screwPorts.Count; i++)
        {
            if (i >= GameConstants.DEFAULT_NUMBER_SCREW_PORT)
            {
                ObjectPoolingEverything.ReturnToPool(GameConstants.SCREW_PORT_SLOT, screwPorts[i].gameObject);

                if (toRemoveStartIndex == 0)
                {
                    toRemoveStartIndex = i;
                }

                toRemoveCount++;
            }
        }

        screwPorts.RemoveRange(toRemoveStartIndex, toRemoveCount);

        Vector3 screwPortPosition = screwPorts[0].transform.position;

        for (int i = 0; i < screwPorts.Count; i++)
        {
            screwPortPosition.x = (-(screwPorts.Count - 1) / 2f + i) * 1.05f * screwPorts[i].transform.lossyScale.x;

            screwPorts[i].transform.position = screwPortPosition;
        }

        screwBoxesObserver.NumLockedScrewBoxes = GameConstants.DEFAULT_INITIAL_NUMBER_SCREW_BOXES;
        screwBoxesObserver.NumScrewInScrewPorts = 0;
    }

    #region CALLBACK
    private void OnLevelStart()
    {
        DebugLogger.LogSimple();
        ResetScrewBoxes();

        _isSpawnScrewBoxFirstTimeInLevel = true;
    }

    private void OnScrewBoxCameraSet(Camera screwBoxCamera)
    {
        // DebugLogger.LogSimple();
        Vector2 worldSize = CommonUtil.GetScreenSizeWorld(screwBoxCamera);

        if (0.19f * worldSize.x < 0.11f * worldSize.y)
        {
            _screwBoxDistance = 0.19f * worldSize.x;
        }
        else
        {
            _screwBoxDistance = 0.11f * worldSize.y;
        }

        _screwBoxPositionY = 0.34f * worldSize.y;

        // screwPortsContainer.localPosition = screwPortsContainer.localPosition.ChangeY(0.3042f * worldSize.y);

        Vector3 screwPortPosition = screwPorts[0].transform.position;

        screwPortPosition.y = 0.26f * worldSize.y;

        for (int i = 0; i < screwPorts.Count; i++)
        {
            screwPortPosition.x = (-(screwPorts.Count - 1) / 2f + i) * 1.05f * screwPorts[i].transform.lossyScale.x;

            screwPorts[i].transform.position = screwPortPosition;
        }
    }

    private void OnRevived(BoosterType boosterType)
    {
        DebugLogger.LogSimple();
        if (boosterType == BoosterType.UnlockScrewBox)
        {
            for (int i = 0; i < screwBoxs.Length; i++)
            {
                if (screwBoxs[i] == null)
                {
                    continue;
                }

                if (screwBoxs[i].IsValidToUnlock())
                {
                    screwBoxs[i].ScrewBoxServiceLocator.screwBoxUI.Unlock();

                    break;
                }
            }
        }
        else if (boosterType == BoosterType.ClearScrewPorts)
        {
            ClearAllScrewPorts();
        }
    }

    private void OnScrewSelected(int screwId, GameFaction selectedFaction)
    {
        DebugLogger.LogSimple();
        ScrewBoxSlot screwBoxSlot = CheckAvailableScrewBoxes(selectedFaction);

        if (screwBoxSlot != null)
        {
            looseScrewEvent?.Invoke(screwId, selectedFaction, screwBoxSlot);
        }

        GamePersistentVariable.isFreshLevel = false;
    }

    private void OnScrewAutoSelected(int screwId, GameFaction selectedFaction)
    {
        DebugLogger.LogSimple();
        ScrewBoxSlot screwBoxSlot = CheckAvailableScrewBoxes(selectedFaction);

        if (screwBoxSlot != null)
        {
            looseScrewEvent?.Invoke(screwId, selectedFaction, screwBoxSlot);
        }
    }

    private void OnScrewBoxCompleted(ScrewBox screwBox)
    {
        DebugLogger.LogSimple();
        for (int i = 0; i < screwBoxs.Length; i++)
        {
            if (screwBoxs[i] == screwBox)
            {
                screwBoxs[i] = null;

                break;
            }
        }
    }

    private void OnScrewBoxUnlocked(ScrewBox screwBox)
    {
        DebugLogger.LogSimple();
        MoveFromScrewPortToScrewBox(screwBox);

        screwBoxSpawnedEvent?.Invoke(screwBox);

        int numLockedScrewBoxes = 0;

        for (int i = 0; i < screwBoxs.Length; i++)
        {
            if (screwBoxs[i] == null)
            {
                continue;
            }

            if (screwBoxs[i].IsLocked)
            {
                numLockedScrewBoxes++;
            }
        }

        screwBoxesObserver.NumLockedScrewBoxes = numLockedScrewBoxes;
    }

    private void OnRewardedAdUnlockScrewBoxCompleted()
    {
        DebugLogger.LogSimple();
        for (int i = 0; i < screwBoxs.Length; i++)
        {
            if (screwBoxs[i] == null)
            {
                continue;
            }

            if (screwBoxs[i].IsValidToUnlock())
            {
                screwBoxs[i].ScrewBoxServiceLocator.screwBoxUI.Unlock();

                break;
            }
        }
    }
    #endregion

    public ScrewBoxSlot CheckAvailableScrewBoxes(GameFaction selectedFaction)
    {
        return CheckAvailableScrewBoxes(selectedFaction, isIncludeScrewPorts: true);
    }

    public ScrewBoxSlot CheckAvailableScrewBoxes(GameFaction selectedFaction, bool isIncludeScrewPorts)
    {
        DebugLogger.LogSimple();
        foreach (var screwBox in screwBoxs)
        {
            if (screwBox == null)
            {
                continue;
            }

            if (screwBox.IsLocked || screwBox.IsInTransition)
            {
                continue;
            }

            if (selectedFaction == screwBox.Faction)
            {
                foreach (var screwBoxSlot in screwBox.ScrewBoxSlots)
                {
                    if (!screwBoxSlot.IsFilled)
                    {
                        return screwBoxSlot;
                    }
                }
            }
        }

        ScrewBoxSlot emptyScrewPort = null;
        int numScrewPortsEmpty = 0;

        if (isIncludeScrewPorts)
        {
            foreach (var screwPort in screwPorts)
            {
                if (!screwPort.IsFilled)
                {
                    if (numScrewPortsEmpty == 0)
                    {
                        emptyScrewPort = screwPort;
                    }

                    numScrewPortsEmpty++;
                }
            }

            if (numScrewPortsEmpty <= GamePersistentVariable.screwHoleLeftToLose && !_isCheckingLose)
            {
                _tweens.Add(Tween.Delay(3).OnComplete(() =>
                {
                    if (GetNumScrewPortsEmpty() == 0)
                    {
                        loseLevelEvent?.Invoke();
                    }

                    _isCheckingLose = false;
                }));

                _isCheckingLose = true;
            }

            if (numScrewPortsEmpty > 0)
            {
                screwBoxesObserver.NumScrewInScrewPorts++;

                return emptyScrewPort;
            }
            else
            {
                ShakeAllScrewPorts();
            }
        }

        return null;
    }

    private int GetNumScrewPortsEmpty()
    {
        int numScrewPortsEmpty = 0;

        foreach (var screwPort in screwPorts)
        {
            if (!screwPort.IsFilled)
            {
                numScrewPortsEmpty++;
            }
        }

        return numScrewPortsEmpty;
    }

    #region SPAWN SCREW BOX
    public void SpawnScrewBox(GameFaction faction)
    {
        DebugLogger.LogSimple();
        SpawnScrewBox(faction, false);
    }

    public void SpawnScrewBox(GameFaction faction, bool isLocked = false)
    {
        DebugLogger.LogSimple();
        if (isLocked)
        {
            SpawnAdsScrewBox();

            return;
        }

        ScrewBox screwBox = ObjectPoolingEverything.GetFromPool<ScrewBox>(GameConstants.SCREW_BOX);

        if (screwBox == null)
        {
            DebugLogger.LogError(content: $"ScrewBoxManager.SpawnScrewBox(): The ScrewBox is null");
        }

        screwBox.Faction = faction;
        screwBox.IsLocked = false;

        screwBox.ResetVisually();

        int index = 0;

        bool isFound = false;

        for (int i = 0; i < screwBoxs.Length; i++)
        {
            if (screwBoxs[i] == null)
            {
                screwBoxs[i] = screwBox;

                index = i;

                isFound = true;

                break;
            }
        }

        if (!isFound)
        {
            return;
        }

        Direction getInDirection = index > 1 ? Direction.Left : Direction.Right;

        ResetScrewBoxSlot();

        Vector3 destination = GetScrewBoxNextPosition(screwBox, index);

        screwBox.transform.localPosition = destination.ChangeX(getInDirection == Direction.Right ? -10 : 10);

        screwBox.IsInTransition = true;

        _tweens.Add(Tween.LocalPosition(screwBox.transform, destination, duration: 0.4f).OnComplete(() =>
        {
            screwBox.IsInTransition = false;

            MoveFromScrewPortToScrewBox(screwBox);

            screwBoxSpawnedEvent?.Invoke(screwBox);
        }));


        void ResetScrewBoxSlot()
        {
            for (int i = 0; i < screwBox.ScrewBoxSlots.Length; i++)
            {
                ScrewBoxSlot screwBoxSlot = screwBox.ScrewBoxSlots[i];

                if (screwBoxSlot.IsFilled)
                {
                    screwBoxSlot.Screw.gameObject.SetActive(false);

                    screwBoxSlot.Screw = null;

                    screwBoxSlot.IsFilled = false;
                }
            }
        }
    }

    public void SpawnScrewBox(ScrewBoxData screwBoxData)
    {

    }

    private void SpawnAdsScrewBox()
    {
        Vector3 destination;

        for (int i = 0; i < screwBoxs.Length; i++)
        {
            if (screwBoxs[i] == null)
            {
                ScrewBox screwBox = ObjectPoolingEverything.GetFromPool<ScrewBox>(GameConstants.SCREW_BOX);

                screwBox.transform.position = new Vector3(10, _screwBoxPositionY, screwBox.transform.position.z);

                screwBox.Lock();

                int index = i;

                destination = GetScrewBoxNextPosition(screwBox, index);
                // destination.x = (-(maxScrewBox - 1) / 2f + index) * _screwBoxDistance;
                // destination.z = -0.001f * index;

                _tweens.Add(Tween.LocalPosition(screwBox.transform, destination, duration: 0.4f)
                .OnComplete(() =>
                {
                    screwBox.ScrewBoxServiceLocator.screwBoxUI.SetUnlockByAdsButtonPosition();
                }));

                screwBoxs[i] = screwBox;

                break;
            }
        }
    }

    private void SpawnAdsScrewBoxes()
    {
        // DebugLogger.LogSimple();
        for (int i = 0; i < screwBoxs.Length; i++)
        {
            if (screwBoxs[i] != null)
            {
                continue;
            }

            ScrewBox screwBox = ObjectPoolingEverything.GetFromPool<ScrewBox>(GameConstants.SCREW_BOX);
            screwBox.transform.position = new Vector3(10, _screwBoxPositionY, screwBox.transform.position.z);

            screwBox.Lock();

            Vector3 destination = GetScrewBoxNextPosition(screwBox, i);

            if (_isSpawnScrewBoxFirstTimeInLevel)
            {
                screwBox.transform.localPosition = destination;

                screwBox.ScrewBoxServiceLocator.screwBoxUI.SetUnlockByAdsButtonPosition();
            }
            else
            {
                _tweens.Add(Tween.LocalPosition(screwBox.transform, destination, duration: 0.4f)
                    .OnComplete(() =>
                    {
                        screwBox.ScrewBoxServiceLocator.screwBoxUI.SetUnlockByAdsButtonPosition();
                    }));
            }

            screwBoxs[i] = screwBox;
        }

        _isSpawnScrewBoxFirstTimeInLevel = false;
    }

    public void UnlockAllScrewBoxes()
    {
        int totalBoxUnlocked = 0;

        for (int i = 0; i < screwBoxs.Length; i++)
        {
            if (screwBoxs[i] == null)
            {
                continue;
            }

            if (screwBoxs[i].IsValidToUnlock())
            {
                screwBoxs[i].ScrewBoxServiceLocator.screwBoxUI.Unlock();

                totalBoxUnlocked++;

                if (totalBoxUnlocked == 2)
                {
                    break;
                }
            }
        }
    }
    #endregion

    #region SPECIAL LEVEL
    private void SpawnLockedScrewBox(ScrewBox refScrewBox, SpecialLevelTaskRecord record)
    {
        ScrewBoxLocked lockedScrewBox = ObjectPoolingEverything.GetFromPool<ScrewBoxLocked>(GameConstants.SCREW_BOX_LOCKED);

        refScrewBox.gameObject.SetActive(false);
        lockedScrewBox.gameObject.SetActive(true);

        lockedScrewBox.Setup(record);

        lockedScrewBox.transform.SetParent(refScrewBox.transform.parent);

        lockedScrewBox.transform.position = refScrewBox.transform.position;
        lockedScrewBox.transform.localScale = refScrewBox.transform.localScale;
        lockedScrewBox.SetSize(CommonUtil.GetScreenSizeWorld(Camera.main));

        int index = screwBoxs
            .Select((value, idx) => new { value, idx })
            .First(item => item.value == refScrewBox).idx;

        screwBoxs[index] = lockedScrewBox;
        screwBoxLockSpawnedEvent?.Invoke(lockedScrewBox);
    }
    #endregion

    private void MoveFromScrewPortToScrewBox(ScrewBox screwBox)
    {
        // avoid too fast action
        enableBoosterEvent?.Invoke(false);

        bool isFound = false;

        for (int i = 0; i < screwPorts.Count; i++)
        {
            BaseScrew screw = screwPorts[i].Screw;

            if (screw == null)
            {
                continue;
            }

            if (screw.IsDone || screw.IsMoving)
            {
                continue;
            }

            if (screwPorts[i].IsFilled && screw.Faction == screwBox.Faction)
            {
                for (int k = 0; k < screwBox.ScrewBoxSlots.Length; k++)
                {
                    if (!screwBox.ScrewBoxSlots[k].IsFilled)
                    {
                        int screwPortIndex = i;
                        int screwBoxSlotIndex = k;

                        screwBox.ScrewBoxSlots[screwBoxSlotIndex].Fill(screwPorts[screwPortIndex].Screw);

                        screwPorts[screwPortIndex].IsFilled = false;

                        screwPorts[screwPortIndex].Screw.UnscrewAnimationAsync
                        (
                            screwBoxSlot: screwBox.ScrewBoxSlots[screwBoxSlotIndex],
                            onCompleteAction: () =>
                            {
                                // screwBox.ScrewBoxSlots[screwBoxSlotIndex].CompleteFill();

                                enableBoosterEvent?.Invoke(true);
                            },
                            isSwitchCamera: false
                        );

                        if (!isFound)
                        {
                            isFound = true;
                        }

                        break;
                    }
                }
            }
        }

        screwBoxesObserver.NumScrewInScrewPorts = GetNumScrewsInScrewPorts();

        if (!isFound)
        {
            enableBoosterEvent?.Invoke(true);
        }
    }

    private void MoveSingleScrewFromHoleToScrewBox(BaseScrew screw)
    {
        DebugLogger.LogSimple();
        if (!screw.ScrewBoxSlot.IsScrewPort)
        {
            return;
        }

        if (screw.IsDone || screw.IsMoving)
        {
            return;
        }

        bool isFound = false;

        for (int i = 0; i < screwBoxs.Length; i++)
        {
            ScrewBox screwBox = screwBoxs[i];

            if (screwBox == null)
            {
                continue;
            }

            if (screwBox.Faction != screw.Faction || screwBox.IsLocked || screwBox.IsInTransition)
            {
                continue;
            }

            for (int k = 0; k < screwBox.ScrewBoxSlots.Length; k++)
            {
                if (!screwBox.ScrewBoxSlots[k].IsFilled)
                {
                    int screwBoxSlotIndex = k;

                    screwBox.ScrewBoxSlots[screwBoxSlotIndex].Fill(screw);

                    screw.ScrewBoxSlot.IsFilled = false;

                    screw.UnscrewAnimationAsync
                    (
                        screwBoxSlot: screwBox.ScrewBoxSlots[screwBoxSlotIndex],
                        onCompleteAction: () =>
                        {
                            // screwBox.ScrewBoxSlots[screwBoxSlotIndex].CompleteFill();

                            enableBoosterEvent?.Invoke(true);
                        },
                        isSwitchCamera: false
                    );

                    isFound = true;

                    break;
                }
            }

            if (isFound)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Count available screw slot in screw-boxes by faction
    /// </summary>
    /// <returns></returns>
    public Dictionary<GameFaction, int> GetScrewSlotsAvailableByFaction()
    {
        Dictionary<GameFaction, int> screwSlotAvailableByFaction = new Dictionary<GameFaction, int>();

        foreach (var faction in GameConstants.SCREW_FACTION)
        {
            screwSlotAvailableByFaction.Add(faction, 0);
        }

        for (int i = 0; i < screwBoxs.Length; i++)
        {
            if (screwBoxs[i] == null || screwBoxs[i].IsLocked)
            {
                continue;
            }

            GameFaction faction = screwBoxs[i].Faction;

            for (int j = 0; j < screwBoxs[i].ScrewBoxSlots.Length; j++)
            {
                ScrewBoxSlot screwBoxSlot = screwBoxs[i].ScrewBoxSlots[j];

                if (!screwBoxSlot.IsFilled || screwBoxs[i].IsInTransition)
                {
                    screwSlotAvailableByFaction[faction]++;
                }
            }
        }

        return screwSlotAvailableByFaction;
    }

    #region BOOSTER
    private void LoosenScrewOnObjectBroken(BaseScrew screw)
    {
        DebugLogger.LogSimple();
        try
        {
            ScrewBoxSlot screwBoxSlot = CheckAvailableScrewBoxes(screw.Faction, isIncludeScrewPorts: false);

            if (screwBoxSlot != null)
            {
                looseScrewWithoutCheckingObstaclesEvent?.Invoke(screw.ScrewId, screw.Faction, screwBoxSlot);
            }
            else
            {
                screw.ForceUnscrew();
            }
        }
        catch (System.Exception)
        {

        }
    }

    public void AddMoreScrewPort()
    {
        DebugLogger.LogSimple();
        ScrewBoxSlot addedScrewBoxSlot = ObjectPoolingEverything.GetFromPool<ScrewBoxSlot>(GameConstants.SCREW_PORT_SLOT);

        screwPorts.Add(addedScrewBoxSlot);

        addedScrewBoxSlot.transform.SetParent(screwPorts[0].transform.parent);

        addedScrewBoxSlot.transform.localRotation = Quaternion.Euler(Vector3.zero);
        addedScrewBoxSlot.transform.localScale = screwPorts[0].transform.localScale;

        Vector3 position = screwPorts[0].transform.position;

        // addedScrewBoxSlot.transform.position = position + new Vector3(10, 0, 0);
        addedScrewBoxSlot.transform.localScale = Vector3.zero;

        for (int i = 0; i < screwPorts.Count; i++)
        {
            int index = i;

            position.x = (-(screwPorts.Count - 1) / 2f + i) * 1.05f * screwPorts[0].transform.lossyScale.x;

            _tweens.Add(Tween.Position(screwPorts[i].transform, position, duration: 0.3f)
            .OnComplete(() =>
            {
                if (index == screwPorts.Count - 1)
                {
                    _tweens.Add(Tween.Scale(addedScrewBoxSlot.transform, screwPorts[0].transform.localScale, duration: 0.3f));
                }
            }));
        }

        GamePersistentVariable.numHolesAdded++;
    }

    private void ClearAllScrewPorts()
    {
        DebugLogger.LogSimple();
        int numScrewCleared = 0;

        for (int i = 0; i < screwPorts.Count; i++)
        {
            if (screwPorts[i].IsFilled)
            {
                if (screwPorts[i].Screw.IsMoving)
                {
                    continue;
                }

                screwPorts[i].IsFilled = false;

                screwPorts[i].Screw.ForceUnscrew();

                screwPorts[i].Screw = null;

                numScrewCleared++;
            }
        }

        screwBoxesObserver.NumScrewInScrewPorts = GetNumScrewsInScrewPorts();

        screwPortsClearedEvent?.Invoke(numScrewCleared);
    }

    private int GetNumScrewsInScrewPorts()
    {
        int numScrew = 0;

        for (int i = 0; i < screwPorts.Count; i++)
        {
            if (screwPorts[i].IsFilled)
            {
                numScrew++;
            }
        }

        return numScrew;
    }

    private void ShakeAllScrewPorts()
    {
        DebugLogger.LogSimple();
        if (_isScrewPortsShaking)
        {
            return;
        }

        for (int i = 0; i < screwPorts.Count; i++)
        {
            bool isLast = i == screwPorts.Count - 1;

            _tweens.Add(Tween.PunchScale(screwPorts[i].transform, strength: 0.3f * Vector3.one, duration: 0.3f)
            .OnComplete(() =>
            {
                if (isLast)
                {
                    _isScrewPortsShaking = false;
                }
            }));

            SoundManager.Instance.PlaySoundLoosenScrewFail();
        }

        _isScrewPortsShaking = true;
    }
    #endregion

    #region UTIL
    private Vector3 GetScrewBoxNextPosition(ScrewBox screwBox, int nextIndex)
    {
        Vector3 destination = screwBox.transform.position;

        destination.x = (-(maxScrewBox - 1) / 2f + nextIndex) * _screwBoxDistance;
        destination.y = _screwBoxPositionY;
        destination.z = -0.001f * nextIndex;

        return destination;
    }
    #endregion
}
