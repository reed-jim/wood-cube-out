using System;
using System.Collections.Generic;
using System.Linq;
using BirdSort2;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;
using static GameEnum;

public class ScrewBoxManager : MonoBehaviour
{
    [SerializeField] private ScrewBox[] screwBoxs;
    [SerializeField] private List<ScrewBoxSlot> screwPorts;
    [SerializeField] private Transform screwPortsContainer;

    private int _numLockedScrewBoxes;
    private int _numScrewInScrewPorts;

    public int NumLockedScrewBoxes
    {
        get => _numLockedScrewBoxes;
        set => _numLockedScrewBoxes = value;
    }

    public int NumScrewInScrewPorts
    {
        get => _numScrewInScrewPorts;
        set => _numScrewInScrewPorts = value;
    }

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
    private float _screwBoxPositionY = 10;
    private bool _isSpawnScrewBoxFirstTimeInLevel;
    private float _screwBoxDistance;
    private bool _isScrewPortsShaking;
    [SerializeField] private Vector3 screwBoxInitialScale;
    [SerializeField] private Sprite boxSprt;
    
    #endregion

    #region EVENT
    public static event Action<int> screwPortsClearedEvent;
    public static event Action<bool> enableBoosterEvent;
        
    #endregion

    #region LIFE CYCLE
    void Awake()
    {
        BoosterUI.addMoreScrewPortEvent += AddMoreScrewPort;
        BoosterUI.clearAllScrewPortsEvent += ClearAllScrewPorts;
        BoosterUI.shakeScrewPortsEvent += ShakeAllScrewPorts;
    }

    void OnDestroy()
    {
        BoosterUI.addMoreScrewPortEvent -= AddMoreScrewPort;
        BoosterUI.clearAllScrewPortsEvent -= ClearAllScrewPorts;
        BoosterUI.shakeScrewPortsEvent -= ShakeAllScrewPorts;
        ReturnScrewBoxesToPool();
    }
    #endregion

    private void ReturnScrewBoxesToPool()
    {
        if (screwBoxs != null)
        {
            for (int i = 0; i < screwBoxs.Length; i++)
            {
                if (screwBoxs[i] != null)
                {
                    for (int j = 0; j < screwBoxs[i].ScrewBoxSlots.Length; j++)
                    {
                        if (screwBoxs[i].ScrewBoxSlots[j].Screw != null)
                        {
                            screwBoxs[i].ScrewBoxSlots[j].IsFilled = false;

                            Destroy(screwBoxs[i].ScrewBoxSlots[j].Screw.gameObject);
                        }
                    }

                    ObjectPoolingEverything.ReturnToPool(GameConstants.SCREW_BOX, screwBoxs[i].gameObject, screwBoxs[i].transform.parent);
                }
            }
        }
    }

    private void ResetScrewBoxes()
    {
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
        
        NumLockedScrewBoxes = GameConstants.DEFAULT_INITIAL_NUMBER_SCREW_BOXES;
        NumScrewInScrewPorts = 0;
    }

    #region CALLBACK
    public void Init()
    {
        _isSpawnScrewBoxFirstTimeInLevel = true;
        Vector2 worldSize = CommonUtil.GetScreenSizeWorld(GamePlayController_BirdSort.Instance.gamePlayCamera.GetComponent<Camera>());

        float scaleWithWidth = (0.18f * worldSize.x) / boxSprt.bounds.size.x;
        float scaleWithHeight = (0.11f * worldSize.y) / boxSprt.bounds.size.y;

        //scaleWithWidth *= 1.25f;
        //scaleWithHeight *= 1.25f;

        float finalScale = Mathf.Min(scaleWithWidth, scaleWithHeight);
        
        screwBoxInitialScale = finalScale * Vector3.one;
        
        _screwBoxDistance = 0.22f * worldSize.x;

        _screwBoxPositionY = 0.33f * worldSize.y;
        Vector3 screwPortPosition = screwPorts[0].transform.localPosition;
        Debug.Log("Anhtuszz dbg: " + screwPortPosition.y + " " + screwPorts[0].transform.localPosition.y);
        for (int i = 0; i < screwPorts.Count; i++)
        {
            screwPortPosition.x = (-(screwPorts.Count - 1) / 2f + i) * 1.08f;
            screwPortPosition.y = 0.2f * worldSize.y;
            screwPorts[i].transform.localPosition = screwPortPosition;
        }
        Debug.Log("Anhtuszz dbg 1: " + screwPortPosition.y + " " + screwPorts[0].transform.localPosition.y);
        ResetScrewBoxes();
    }
    public void OnRevived(GiftType boosterType)
    {
        if (boosterType == GiftType.AddBox)
        {
            for (int i = 0; i < screwBoxs.Length; i++)
            {
                if (screwBoxs[i].IsLocked)
                {
                    screwBoxs[i].ScrewBoxServiceLocator.screwBoxUI.Unlock();

                    break;
                }
            }
        }
        else if (boosterType == GiftType.ClearHole)
        {
            ClearAllScrewPorts();
        }
    }

    public void OnScrewSelected(int screwId, GameFaction selectedFaction)
    {
        ScrewBoxSlot screwBoxSlot = CheckAvailableScrewBoxes(selectedFaction);

        if (screwBoxSlot != null)
        {
            foreach (var i in GamePlayController_BirdSort.Instance.level.screwManager._screws)
            {
                i.Loose(screwId, selectedFaction, screwBoxSlot);
            }
        }
    }

    public void OnScrewBoxCompleted(ScrewBox screwBox)
    {
        for (int i = 0; i < screwBoxs.Length; i++)
        {
            if (screwBoxs[i] == screwBox)
            {
                screwBoxs[i] = null;

                break;
            }
        }
    }

    public void OnScrewBoxUnlocked(ScrewBox screwBox)
    {
        MoveFromScrewPortToScrewBox(screwBox);

        GamePlayController_BirdSort.Instance.level.screwManager.OnScrewBoxSpawned(screwBox);

        int numLockedScrewBoxes = 0;

        for (int i = 0; i < screwBoxs.Length; i++)
        {
            if (screwBoxs[i].IsLocked)
            {
                numLockedScrewBoxes++;
            }
        }

        NumLockedScrewBoxes = numLockedScrewBoxes;
    }

    private void OnRewardedAdUnlockScrewBoxCompleted()
    {
        for (int i = 0; i < screwBoxs.Length; i++)
        {
            if (screwBoxs[i] == null)
            {
                continue;
            }

            if (screwBoxs[i].IsLocked)
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

            if (numScrewPortsEmpty == 1)
            {
                if (GamePlayController_BirdSort.Instance.level.screwBoxManager.NumLockedScrewBoxes == 0)
                {
                    GamePlayController_BirdSort.Instance.level.Lose(0);
                }
                else
                {
                    GamePlayController_BirdSort.Instance.level.Lose(1);
                }
                
            }
            else
            {
                NumScrewInScrewPorts++;

                return emptyScrewPort;
            }
        }

        return null;
    }

    #region SPAWN SCREW BOX
    public void SpawnScrewBox(GameFaction faction)
    {
        SpawnScrewBox(faction, false);
    }

    public void SpawnScrewBox(GameFaction faction, bool isLocked = false)
    {
        if (isLocked)
        {
            SpawnAdsScrewBox();

            return;
        }

        ScrewBox screwBox = ObjectPoolingEverything.GetFromPool<ScrewBox>(GameConstants.SCREW_BOX);

        screwBox.Faction = faction;

        int index = 0;

        for (int i = 0; i < screwBoxs.Length; i++)
        {
            if (screwBoxs[i] == null)
            {
                screwBoxs[i] = screwBox;
                screwBoxs[i]._initialScale = screwBoxInitialScale;
                screwBox.transform.localScale = screwBoxInitialScale;
                index = i;

                break;
            }
        }
        
        Direction getInDirection = index > 1 ? Direction.Left : Direction.Right;

        ResetScrewBoxSlot();

        Vector3 destination = GetScrewBoxNextPosition(screwBox, index);

        screwBox.transform.localPosition = destination.ChangeX(getInDirection == Direction.Right ? -10 : 10);
        // destination.x = (-(maxScrewBox - 1) / 2f + index) * _screwBoxDistance;
        // destination.z = -0.001f * index;

        if (_isSpawnScrewBoxFirstTimeInLevel)
        {
            screwBox.transform.localPosition = destination;

            MoveFromScrewPortToScrewBox(screwBox);
        }
        else
        {
            screwBox.IsInTransition = true;

            Tween.LocalPosition(screwBox.transform, destination, duration: 0.5f).OnComplete(() =>
            {
                screwBox.IsInTransition = false;

                MoveFromScrewPortToScrewBox(screwBox);
                GamePlayController_BirdSort.Instance.level.screwManager.OnScrewBoxSpawned(screwBox);
            });
        }

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
    
    private void SpawnAdsScrewBox()
    {
        Vector3 destination;

        for (int i = 0; i < screwBoxs.Length; i++)
        {
            if (screwBoxs[i] == null)
            {
                ScrewBox screwBox = ObjectPoolingEverything.GetFromPool<ScrewBox>(GameConstants.SCREW_BOX);

                screwBox.transform.position = new Vector3(10, _screwBoxPositionY, screwBox.transform.position.z);
                screwBox._initialScale = screwBoxInitialScale;
                screwBox.transform.localScale = screwBoxInitialScale;
                screwBox.Lock();

                int index = i;

                destination = GetScrewBoxNextPosition(screwBox, index);
                // destination.x = (-(maxScrewBox - 1) / 2f + index) * _screwBoxDistance;
                // destination.z = -0.001f * index;

                Tween.LocalPosition(screwBox.transform, destination, duration: 0.5f)
                .OnComplete(() =>
                {
                    screwBox.ScrewBoxServiceLocator.screwBoxUI.SetUnlockByAdsButtonPosition();
                });

                screwBoxs[i] = screwBox;

                break;
            }
        }
    }

    public void SpawnAdsScrewBoxes()
    {
        for (int i = 0; i < screwBoxs.Length; i++)
        {
            if (screwBoxs[i] == null)
            {
                int index = i;

                ScrewBox screwBox = ObjectPoolingEverything.GetFromPool<ScrewBox>(GameConstants.SCREW_BOX);

                screwBox.transform.position = new Vector3(10, _screwBoxPositionY, screwBox.transform.position.z);
                screwBox._initialScale = screwBoxInitialScale;
                screwBox.transform.localScale = screwBoxInitialScale;
                screwBox.Lock();

                Vector3 destination = GetScrewBoxNextPosition(screwBox, index);

                // destination.x = (-(maxScrewBox - 1) / 2f + index) * _screwBoxDistance;
                // destination.z = -0.001f * index;

                if (_isSpawnScrewBoxFirstTimeInLevel)
                {
                    screwBox.transform.localPosition = destination;

                    screwBox.ScrewBoxServiceLocator.screwBoxUI.SetUnlockByAdsButtonPosition();
                }
                else
                {
                    Tween.LocalPosition(screwBox.transform, destination, duration: 0.5f)
                    .OnComplete(() =>
                    {
                        screwBox.ScrewBoxServiceLocator.screwBoxUI.SetUnlockByAdsButtonPosition();
                    });
                }

                screwBoxs[i] = screwBox;
            }
        }

        _isSpawnScrewBoxFirstTimeInLevel = false;
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

            if (screwPorts[i].IsFilled && screw.Faction == screwBox.Faction && !screw.IsMoving)
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

        NumScrewInScrewPorts = GetNumScrewsInScrewPorts();

        if (!isFound)
        {
            enableBoosterEvent?.Invoke(true);
        }
    }

    public void MoveSingleScrewFromHoleToScrewBox(BaseScrew screw)
    {
        if (!screw.ScrewBoxSlot.IsScrewPort)
        {
            return;
        }

        if (screw.IsMoving)
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

    public Dictionary<GameFaction, int> GetScrewPortAvailableByFaction()
    {
        Dictionary<GameFaction, int> screwPortAvailableByFaction = new Dictionary<GameFaction, int>();

        for (int i = 0; i < GameConstants.SCREW_FACTION.Length; i++)
        {
            screwPortAvailableByFaction.Add(GameConstants.SCREW_FACTION[i], 0);
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
                    screwPortAvailableByFaction[faction]++;
                }
            }
        }

        return screwPortAvailableByFaction;
    }

    #region BOOSTER
    public void LoosenScrewOnObjectBroken(BaseScrew screw)
    {
        ScrewBoxSlot screwBoxSlot = CheckAvailableScrewBoxes(screw.Faction, isIncludeScrewPorts: false);

        if (screwBoxSlot != null)
        {
            foreach (var i in GamePlayController_BirdSort.Instance.level.screwManager._screws)
            {
                i.LooseWithoutCheckingObstacles(screw.ScrewId, screw.Faction, screwBoxSlot);
            }
        }
        else
        {
            screw.ForceUnscrew();
        }
    }

    public void AddMoreScrewPort()
    {
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

            position.x = (-(screwPorts.Count - 1) / 2f + i) * 1.08f;

            Tween.Position(screwPorts[i].transform, position, duration: 0.3f)
            .OnComplete(() =>
            {
                if (index == screwPorts.Count - 1)
                {
                    Tween.Scale(addedScrewBoxSlot.transform, Vector3.one, duration: 0.3f);
                }
            });
        }
    }

    public void ClearAllScrewPorts()
    {
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

        NumScrewInScrewPorts = GetNumScrewsInScrewPorts();

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
        if (_isScrewPortsShaking)
        {
            return;
        }

        for (int i = 0; i < screwPorts.Count; i++)
        {
            bool isLast = i == screwPorts.Count - 1;

            Tween.PunchScale(screwPorts[i].transform, strength: 0.3f * Vector3.one, duration: 0.4f)
            .OnComplete(() =>
            {
                if (isLast)
                {
                    _isScrewPortsShaking = false;
                }
            });
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
