using System;
using System.Collections;
using Animancer;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    [SerializeField] private AnimancerComponent animancerComponent;
    [SerializeField] private AnimationClip attackAnimation;
    public LinearMixerTransition locomotionMixer;

    public void UpdateMixer(float value)
    {
        locomotionMixer.State.Parameter = value;
    }

    public void PlayAnimation(Action onComplete)
    {
        AnimancerState state = animancerComponent.Play(attackAnimation);

        state.Events(this).OnEnd += onComplete;
    }
}
