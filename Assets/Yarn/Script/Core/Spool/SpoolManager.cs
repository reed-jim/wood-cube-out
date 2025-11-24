using System;
using System.Collections.Generic;
using System.Linq;
using NamPhuThuy.GamePlay;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;
using static GameEnum;

public class SpoolManager : MonoBehaviour
{
    public static SpoolManager Instance;

    [SerializeField] private SpoolBox[] spoolBoxs;
    [SerializeField] private List<SpoolBoxSlot> piecePorts;
    [SerializeField] public Transform outsideSpoolSlot;
    [SerializeField] private Transform piecePortsContainer;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private ScrewBoxesObserver spoolBoxesObserver;

    [Header("CUSTOMIZE")]
    [SerializeField] private int maxSpoolBox;

    public SpoolBox[] SpoolBoxs
    {
        get => spoolBoxs;
    }

    public List<SpoolBoxSlot> PiecePorts
    {
        get => piecePorts;
    }

    #region PRIVATE FIELD
    private List<Tween> _tweens;
    private float _spoolBoxPositionY = 10;
    private bool _isSpawnSpoolBoxFirstTimeInLevel;
    private float _spoolBoxDistance;
    private bool _isPiecePortsShaking;
    private bool _isCheckingLose;
    #endregion

    #region EVENT
    public static event Action<int, GameFaction, SpoolBoxSlot> loosePieceEvent;
    public static event Action<int, GameFaction, SpoolBoxSlot> loosePieceWithoutCheckingVisibilityEvent;
    public static event Action<int, GameFaction, SpoolBoxSlot> loosePieceWithoutCheckingObstaclesEvent;
    public static event Action<SpoolBox> spoolBoxSpawnedEvent;
    public static event Action loseLevelEvent;
    public static event Action<int> piecePortsClearedEvent;
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

        YarnLevelLoader.startLevelEvent += OnLevelStart;
        BasePiece.selectPieceEvent += OnPieceSelected;
        BasePiece.autoSelectPieceEvent += OnPieceAutoSelected;
        BasePiece.pieceMovedCompletedEvent += MoveSinglePieceFromHoleToSpoolBox;
        ScrewBoxCameraManager.setCameraEvent += OnSpoolBoxCameraSet;
        PieceManager.magicPickPieceEvent += OnPieceMagicPicked;
        PieceManager.spawnSpoolBoxEvent += SpawnSpoolBox;
        PieceManager.spawnAdsSpoolBoxesEvent += SpawnAdsSpoolBoxes;
        SpoolBox.spoolBoxCompletedEvent += OnSpoolBoxCompleted;
        SpoolBox.spoolBoxUnlockedEvent += OnSpoolBoxUnlocked;
        BoosterUI.addMoreScrewPortEvent += AddMorePiecePort;
        BoosterUI.clearAllScrewPortsEvent += ClearAllPiecePorts;
        BoosterUI.shakeScrewPortsEvent += ShakeAllPiecePorts;
        // BasePiece.loosenPieceOnObjectBrokenEvent += LoosenPieceOnObjectBroken;
        RevivePopup.reviveEvent += OnRevived;
        BuyBoosterPopup.unlockSpoolBoxEvent += OnRewardedAdUnlockSpoolBoxCompleted;

        _tweens = new List<Tween>();
    }

    void OnDestroy()
    {
        YarnLevelLoader.startLevelEvent -= OnLevelStart;
        BasePiece.selectPieceEvent -= OnPieceSelected;
        BasePiece.autoSelectPieceEvent -= OnPieceAutoSelected;
        BasePiece.pieceMovedCompletedEvent -= MoveSinglePieceFromHoleToSpoolBox;
        ScrewBoxCameraManager.setCameraEvent -= OnSpoolBoxCameraSet;
        PieceManager.magicPickPieceEvent -= OnPieceMagicPicked;
        PieceManager.spawnSpoolBoxEvent -= SpawnSpoolBox;
        PieceManager.spawnAdsSpoolBoxesEvent -= SpawnAdsSpoolBoxes;
        SpoolBox.spoolBoxCompletedEvent -= OnSpoolBoxCompleted;
        SpoolBox.spoolBoxUnlockedEvent -= OnSpoolBoxUnlocked;
        BoosterUI.addMoreScrewPortEvent -= AddMorePiecePort;
        BoosterUI.clearAllScrewPortsEvent -= ClearAllPiecePorts;
        BoosterUI.shakeScrewPortsEvent -= ShakeAllPiecePorts;
        // BasePiece.loosenPieceOnObjectBrokenEvent -= LoosenPieceOnObjectBroken;
        RevivePopup.reviveEvent -= OnRevived;
        BuyBoosterPopup.unlockSpoolBoxEvent -= OnRewardedAdUnlockSpoolBoxCompleted;

        CommonUtil.StopAllTweens(_tweens);

        ReturnSpoolBoxesToPool();
    }
    #endregion

    private void ReturnSpoolBoxesToPool()
    {
        if (spoolBoxs != null)
        {
            for (int i = 0; i < spoolBoxs.Length; i++)
            {
                if (spoolBoxs[i] != null)
                {
                    for (int j = 0; j < spoolBoxs[i].SpoolBoxSlots.Length; j++)
                    {
                        if (spoolBoxs[i].SpoolBoxSlots[j].Piece != null)
                        {
                            spoolBoxs[i].SpoolBoxSlots[j].IsFilled = false;

                            Destroy(spoolBoxs[i].SpoolBoxSlots[j].Piece.gameObject);
                        }
                    }

                    ObjectPoolingEverything.ReturnToPool(GameConstants.spoolBox, spoolBoxs[i].gameObject, spoolBoxs[i].transform.parent);
                }
            }
        }
    }

    private void ResetSpoolBoxes()
    {
        ReturnSpoolBoxesToPool();

        spoolBoxs = new SpoolBox[maxSpoolBox];

        // RESET SCREW PORTS
        for (int i = 0; i < piecePorts.Count; i++)
        {
            if (piecePorts[i].IsFilled)
            {
                piecePorts[i].IsFilled = false;

                Destroy(piecePorts[i].Piece.gameObject);

                piecePorts[i].Piece = null;

                piecePorts[i].Reset();
            }
        }

        GamePersistentVariable.numHolesAdded = 0;

        int toRemoveStartIndex = 0;
        int toRemoveCount = 0;

        for (int i = 0; i < piecePorts.Count; i++)
        {
            if (i >= GameConstants.DEFAULT_NUMBER_SCREW_PORT)
            {
                ObjectPoolingEverything.ReturnToPool(GameConstants.SCREW_PORT_SLOT, piecePorts[i].gameObject);

                if (toRemoveStartIndex == 0)
                {
                    toRemoveStartIndex = i;
                }

                toRemoveCount++;
            }
        }

        piecePorts.RemoveRange(toRemoveStartIndex, toRemoveCount);

        Vector3 piecePortPosition = piecePorts[0].transform.position;

        for (int i = 0; i < piecePorts.Count; i++)
        {
            piecePortPosition.x = (-(piecePorts.Count - 1) / 2f + i) * GamePlayConst.spoolXDistanceScale * piecePorts[i].transform.lossyScale.x;

            piecePorts[i].transform.position = piecePortPosition;
        }

        spoolBoxesObserver.NumLockedScrewBoxes = GameConstants.DEFAULT_INITIAL_NUMBER_SCREW_BOXES;
        spoolBoxesObserver.NumScrewInScrewPorts = 0;
    }

    #region CALLBACK
    private void OnLevelStart()
    {
        ResetSpoolBoxes();

        _isSpawnSpoolBoxFirstTimeInLevel = true;
    }

    private void OnSpoolBoxCameraSet(Camera spoolBoxCamera)
    {
        Vector2 worldSize = CommonUtil.GetScreenSizeWorld(spoolBoxCamera);

        if (0.19f * worldSize.x < 0.11f * worldSize.y)
        {
            _spoolBoxDistance = 0.21f * worldSize.x;
        }
        else
        {
            _spoolBoxDistance = 0.13f * worldSize.y;
        }

        _spoolBoxPositionY = 0.35f * worldSize.y;

        // piecePortsContainer.localPosition = piecePortsContainer.localPosition.ChangeY(0.3042f * worldSize.y);

        Vector3 piecePortPosition = piecePorts[0].transform.position;

        piecePortPosition.y = 0.25f * worldSize.y;

        for (int i = 0; i < piecePorts.Count; i++)
        {
            piecePortPosition.x = (-(piecePorts.Count - 1) / 2f + i) * GamePlayConst.spoolXDistanceScale * piecePorts[i].transform.lossyScale.x;

            piecePorts[i].transform.position = piecePortPosition;
        }
    }

    private void OnRevived(BoosterType boosterType)
    {
        if (boosterType == BoosterType.UnlockSpoolBox)
        {
            for (int i = 0; i < spoolBoxs.Length; i++)
            {
                if (spoolBoxs[i] == null)
                {
                    continue;
                }

                if (spoolBoxs[i].IsLocked)
                {
                    spoolBoxs[i].SpoolBoxServiceLocator.spoolBoxUI.Unlock();

                    break;
                }
            }
        }
        else if (boosterType == BoosterType.ClearAlternativeSpools)
        {
            ClearAllPiecePorts();
        }
    }

    private void OnPieceSelected(int pieceId, GameFaction selectedFaction)
    {
        SpoolBoxSlot spoolBoxSlot = CheckAvailableSpoolBoxes(selectedFaction);

        if (spoolBoxSlot != null)
        {
            loosePieceEvent?.Invoke(pieceId, selectedFaction, spoolBoxSlot);
        }

        GamePersistentVariable.isFreshLevel = false;
    }

    private void OnPieceMagicPicked(int pieceId, GameFaction selectedFaction)
    {
        SpoolBoxSlot spoolBoxSlot = CheckAvailableSpoolBoxes(selectedFaction, isIncludePiecePorts: false);

        if (spoolBoxSlot != null)
        {
            loosePieceEvent?.Invoke(pieceId, selectedFaction, spoolBoxSlot);
        }
    }

    private void OnPieceAutoSelected(int pieceId, GameFaction selectedFaction)
    {
        SpoolBoxSlot spoolBoxSlot = CheckAvailableSpoolBoxes(selectedFaction);

        if (spoolBoxSlot != null)
        {
            loosePieceEvent?.Invoke(pieceId, selectedFaction, spoolBoxSlot);
        }
    }

    private void OnSpoolBoxCompleted(SpoolBox spoolBox)
    {
        for (int i = 0; i < spoolBoxs.Length; i++)
        {
            if (spoolBoxs[i] == spoolBox)
            {
                spoolBoxs[i] = null;

                break;
            }
        }
    }

    private void OnSpoolBoxUnlocked(SpoolBox spoolBox)
    {
        MoveFromPiecePortToSpoolBox(spoolBox);

        spoolBoxSpawnedEvent?.Invoke(spoolBox);

        int numLockedSpoolBoxes = 0;

        for (int i = 0; i < spoolBoxs.Length; i++)
        {
            if (spoolBoxs[i] == null)
            {
                continue;
            }

            if (spoolBoxs[i].IsLocked)
            {
                numLockedSpoolBoxes++;
            }
        }

        spoolBoxesObserver.NumLockedScrewBoxes = numLockedSpoolBoxes;
    }

    private void OnRewardedAdUnlockSpoolBoxCompleted()
    {
        for (int i = 0; i < spoolBoxs.Length; i++)
        {
            if (spoolBoxs[i] == null)
            {
                continue;
            }

            if (spoolBoxs[i].IsLocked)
            {
                spoolBoxs[i].SpoolBoxServiceLocator.spoolBoxUI.Unlock();

                break;
            }
        }
    }
    #endregion

    public SpoolBoxSlot CheckAvailableSpoolBoxes(GameFaction selectedFaction)
    {
        return CheckAvailableSpoolBoxes(selectedFaction, isIncludePiecePorts: true);
    }

    public SpoolBoxSlot CheckAvailableSpoolBoxes(GameFaction selectedFaction, bool isIncludePiecePorts)
    {
        foreach (var spoolBox in spoolBoxs)
        {
            if (spoolBox == null)
            {
                continue;
            }

            if (spoolBox.IsLocked || spoolBox.IsInTransition)
            {
                continue;
            }

            if (selectedFaction == spoolBox.Faction)
            {
                foreach (var spoolBoxSlot in spoolBox.SpoolBoxSlots)
                {
                    if (!spoolBoxSlot.IsFilled)
                    {
                        return spoolBoxSlot;
                    }
                }
            }
        }

        SpoolBoxSlot emptyPiecePort = null;
        int numPiecePortsEmpty = 0;

        if (isIncludePiecePorts)
        {
            foreach (var piecePort in piecePorts)
            {
                if (!piecePort.IsFilled)
                {
                    if (numPiecePortsEmpty == 0)
                    {
                        emptyPiecePort = piecePort;
                    }

                    numPiecePortsEmpty++;
                }
            }

            if (numPiecePortsEmpty <= GamePersistentVariable.screwHoleLeftToLose && !_isCheckingLose)
            {
                _tweens.Add(Tween.Delay(3).OnComplete(() =>
                {
                    if (GetNumPiecePortsEmpty() == 0)
                    {
                        loseLevelEvent?.Invoke();
                    }

                    _isCheckingLose = false;
                }));

                _isCheckingLose = true;
            }

            if (numPiecePortsEmpty > 0)
            {
                spoolBoxesObserver.NumScrewInScrewPorts++;

                return emptyPiecePort;
            }
            else
            {
                ShakeAllPiecePorts();
            }
        }

        return null;
    }

    private int GetNumPiecePortsEmpty()
    {
        int numPiecePortsEmpty = 0;

        foreach (var piecePort in piecePorts)
        {
            if (!piecePort.IsFilled)
            {
                numPiecePortsEmpty++;
            }
        }

        return numPiecePortsEmpty;
    }

    #region SPAWN SCREW BOX
    public void SpawnSpoolBox(GameFaction faction)
    {
        SpawnSpoolBox(faction, false);
    }

    public void SpawnSpoolBox(GameFaction faction, bool isLocked = false)
    {
        if (isLocked)
        {
            SpawnAdsSpoolBox();

            return;
        }

        SpoolBox spoolBox = ObjectPoolingEverything.GetFromPool<SpoolBox>(GameConstants.spoolBox);

        spoolBox.Faction = faction;
        spoolBox.IsLocked = false;

        spoolBox.ResetVisually();

        int index = 0;

        bool isFound = false;

        for (int i = 0; i < spoolBoxs.Length; i++)
        {
            if (spoolBoxs[i] == null)
            {
                spoolBoxs[i] = spoolBox;

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

        ResetSpoolBoxSlot();

        Vector3 destination = GetSpoolBoxNextPosition(spoolBox, index);

        spoolBox.transform.localPosition = destination.ChangeX(getInDirection == Direction.Right ? -10 : 10);

        // destination.x = (-(maxSpoolBox - 1) / 2f + index) * _spoolBoxDistance;
        // destination.z = -0.001f * index;

        spoolBox.IsInTransition = true;

        _tweens.Add(Tween.LocalPosition(spoolBox.transform, destination, duration: 0.4f).OnComplete(() =>
        {
            spoolBox.IsInTransition = false;

            MoveFromPiecePortToSpoolBox(spoolBox);

            spoolBoxSpawnedEvent?.Invoke(spoolBox);
        }));

        // if (_isSpawnSpoolBoxFirstTimeInLevel)
        // {
        //     spoolBox.transform.localPosition = destination;

        //     MoveFromPiecePortToSpoolBox(spoolBox);
        // }
        // else
        // {
        //     spoolBox.IsInTransition = true;

        //     _tweens.Add(Tween.LocalPosition(spoolBox.transform, destination, duration: 0.5f).OnComplete(() =>
        //     {
        //         spoolBox.IsInTransition = false;

        //         MoveFromPiecePortToSpoolBox(spoolBox);

        //         spoolBoxSpawnedEvent?.Invoke(spoolBox);
        //     }));
        // }

        void ResetSpoolBoxSlot()
        {
            for (int i = 0; i < spoolBox.SpoolBoxSlots.Length; i++)
            {
                SpoolBoxSlot spoolBoxSlot = spoolBox.SpoolBoxSlots[i];

                if (spoolBoxSlot.IsFilled)
                {
                    if (spoolBoxSlot.Piece != null)
                    {
                        spoolBoxSlot.Piece.gameObject.SetActive(false);

                        spoolBoxSlot.Piece = null;
                    }

                    spoolBoxSlot.IsFilled = false;
                }
            }
        }
    }

    // public void SpawnSpoolBox(SpoolBoxData spoolBoxData)
    // {

    // }

    private void SpawnAdsSpoolBox()
    {
        Vector3 destination;

        for (int i = 0; i < spoolBoxs.Length; i++)
        {
            if (spoolBoxs[i] == null)
            {
                SpoolBox spoolBox = ObjectPoolingEverything.GetFromPool<SpoolBox>(GameConstants.spoolBox);

                spoolBox.transform.position = new Vector3(10, _spoolBoxPositionY, spoolBox.transform.position.z);

                spoolBox.Lock();

                int index = i;

                destination = GetSpoolBoxNextPosition(spoolBox, index);
                // destination.x = (-(maxSpoolBox - 1) / 2f + index) * _spoolBoxDistance;
                // destination.z = -0.001f * index;

                _tweens.Add(Tween.LocalPosition(spoolBox.transform, destination, duration: 0.4f)
                .OnComplete(() =>
                {
                    spoolBox.SpoolBoxServiceLocator.spoolBoxUI.SetUnlockByAdsButtonPosition();
                }));

                spoolBoxs[i] = spoolBox;

                break;
            }
        }
    }

    private void SpawnAdsSpoolBoxes()
    {
        for (int i = 0; i < spoolBoxs.Length; i++)
        {
            if (spoolBoxs[i] == null)
            {
                int index = i;

                SpoolBox spoolBox = ObjectPoolingEverything.GetFromPool<SpoolBox>(GameConstants.spoolBox);

                spoolBox.transform.position = new Vector3(10, _spoolBoxPositionY, spoolBox.transform.position.z);

                spoolBox.Lock();

                Vector3 destination = GetSpoolBoxNextPosition(spoolBox, index);

                // destination.x = (-(maxSpoolBox - 1) / 2f + index) * _spoolBoxDistance;
                // destination.z = -0.001f * index;

                if (_isSpawnSpoolBoxFirstTimeInLevel)
                {
                    spoolBox.transform.localPosition = destination;

                    spoolBox.SpoolBoxServiceLocator.spoolBoxUI.SetUnlockByAdsButtonPosition();
                }
                else
                {
                    _tweens.Add(Tween.LocalPosition(spoolBox.transform, destination, duration: 0.4f)
                    .OnComplete(() =>
                    {
                        spoolBox.SpoolBoxServiceLocator.spoolBoxUI.SetUnlockByAdsButtonPosition();
                    }));
                }

                spoolBoxs[i] = spoolBox;
            }
        }

        _isSpawnSpoolBoxFirstTimeInLevel = false;
    }

    public void UnlockAllSpoolBoxes()
    {
        int totalBoxUnlocked = 0;

        for (int i = 0; i < spoolBoxs.Length; i++)
        {
            if (spoolBoxs[i] == null)
            {
                continue;
            }

            if (spoolBoxs[i].IsLocked)
            {
                spoolBoxs[i].SpoolBoxServiceLocator.spoolBoxUI.Unlock();

                totalBoxUnlocked++;

                if (totalBoxUnlocked == 2)
                {
                    break;
                }
            }
        }
    }
    #endregion

    private void MoveFromPiecePortToSpoolBox(SpoolBox spoolBox)
    {
        // avoid too fast action
        enableBoosterEvent?.Invoke(false);

        bool isFound = false;

        for (int i = 0; i < piecePorts.Count; i++)
        {
            BasePiece piece = piecePorts[i].Piece;

            if (piece == null)
            {
                continue;
            }

            if (piece.IsDone || piece.IsMoving)
            {
                continue;
            }

            if (piecePorts[i].IsFilled && piece.Faction == spoolBox.Faction)
            {
                for (int k = 0; k < spoolBox.SpoolBoxSlots.Length; k++)
                {
                    if (!spoolBox.SpoolBoxSlots[k].IsFilled)
                    {
                        int piecePortIndex = i;
                        int spoolBoxSlotIndex = k;

                        spoolBox.SpoolBoxSlots[spoolBoxSlotIndex].Fill(piecePorts[piecePortIndex].Piece);

                        piecePorts[piecePortIndex].IsFilled = false;

                        Yarn2D yarn2D = ObjectPoolingEverything.GetFromPool<Yarn2D>(GameConstants.yard2D);

                        yarn2D.Rewind(piece.Faction, piecePorts[i], spoolBox.SpoolBoxSlots[spoolBoxSlotIndex],
                            onCompleteAction: () =>
                            {
                                spoolBox.SpoolBoxSlots[spoolBoxSlotIndex].CompleteFill();

                                piece.InvokePieceLoosenedEvent();
                            }
                        );

                        piece.IsDone = true;

                        piecePorts[i].SmoothSetDissolveHeight(1, 0);

                        if (!isFound)
                        {
                            isFound = true;
                        }

                        break;
                    }
                }
            }
        }

        spoolBoxesObserver.NumScrewInScrewPorts = GetNumPiecesInPiecePorts();

        if (!isFound)
        {
            enableBoosterEvent?.Invoke(true);
        }
    }

    private void MoveSinglePieceFromHoleToSpoolBox(BasePiece piece)
    {
        if (!piece.SpoolBoxSlot.IsAlternative)
        {
            return;
        }

        if (piece.IsDone || piece.IsMoving)
        {
            return;
        }

        bool isFound = false;

        for (int i = 0; i < spoolBoxs.Length; i++)
        {
            SpoolBox spoolBox = spoolBoxs[i];

            if (spoolBox == null)
            {
                continue;
            }

            if (spoolBox.Faction != piece.Faction || spoolBox.IsLocked || spoolBox.IsInTransition)
            {
                continue;
            }

            for (int k = 0; k < spoolBox.SpoolBoxSlots.Length; k++)
            {
                if (!spoolBox.SpoolBoxSlots[k].IsFilled)
                {
                    int spoolBoxSlotIndex = k;

                    spoolBox.SpoolBoxSlots[spoolBoxSlotIndex].Fill(piece);

                    piece.SpoolBoxSlot.IsFilled = false;

                    Yarn2D yarn2D = ObjectPoolingEverything.GetFromPool<Yarn2D>(GameConstants.yard2D);

                    yarn2D.Rewind(piece.Faction, piece.SpoolBoxSlot, spoolBox.SpoolBoxSlots[spoolBoxSlotIndex],
                        onCompleteAction: () =>
                        {
                            spoolBox.SpoolBoxSlots[spoolBoxSlotIndex].CompleteFill();

                            piece.InvokePieceLoosenedEvent();
                        }
                    );

                    piece.IsDone = true;

                    piece.SpoolBoxSlot.SmoothSetDissolveHeight(1, 0);

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

    public Dictionary<GameFaction, int> GetPiecePortAvailableByFaction()
    {
        Dictionary<GameFaction, int> piecePortAvailableByFaction = new Dictionary<GameFaction, int>();

        for (int i = 0; i < GameConstants.SCREW_FACTION.Length; i++)
        {
            piecePortAvailableByFaction.Add(GameConstants.SCREW_FACTION[i], 0);
        }

        for (int i = 0; i < spoolBoxs.Length; i++)
        {
            if (spoolBoxs[i] == null || spoolBoxs[i].IsLocked)
            {
                continue;
            }

            GameFaction faction = spoolBoxs[i].Faction;

            for (int j = 0; j < spoolBoxs[i].SpoolBoxSlots.Length; j++)
            {
                SpoolBoxSlot spoolBoxSlot = spoolBoxs[i].SpoolBoxSlots[j];

                if (!spoolBoxSlot.IsFilled || spoolBoxs[i].IsInTransition)
                {
                    piecePortAvailableByFaction[faction]++;
                }
            }
        }

        return piecePortAvailableByFaction;
    }

    #region BOOSTER
    private void LoosenPieceOnObjectBroken(BasePiece piece)
    {
        // try
        // {
        //     SpoolBoxSlot spoolBoxSlot = CheckAvailableSpoolBoxes(piece.Faction, isIncludePiecePorts: false);

        //     if (spoolBoxSlot != null)
        //     {
        //         loosePieceWithoutCheckingObstaclesEvent?.Invoke(piece.PieceId, piece.Faction, spoolBoxSlot);
        //     }
        //     else
        //     {
        //         piece.ForceUnpiece();
        //     }
        // }
        // catch (System.Exception)
        // {

        // }
    }

    public void AddMorePiecePort()
    {
        SpoolBoxSlot addedSpoolBoxSlot = ObjectPoolingEverything.GetFromPool<SpoolBoxSlot>(GameConstants.spoolBoxSlot);

        piecePorts.Add(addedSpoolBoxSlot);

        addedSpoolBoxSlot.transform.SetParent(piecePorts[0].transform.parent);

        addedSpoolBoxSlot.transform.localRotation = Quaternion.Euler(Vector3.zero);
        addedSpoolBoxSlot.transform.localScale = piecePorts[0].transform.localScale;

        Vector3 position = piecePorts[0].transform.position;

        // addedSpoolBoxSlot.transform.position = position + new Vector3(10, 0, 0);
        addedSpoolBoxSlot.transform.localScale = Vector3.zero;


        float targetScale = 1f;
        if (piecePorts[0] != null)
        {
            targetScale = piecePorts[0].transform.localScale.x;
        }
        else
        {
            targetScale = GamePlayConst.spoolPortScale;
        }

        for (int i = 0; i < piecePorts.Count; i++)
        {
            int index = i;

            position.x = (-(piecePorts.Count - 1) / 2f + i) * GamePlayConst.spoolXDistanceScale * piecePorts[0].transform.lossyScale.x;

            _tweens.Add(Tween.Position(piecePorts[i].transform, position, duration: 0.3f)
            .OnComplete(() =>
            {
                if (index == piecePorts.Count - 1)
                {
                    _tweens.Add(Tween.Scale(addedSpoolBoxSlot.transform, Vector3.one * targetScale, duration: 0.3f));
                }
            }));
        }

        GamePersistentVariable.numHolesAdded++;
    }

    private void ClearAllPiecePorts()
    {
        int numPieceCleared = 0;

        for (int i = 0; i < piecePorts.Count; i++)
        {
            if (piecePorts[i].IsFilled)
            {
                if (piecePorts[i].Piece.IsMoving)
                {
                    continue;
                }

                piecePorts[i].IsFilled = false;

                piecePorts[i].Piece.ThrowOutside();

                Yarn2D yarn2D = ObjectPoolingEverything.GetFromPool<Yarn2D>(GameConstants.yard2D);

                BasePiece cachePiece = piecePorts[i].Piece;

                yarn2D.Rewind(piecePorts[i].Piece.Faction, piecePorts[i], outsideSpoolSlot,
                    onCompleteAction: () =>
                    {
                        cachePiece.PieceServiceLocator.pieceProperty.IsRewinding = false;

                        // spoolBox.SpoolBoxSlots[spoolBoxSlotIndex].CompleteFill();

                        // piece.InvokePieceLoosenedEvent();
                    }
                );

                piecePorts[i].Piece = null;

                numPieceCleared++;
            }
        }

        spoolBoxesObserver.NumScrewInScrewPorts = GetNumPiecesInPiecePorts();

        piecePortsClearedEvent?.Invoke(numPieceCleared);
    }

    private int GetNumPiecesInPiecePorts()
    {
        int numPiece = 0;

        for (int i = 0; i < piecePorts.Count; i++)
        {
            if (piecePorts[i].IsFilled)
            {
                numPiece++;
            }
        }

        return numPiece;
    }

    private void ShakeAllPiecePorts()
    {
        if (_isPiecePortsShaking)
        {
            return;
        }

        for (int i = 0; i < piecePorts.Count; i++)
        {
            bool isLast = i == piecePorts.Count - 1;

            _tweens.Add(Tween.PunchScale(piecePorts[i].transform, strength: 0.3f * Vector3.one, duration: 0.3f)
            .OnComplete(() =>
            {
                if (isLast)
                {
                    _isPiecePortsShaking = false;
                }
            }));

            SoundManager.Instance.PlaySoundLoosenScrewFail();
        }

        _isPiecePortsShaking = true;
    }
    #endregion

    #region UTIL
    private Vector3 GetSpoolBoxNextPosition(SpoolBox spoolBox, int nextIndex)
    {
        Vector3 destination = spoolBox.transform.position;

        destination.x = (-(maxSpoolBox - 1) / 2f + nextIndex) * _spoolBoxDistance;
        destination.y = _spoolBoxPositionY;
        destination.z = -0.001f * nextIndex;

        return destination;
    }
    #endregion
}
