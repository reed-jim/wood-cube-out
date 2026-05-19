using System;
using System.Collections;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [SerializeField] private string animationStateName = "Attack";

    public void PlayAnimation(Action onComplete = null)
    {
        StartCoroutine(PlayAnimationRoutine(onComplete));
    }

    private IEnumerator PlayAnimationRoutine(Action onComplete)
    {
        animator.Play(animationStateName);

        yield return null;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float animationLength = stateInfo.length;

        yield return new WaitForSeconds(animationLength);

        onComplete?.Invoke();
    }
}
