using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ScaleUpDownLoop : MonoBehaviour
{
    public float scaleUp = 1.1f;
    public float duration = 0.6f;

    private Tween loopTween;

    void OnEnable()
    {
        StartLoop();
    }

    void OnDisable()
    {
        KillTween();
    }

    void StartLoop()
    {
        // Avoid stacking tweens
        KillTween();

        transform.localScale = Vector3.one;

        loopTween = transform.DOScale(scaleUp, duration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    void KillTween()
    {
        if (loopTween != null && loopTween.IsActive())
        {
            loopTween.Kill();
            loopTween = null;
        }
    }
}
