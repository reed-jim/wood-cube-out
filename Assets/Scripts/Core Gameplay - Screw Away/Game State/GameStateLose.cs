using System;
using UnityEngine;

public class GameStateLose : BaseGameState
{
    private GameStateMachine _gameStateMachine;

    public GameStateLose(GameStateMachine gameStateMachine)
    {
        _gameStateMachine = gameStateMachine;
    }

    public override GameState GameState { get => GameState.Lose; }

    public static event Action loseLevelEvent;
    public static event Action showRevivePopupEvent;

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
        if (_gameStateMachine.ScrewBoxesObserver.NumLockedScrewBoxes == 0)
        {
            loseLevelEvent?.Invoke();
        }
        else
        {
            showRevivePopupEvent?.Invoke();
        }
    }

    public override void Exit()
    {

    }

    public override void Update()
    {

    }
}
