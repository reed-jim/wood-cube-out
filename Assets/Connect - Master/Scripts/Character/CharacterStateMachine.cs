using UnityEngine;

public class CharacterStateMachine : MonoBehaviour
{
    public CharacterServiceLocator characterServiceLocator;

    public SeekingState seekingState;
    public AttackState attackState;

    public ICharacterState currentState;

    void Start()
    {
        ChangeState(seekingState);
    }

    void Update()
    {
        currentState.Update();
    }

    public void ChangeState(ICharacterState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }
}

public class SeekingState : ICharacterState
{
    private BaseEnemy enemy;
    private CharacterStateMachine characterStateMachine;

    public SeekingState(BaseEnemy enemy)
    {
        this.enemy = enemy;
        characterStateMachine = enemy.characterServiceLocator.characterStateMachine;
    }

    public void Enter()
    {

    }

    public void Update()
    {
        if (Vector3.Distance(enemy.transform.position, CharacterManager.Ins.mainCharacter.transform.position) > 1)
        {

        }
        else
        {
            characterStateMachine.ChangeState(characterStateMachine.attackState);
        }
    }

    public void Exit()
    {

    }
}

public class AttackState : ICharacterState
{
    private BaseEnemy enemy;
    private CharacterStateMachine characterStateMachine;

    public AttackState(BaseEnemy enemy)
    {
        this.enemy = enemy;
        characterStateMachine = enemy.characterServiceLocator.characterStateMachine;
    }

    public void Enter()
    {

    }

    public void Update()
    {

    }

    public void Exit()
    {

    }
}

public interface ICharacterState
{
    public void Enter();
    public void Update();
    public void Exit();
}
