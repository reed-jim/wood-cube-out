using System;
using NamPhuThuy.GamePlay;
using UnityEngine;
using static GameEnum;

public class GameStateLose : BaseGameState
{
    private GameStateMachine _gameStateMachine;

    public GameStateLose(GameStateMachine gameStateMachine)
    {
        _gameStateMachine = gameStateMachine;
    }

    public override GameState GameState { get => GameState.Lose; }

    public static event Action loseLevelEvent;
    public static event Action<BoosterType> showRevivePopupEvent;

    public override bool CanTransitionTo()
    {
        if (_gameStateMachine.CurrentState == GameState.Win || _gameStateMachine.CurrentState == GameState.Lose)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public override void Enter()
    {
        if (TileManager.Ins.NumLockedTileSlotContainer == 0)
        {
            showRevivePopupEvent?.Invoke(BoosterType.ClearScrewPorts);
        }
        else
        {
            showRevivePopupEvent?.Invoke(BoosterType.UnlockScrewBox);
        }
    }

    public override void Exit()
    {

    }

    public override void Update()
    {

    }
}
