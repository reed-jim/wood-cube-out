using System;
using UnityEngine;
using static GameEnum;

[Serializable]
public class ScrewData
{
    [SerializeField] private int _screwId;
    [SerializeField] private GameFaction _faction;

    // [SerializeField] private bool _isDone;
    // [SerializeField] private bool _isInScrewPort;
    // [SerializeField] private bool _isDestroyed;

    private const int IsMovingBit = 1 << 0;
    private const int IsDoneBit = 1 << 1;
    private const int IsOutsideBit = 1 << 2;
    private const int IsInScrewPortBit = 1 << 3;
    private const int IsActiveBit = 1 << 4;

    public int ScrewId
    {
        get => _screwId;
        set => _screwId = value;
    }

    public GameFaction Faction
    {
        get => _faction;
        set => _faction = value;
    }

    public int stateFlags;

    // public bool IsDone
    // {
    //     get => _isDone;
    //     set => _isDone = value;
    // }

    // public bool IsInScrewPort
    // {
    //     get => _isInScrewPort;
    //     set => _isInScrewPort = value;
    // }

    // public bool IsDestroyed
    // {
    //     get => _isDestroyed;
    //     set => _isDestroyed = value;
    // }

    public void SetState(bool isDone, bool isMoving, bool inOutside, bool isInScrewPort, bool isActive)
    {
        stateFlags = 0;

        if (isMoving) stateFlags |= IsMovingBit;
        if (isDone) stateFlags |= IsDoneBit;
        if (inOutside) stateFlags |= IsOutsideBit;
        if (isInScrewPort) stateFlags |= IsInScrewPortBit;
        if (isActive) stateFlags |= IsActiveBit;
    }

    public bool IsMoving()
    {
        return (stateFlags & IsMovingBit) != 0;
    }

    public bool IsDone()
    {
        return (stateFlags & IsDoneBit) != 0;
    }

    public bool IsOutside()
    {
        return (stateFlags & IsOutsideBit) != 0;
    }

    public bool IsInScrewPort()
    {
        return (stateFlags & IsInScrewPortBit) != 0;
    }
}
