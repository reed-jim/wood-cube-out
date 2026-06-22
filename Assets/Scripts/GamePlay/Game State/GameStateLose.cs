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
        bool isTimeOut = TileManager.Ins != null && 
                         TileManager.Ins.levelItem != null && 
                         TileManager.Ins.levelItem.timeLimit > 0 && 
                         TileManager.Ins.currentLevelTime <= 0;

        if (isTimeOut)
        {
            loseLevelEvent?.Invoke();
        }
        else
        {
            if (TileManager.Ins != null && TileManager.Ins.NumLockedTileSlotContainer == 0)
            {
                showRevivePopupEvent?.Invoke(BoosterType.ClearScrewPorts);
            }
            else
            {
                showRevivePopupEvent?.Invoke(BoosterType.UnlockScrewBox);
            }
        }
    }

    public override void Exit()
    {

    }

    public override void Update()
    {

    }
}
