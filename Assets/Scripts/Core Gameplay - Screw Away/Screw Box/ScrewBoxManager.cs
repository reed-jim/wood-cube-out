using System;
using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using UnityEngine;
using static GameEnum;

public class ScrewBoxManager : MonoBehaviour
{
    [SerializeField] private ScrewBox[] screwBoxs;
    [SerializeField] private List<ScrewBoxSlot> screwPorts;

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
    private float _screwBoxPositionY = 10;
    private bool _isSpawnScrewBoxFirstTimeInLevel;
    private float _screwBoxDistance;
    #endregion

    #region EVENT
    public static event Action<int, GameFaction, ScrewBoxSlot> looseScrewEvent;
    public static event Action<ScrewBox> screwBoxSpawnedEvent;
    public static event Action loseLevelEvent;
    #endregion

    #region LIFE CYCLE
    void Awake()
    {
        LevelLoader.startLevelEvent += OnLevelStart;
        ScrewBoxCameraManager.setCameraEvent += OnScrewBoxCameraSet;
        BaseScrew.selectScrewEvent += OnScrewSelected;
        ScrewManager.spawnScrewBoxEvent += SpawnScrewBox;
        ScrewManager.spawnAdsScrewBoxesEvent += SpawnAdsScrewBoxes;
        ScrewBox.screwBoxCompletedEvent += OnScrewBoxCompleted;
        ScrewBox.screwBoxUnlockedEvent += OnScrewBoxUnlocked;
        BoosterUI.addMoreScrewPortEvent += AddMoreScrewPort;
        BoosterUI.clearAllScrewPortsEvent += ClearAllScrewPorts;
        BasicObjectPart.loosenScrewOnObjectBrokenEvent += LoosenScrewOnObjectBroken;
        RevivePopup.reviveEvent += OnRevived;
        BuyBoosterPopup.unlockScrewBoxEvent += OnRewardedAdUnlockScrewBoxCompleted;
    }

    void OnDestroy()
    {
        LevelLoader.startLevelEvent -= OnLevelStart;
        ScrewBoxCameraManager.setCameraEvent -= OnScrewBoxCameraSet;
        BaseScrew.selectScrewEvent -= OnScrewSelected;
        ScrewManager.spawnScrewBoxEvent -= SpawnScrewBox;
        ScrewManager.spawnAdsScrewBoxesEvent -= SpawnAdsScrewBoxes;
        ScrewBox.screwBoxCompletedEvent -= OnScrewBoxCompleted;
        ScrewBox.screwBoxUnlockedEvent -= OnScrewBoxUnlocked;
        BoosterUI.addMoreScrewPortEvent -= AddMoreScrewPort;
        BoosterUI.clearAllScrewPortsEvent -= ClearAllScrewPorts;
        BasicObjectPart.loosenScrewOnObjectBrokenEvent -= LoosenScrewOnObjectBroken;
        RevivePopup.reviveEvent -= OnRevived;
        BuyBoosterPopup.unlockScrewBoxEvent -= OnRewardedAdUnlockScrewBoxCompleted;

        ResetScrewBoxes();
    }
    #endregion

    private void ResetScrewBoxes()
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

                    ObjectPoolingEverything.ReturnToPool(GameConstants.SCREW_BOX, screwBoxs[i].gameObject);
                }
            }
        }

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

        Vector3 screwPortPosition = screwPorts[0].transform.position;

        for (int i = 0; i < screwPorts.Count; i++)
        {
            screwPortPosition.x = (-(screwPorts.Count - 1) / 2f + i) * 1.08f;

            screwPorts[i].transform.position = screwPortPosition;
        }

        screwBoxesObserver.NumLockedScrewBoxes = GameConstants.DEFAULT_INITIAL_NUMBER_SCREW_BOXES;
    }

    #region CALLBACK
    private void OnLevelStart()
    {
        ResetScrewBoxes();

        _isSpawnScrewBoxFirstTimeInLevel = true;
    }

    private void OnScrewBoxCameraSet(Camera screwBoxCamera)
    {
        Vector2 worldSize = CommonUtil.GetScreenSizeWorld(screwBoxCamera);

        _screwBoxDistance = 0.215f * worldSize.x;
        // _screwBoxPositionY = 0.333f * worldSize.y;

        // the camera is at position (0, 6.5f, -10) and rotation (30, 0, 0);
        // Vector3 test = screwBoxCamera.transform.InverseTransformPoint(new Vector3(0, (640f / (1920 / 2)) * worldSize.y, 0));

        _screwBoxPositionY = 0.42f * worldSize.y;
    }

    private void OnRevived()
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

    private void OnScrewSelected(int screwId, GameFaction selectedFaction)
    {
        ScrewBoxSlot screwBoxSlot = CheckAvailableScrewBoxes(selectedFaction);

        if (screwBoxSlot != null)
        {
            looseScrewEvent?.Invoke(screwId, selectedFaction, screwBoxSlot);
        }
    }

    private void OnScrewBoxCompleted(ScrewBox screwBox)
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

    private void OnScrewBoxUnlocked(ScrewBox screwBox)
    {
        MoveFromScrewPortToScrewBox(screwBox);

        int numLockedScrewBoxes = 0;

        for (int i = 0; i < screwBoxs.Length; i++)
        {
            if (screwBoxs[i].IsLocked)
            {
                numLockedScrewBoxes++;
            }
        }

        screwBoxesObserver.NumLockedScrewBoxes = numLockedScrewBoxes;
    }

    private void OnRewardedAdUnlockScrewBoxCompleted()
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

            if (screwBox.IsLocked)
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
                loseLevelEvent?.Invoke();
            }
            else
            {
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

        screwBox.transform.position = new Vector3(-10, _screwBoxPositionY, screwBox.transform.position.z);

        int index = 0;

        for (int i = 0; i < screwBoxs.Length; i++)
        {
            if (screwBoxs[i] == null)
            {
                screwBoxs[i] = screwBox;

                index = i;

                break;
            }
        }

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

        Vector3 destination = screwBox.transform.position;

        destination.x = (-(maxScrewBox - 1) / 2f + index) * _screwBoxDistance;
        destination.z = -0.001f * index;

        if (_isSpawnScrewBoxFirstTimeInLevel)
        {
            screwBox.transform.position = destination;

            MoveFromScrewPortToScrewBox(screwBox);
        }
        else
        {
            Tween.LocalPosition(screwBox.transform, destination, duration: 0.5f).OnComplete(() =>
            {
                MoveFromScrewPortToScrewBox(screwBox);

                screwBoxSpawnedEvent?.Invoke(screwBox);
            });
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

                destination = screwBox.transform.position;
                destination.x = (-(maxScrewBox - 1) / 2f + index) * _screwBoxDistance;
                destination.z = -0.001f * index;

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

    private void SpawnAdsScrewBoxes()
    {
        for (int i = 0; i < screwBoxs.Length; i++)
        {
            if (screwBoxs[i] == null)
            {
                ScrewBox screwBox = ObjectPoolingEverything.GetFromPool<ScrewBox>(GameConstants.SCREW_BOX);

                screwBox.transform.position = new Vector3(10, _screwBoxPositionY, screwBox.transform.position.z);

                screwBox.Lock();

                int index = i;




                Vector3 destination = screwBox.transform.position;

                destination.x = (-(maxScrewBox - 1) / 2f + index) * _screwBoxDistance;
                destination.z = -0.001f * index;

                if (_isSpawnScrewBoxFirstTimeInLevel)
                {
                    screwBox.transform.position = destination;

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

    // private void MoveFromScrewPortToScrewBox(ScrewBox screwBox)
    // {
    //     for (int i = 0; i < screwBoxs.Length; i++)
    //     {
    //         if (screwBoxs[i] == null)
    //         {
    //             continue;
    //         }

    //         if (screwBoxs[i].IsLocked)
    //         {
    //             continue;
    //         }

    //         for (int j = 0; j < screwPorts.Count; j++)
    //         {
    //             if (screwPorts[j].IsFilled && screwPorts[j].Screw.Faction == screwBoxs[i].Faction)
    //             {
    //                 for (int k = 0; k < screwBoxs[i].ScrewBoxSlots.Length; k++)
    //                 {
    //                     if (!screwBoxs[i].ScrewBoxSlots[k].IsFilled)
    //                     {
    //                         int screwPortIndex = j;
    //                         int screwBoxIndex = i;
    //                         int screwBoxSlotIndex = k;

    //                         screwBoxs[screwBoxIndex].ScrewBoxSlots[screwBoxSlotIndex].Fill(screwPorts[screwPortIndex].Screw);

    //                         screwPorts[screwPortIndex].Screw.UnscrewAnimationAsync
    //                         (
    //                             screwBoxSlot: screwBoxs[screwBoxIndex].ScrewBoxSlots[screwBoxSlotIndex],
    //                             onCompleteAction: () =>
    //                             {
    //                                 screwPorts[screwPortIndex].IsFilled = false;

    //                                 screwBoxs[screwBoxIndex].ScrewBoxSlots[screwBoxSlotIndex].CompleteFill();
    //                             },
    //                             isSwitchCamera: false
    //                         );

    //                         break;
    //                     }
    //                 }
    //             }
    //         }
    //     }
    // }

    private void MoveFromScrewPortToScrewBox(ScrewBox screwBox)
    {
        for (int i = 0; i < screwPorts.Count; i++)
        {
            if (screwPorts[i].IsFilled && screwPorts[i].Screw.Faction == screwBox.Faction)
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
                                // screwPorts[screwPortIndex].IsFilled = false;

                                screwBox.ScrewBoxSlots[screwBoxSlotIndex].CompleteFill();
                            },
                            isSwitchCamera: false
                        );

                        break;
                    }
                }
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

                if (!screwBoxSlot.IsFilled)
                {
                    screwPortAvailableByFaction[faction]++;
                }
            }
        }

        return screwPortAvailableByFaction;
    }

    #region BOOSTER
    private void LoosenScrewOnObjectBroken(BaseScrew screw)
    {
        ScrewBoxSlot screwBoxSlot = CheckAvailableScrewBoxes(screw.Faction, isIncludeScrewPorts: false);

        if (screwBoxSlot != null)
        {
            looseScrewEvent?.Invoke(screw.ScrewId, screw.Faction, screwBoxSlot);
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
                    Tween.Scale(addedScrewBoxSlot.transform, 1.15f * Vector3.one, duration: 0.3f);
                }
            });

            // screwPorts[i].transform.position = position;
        }
    }

    private void ClearAllScrewPorts()
    {
        for (int i = 0; i < screwPorts.Count; i++)
        {
            if (screwPorts[i].IsFilled)
            {
                screwPorts[i].IsFilled = false;

                screwPorts[i].Screw.ForceUnscrew();

                screwPorts[i].Screw = null;
            }
        }
    }
    #endregion
}
