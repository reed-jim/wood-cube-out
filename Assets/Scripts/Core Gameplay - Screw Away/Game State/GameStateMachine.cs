using System;
using UnityEngine;

public class GameStateMachine : MonoBehaviour
{
    private BaseGameState currentState;

    public GameState CurrentState
    {
        get => currentState.GameState;
    }

    private void Awake()
    {
        LevelLoader.startLevelEvent += EnterPlayingState;
        ScrewManager.winLevelEvent += EnterWinState;
        ScrewBoxManager.loseLevelEvent += EnterLoseState;

        ChangeState(new GameStatePlaying());
    }

    void OnDestroy()
    {
        LevelLoader.startLevelEvent -= EnterPlayingState;
        ScrewManager.winLevelEvent -= EnterWinState;
        ScrewBoxManager.loseLevelEvent -= EnterLoseState;
    }

    // void Update()
    // {
    //     if (currentState != null)
    //     {
    //         currentState.Update();
    //     }
    // }

    public void ChangeState(BaseGameState newState)
    {
        if (currentState != null)
        {
            currentState.Exit();
        }

        currentState = newState;

        currentState.Enter();
    }

    private void EnterPlayingState()
    {
        ChangeState(new GameStatePlaying());
    }

    private void EnterWinState()
    {
        ChangeState(new GameStateWin(this));
    }

    private void EnterLoseState()
    {
        ChangeState(new GameStateLose(this));
    }
}
