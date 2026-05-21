using System;
using System.Collections;
using Animancer;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    [SerializeField] private AnimancerComponent animancerComponent;
    // [SerializeField] private AnimationClip attackAnimation;
    public MixerTransition2D locomotionMixer;

    private Vector2 inputVector;
    private Vector3 moveDirection;
    private MixerState<Vector2> mixerState;

    public float runPower;
    private Vector2 oldInputVector;

    void Start()
    {
        mixerState = (MixerState<Vector2>)animancerComponent.Play(locomotionMixer);
    }

    void Update()
    {
        HandleInput();
        HandleAnimation();
    }

    private void HandleInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        inputVector = new Vector2(h, v);

        if (h != 0 || v != 0)
        {
            runPower += 0.01f;
        }
        else
        {
            runPower -= 0.01f;

            inputVector = oldInputVector;
        }

        runPower = Mathf.Clamp01(runPower);

        oldInputVector = inputVector;

        inputVector = runPower * Vector2.ClampMagnitude(inputVector, 1f);
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

        mixerState.Parameter = animationVector;
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
