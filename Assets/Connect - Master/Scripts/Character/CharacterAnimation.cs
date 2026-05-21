using System;
using System.Collections;
using Animancer;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    [SerializeField] private Rigidbody characterRigidbody;
    [SerializeField] private AnimancerComponent animancerComponent;
    // [SerializeField] private AnimationClip attackAnimation;

    [SerializeField] private Transform cameraTransform;

    public MixerTransition2D locomotionMixer;

    private Vector2 inputVector;
    private Vector3 moveDirection;
    private MixerState<Vector2> mixerState;

    public float runPower;
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
        HandleAnimation();
    }

    private void HandleInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        inputVector = new Vector2(h, v);

        if (h != 0 || v != 0)
        {
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
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection =
            camForward * inputVector.y +
            camRight * inputVector.x;

        desiredVelocity = speed * moveDirection;

        Vector3 currentVelocity = characterRigidbody.linearVelocity;

        Vector3 horizontalVelocity =
            new Vector3(
                currentVelocity.x,
                0,
                currentVelocity.z);

        Vector3 velocityChange =
            desiredVelocity - horizontalVelocity;

        characterRigidbody.AddForce(
            velocityChange,
            ForceMode.VelocityChange);
    }

    private void HandleAnimation()
    {
        // bool isRunning = Input.GetKey(KeyCode.LeftShift);

        float vertical = inputVector.y;
        float horizontal = inputVector.x;

        // // Increase forward parameter if running
        // if (isRunning && vertical > 0)
        // {
        //     vertical *= 2f;
        // }

        Vector2 animationVector =
            new Vector2(horizontal, vertical);




        Vector3 velocity = characterRigidbody.linearVelocity;
        velocity.y = 0;

        // // Convert world velocity to local velocity
        // Vector3 localVelocity =
        //     transform.InverseTransformDirection(velocity);

        Vector2 targetAnimVelocity =
            new Vector2(
                velocity.x,
                velocity.z);

        mixerState.Parameter = targetAnimVelocity;
    }

    // void Update()
    // {
    //     if (Input.GetKey(KeyCode.UpArrow))
    //     {
    //         value += 0.01f;
    //     }
    //     else
    //     {
    //         value -= 0.01f;
    //     }

    //     value = Mathf.Clamp01(value);

    //     UpdateMixer(value);
    // }

    public void UpdateMixer(float value)
    {
        // locomotionMixer.State.Parameter = value;
    }

    public void PlayAnimation(Action onComplete)
    {
        // AnimancerState state = animancerComponent.Play(attackAnimation);

        // state.Events(this).OnEnd += onComplete;
    }
}
