using System;
using BirdSort2;
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
    public static event Action<GiftType> showRevivePopupEvent;

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
        if (GamePlayController_BirdSort.Instance.level.screwBoxManager.NumLockedScrewBoxes == 0)
        {
            // loseLevelEvent?.Invoke();
            showRevivePopupEvent?.Invoke(GiftType.ClearHole);
        }
        else
        {
            showRevivePopupEvent?.Invoke(GiftType.AddBox);
        }
    }

    public override void Exit()
    {

    }

    public override void Update()
    {

    }
}
