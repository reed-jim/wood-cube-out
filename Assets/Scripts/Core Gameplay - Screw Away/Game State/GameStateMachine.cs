using System;
using UnityEngine;

public class GameStateMachine : MonoBehaviour
{
    private BaseGameState currentState;

    [SerializeField] private ScrewBoxesObserver screwBoxesObserver;

    public GameState CurrentState
    {
        get => currentState.GameState;
    }

    public ScrewBoxesObserver ScrewBoxesObserver
    {
        get => screwBoxesObserver;
    }

    private void Awake()
    {
        LevelLoader.startLevelEvent += EnterPlayingState;
        ScrewManager.winLevelEvent += EnterWinState;
        ScrewBoxManager.loseLevelEvent += EnterLoseState;
        RevivePopup.reviveEvent += EnterPlayingState;

        ChangeState(new GameStatePlaying());
    }

    void OnDestroy()
    {
        LevelLoader.startLevelEvent -= EnterPlayingState;
        ScrewManager.winLevelEvent -= EnterWinState;
        ScrewBoxManager.loseLevelEvent -= EnterLoseState;
        RevivePopup.reviveEvent -= EnterPlayingState;
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
        if (!newState.CanTransitionTo())
        {
            return;
        }

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
