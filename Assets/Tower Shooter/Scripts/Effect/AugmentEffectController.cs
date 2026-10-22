using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class AugmentEffectController : MonoBehaviour
{
    [SerializeField] private Image effectImage; // Separate UI Image layered on top of the node
    private Material matInstance;

    private void Awake()
    {
        matInstance = new Material(effectImage.material);
        effectImage.material = matInstance;
        matInstance.SetFloat("_TriggerProgress", 0.0f); // Hidden by default
    }

    public void PlayEffect(int effectType, float duration = 0.5f)
    {
        matInstance.SetFloat("_EffectType", effectType);
        matInstance.SetFloat("_TriggerProgress", 0.0f);

        // Animate progress from 0 to 1, then reset to 0 so it vanishes
        DOTween.To(() => matInstance.GetFloat("_TriggerProgress"),
                   x => matInstance.SetFloat("_TriggerProgress", x),
                   1.0f, duration)
               .SetEase(Ease.OutQuad)
               .OnComplete(() =>
               {
                   matInstance.SetFloat("_TriggerProgress", 0.0f);
               });
    }

    #region Debug
    [ContextMenu("PlayEffectOne")]
    public void PlayEffectOne()
    {
        PlayEffect(1);
    }

    [ContextMenu("PlayEffectTwo")]
    public void PlayEffectTwo()
    {
        PlayEffect(2);
    }

    [ContextMenu("PlayEffectThree")]
    public void PlayEffectThree()
    {
        PlayEffect(3);
    }
    #endregion
}