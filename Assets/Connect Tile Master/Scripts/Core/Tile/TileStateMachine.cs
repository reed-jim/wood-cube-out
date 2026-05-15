using UnityEngine;

public class TileStateMachine : MonoBehaviour
{
    public ITileState CurrentState { get; private set; }

    public void ChangeState(ITileState newState, object data = null)
    {
        CurrentState?.Exit();

        CurrentState = newState;

        CurrentState?.Enter(data);
    }

    public void Update()
    {
        CurrentState?.Update();
    }
}

public class OnBoardState : ITileState
{
    public void Enter(object param = null) { }
    public void Update() { }
    public void Exit() { }
}

public class MoveToSlotState : ITileState
{
    public void Enter(object param = null)
    {
        TileManager.Ins.MoveTileToSlot((BaseTile)param, null);
    }

    public void Update() { }
    public void Exit() { }
}

public interface ITileState
{
    void Enter(object param = null);
    void Update();
    void Exit();
}
