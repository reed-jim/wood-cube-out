using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class AugmentEffectController : MonoBehaviour
{
    [SerializeField] private Image effectImage; // Separate UI Image layered on top of the node
    private Material matInstance;

    private void Awake()
    {
        if (effectImage != null && effectImage.material != null)
        {
            matInstance = new Material(effectImage.material);
            effectImage.material = matInstance;
            ResetAllEffects();
        }
    }

    private void ResetAllEffects()
    {
        matInstance.SetFloat("_TriggerProgress", 0.0f);
        matInstance.SetFloat("_Effect1Active", 0.0f);
        matInstance.SetFloat("_Effect2Active", 0.0f);
        matInstance.SetFloat("_Effect3Active", 0.0f);
        matInstance.SetFloat("_Effect4Active", 0.0f);
        matInstance.SetFloat("_Effect5Active", 0.0f);
    }

    public void PlayEffect(int effectIndex, float duration = 0.6f)
    {
        // Reset progress and turn off all effects first
        ResetAllEffects();

        // Enable only the chosen effect flag (1 through 5)
        string targetProp = "_Effect" + Mathf.Clamp(effectIndex, 1, 5) + "Active";
        matInstance.SetFloat(targetProp, 1.0f);

        // Animate progress from 0 to 1, then clean up on complete
        DOTween.To(() => matInstance.GetFloat("_TriggerProgress"),
                   x => matInstance.SetFloat("_TriggerProgress", x),
                   1.0f, duration)
               .SetEase(Ease.OutQuad)
               .OnComplete(ResetAllEffects);
    }

    #region Debug Context Menus
    [ContextMenu("Play Effect 1: Hyper-Plasma Shockwave")]
    public void PlayEffectOne() => PlayEffect(1);

    [ContextMenu("Play Effect 2: Quantum Singularity Rift")]
    public void PlayEffectTwo() => PlayEffect(2);

    [ContextMenu("Play Effect 3: Tachyon Overload Surge")]
    public void PlayEffectThree() => PlayEffect(3);

    [ContextMenu("Play Effect 4: Nanite Matrix Shock")]
    public void PlayEffectFour() => PlayEffect(4);

    [ContextMenu("Play Effect 5: Supernova Core Eruption")]
    public void PlayEffectFive() => PlayEffect(5);
    #endregion
}