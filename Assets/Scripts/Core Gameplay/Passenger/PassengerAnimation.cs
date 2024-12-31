using UnityEngine;
using static GameEnum;

public class PassengerAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private void Awake()
    {
        Passenger.changeAnimationEvent += ChangeAnimation;
        PassengerQueue.changeAnimationEvent += ChangeAnimation;
    }

    private void OnDestroy()
    {
        Passenger.changeAnimationEvent -= ChangeAnimation;
        PassengerQueue.changeAnimationEvent -= ChangeAnimation;
    }

    private void ChangeAnimation(int instanceId, CharacterAnimationState characterAnimationState)
    {
        if (instanceId == gameObject.GetInstanceID())
        {
            animator.SetInteger(GameConstants.ANIMATION_STATE, (int)characterAnimationState);
        }
    }
}
