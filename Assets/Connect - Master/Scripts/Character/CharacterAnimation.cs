using System;
using System.Collections;
using Animancer;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    [Header("Component")]
    [SerializeField] private Rigidbody characterRigidbody;
    [SerializeField] private AnimancerComponent animancerComponent;
    // [SerializeField] private AnimationClip attackAnimation;

    [SerializeField] private Transform cameraTransform;

    public MixerTransition2D locomotionMixer;
    private MixerState<Vector2> mixerState;

    public float runPower;
    private Vector2 inputVector;
    private Vector2 oldInputVector;
    private Vector3 desiredVelocity;

    [Header("Movement")]
    public float speed;
    public float runPowerDelta;

    void Start()
    {
        mixerState = (MixerState<Vector2>)animancerComponent.Play(locomotionMixer);
    }

    void Update()
    {
        HandleInput();
        HandleMovement();
        // HandleAnimation();
    }

    private void HandleInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        inputVector = new Vector2(h, v);

        if (h != 0 || v != 0)
        {
            Debug.Log("inputVector " + inputVector);
            runPower += runPowerDelta;
        }
        else
        {
            runPower -= 3 * runPowerDelta;

            inputVector = oldInputVector;
        }

        runPower = Mathf.Clamp01(runPower);

        oldInputVector = inputVector;

        inputVector = runPower * Vector2.ClampMagnitude(inputVector, 1f);
    }

    private void HandleMovement()
    {
        // Vector3 camForward = cameraTransform.forward;
        // Vector3 camRight = cameraTransform.right;

        // camForward.y = 0;
        // camRight.y = 0;

        // camForward.Normalize();
        // camRight.Normalize();

        // Vector3 moveDirection =
        //     camForward * inputVector.y +
        //     camRight * inputVector.x;

        Vector3 moveDirection = new Vector3(inputVector.y, 0, inputVector.x);

        desiredVelocity = speed * moveDirection;
        desiredVelocity.y = 0;

        Vector3 currentVelocity = characterRigidbody.linearVelocity;

        Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);

        Vector3 velocityChange =
            desiredVelocity - horizontalVelocity;

        velocityChange.x = Mathf.Clamp(velocityChange.x, -10, 10);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -10, 10);

        // if (inputVector.x > 0 || inputVector.y > 0)
        // {
        //     Debug.Log($"HandleMovement {velocityChange} {desiredVelocity}");
        // }

        characterRigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    private void HandleAnimation()
    {
        Vector3 velocity = characterRigidbody.linearVelocity;
        velocity.y = 0;

        Vector2 targetAnimVelocity = new Vector2(velocity.x, velocity.z) / speed;

        mixerState.Parameter = targetAnimVelocity;
    }

    public void PlayAnimation(Action onComplete)
    {
        // AnimancerState state = animancerComponent.Play(attackAnimation);

        // state.Events(this).OnEnd += onComplete;
    }
}
