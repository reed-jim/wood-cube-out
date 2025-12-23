using System;
using NamPhuThuy.Common;
using Unity.VisualScripting;
using UnityEngine;

public class GameStateWin : BaseGameState
{
    private GameStateMachine _gameStateMachine;

    public GameStateWin(GameStateMachine gameStateMachine)
    {
        _gameStateMachine = gameStateMachine;
    }

    public override GameState GameState { get => GameState.Win; }

    public static event Action winLevelEvent;

    public override bool CanTransitionTo()
    {
        if (_gameStateMachine.CurrentState == GameState.Lose)
        {
            DebugLogger.Log(message: $"Cannot transition to Win state from Lose state.");
            return false;
        }

        if (_gameStateMachine.CurrentState == GameState.Win)
        {
            DebugLogger.Log(message: $"Cannot transition to Win state from Lose state.");
            return false;
        }
        
        return true;
    }

    public override void Enter()
    {
        // DebugLogger.Log(message: $"");
        winLevelEvent?.Invoke();
    }

    public override void Exit()
    {

    }

    public override void Update()
    {

    }
}
