using System;
using NamPhuThuy.Common;
using PrimeTween;
using UnityEngine;
using static GameEnum;

public class GameStateMachine : MonoBehaviour
{
    private BaseGameState currentState;

    [SerializeField] private ScrewBoxesObserver screwBoxesObserver;

    [SerializeField] private float delayToEnableInputAfterRevived;
    [SerializeField] private GameState currentStateVar;

    public GameState CurrentState
    {
        get => currentState.GameState;
    }

    public ScrewBoxesObserver ScrewBoxesObserver
    {
        get => screwBoxesObserver;
    }

    public static event Action<bool> enableInputEvent;

    private void Awake()
    {
        YarnLevelLoader.startLevelEvent += EnterPlayingState;
        PieceManager.winLevelEvent += EnterWinState;
        SpoolManager.loseLevelEvent += EnterLoseState;
        RevivePopup.reviveEvent += OnRevived;

        ChangeState(new GameStatePlaying());
    }

    void OnDestroy()
    {
        YarnLevelLoader.startLevelEvent -= EnterPlayingState;
        PieceManager.winLevelEvent -= EnterWinState;
        SpoolManager.loseLevelEvent -= EnterLoseState;
        RevivePopup.reviveEvent -= OnRevived;
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
        // DebugLogger.Log(message: $"currentState: {currentState}, newState: {newState}", context: this);
        if (!newState.CanTransitionTo())
        {
            DebugLogger.Log(message: $"Cannot transition to state: {newState.GameState}", context: this);
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

        Tween.Delay(delayToEnableInputAfterRevived).OnComplete(() =>
        {
            enableInputEvent?.Invoke(true);
        });
    }

    private void EnterWinState()
    {
        // DebugLogger.Log(context: this);
        ChangeState(new GameStateWin(this));
    }

    private void EnterLoseState()
    {
        enableInputEvent?.Invoke(false);

        ChangeState(new GameStateLose(this));
    }

    private void OnRevived(BoosterType boosterType)
    {
        GamePersistentVariable.isReviveState = false;
        EnterPlayingState();
    }
}
