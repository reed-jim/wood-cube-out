using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;
using UnityEngine.UI;

public class LazyHealthBar : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image fill;
    [SerializeField] private Image lazyFill;
    [SerializeField] private Image background;

    [Header("Customize")]
    public float fillDuration = 0.3f;
    public float lazyFillDuration = 1f;

    private Tween _progressTween;
    private Tween _lazyProgressTween;

    public void SetProgress(float progress)
    {
        if (_progressTween.isAlive)
        {
            _progressTween.Stop();
        }

        if (_lazyProgressTween.isAlive)
        {
            _lazyProgressTween.Stop();
        }

        if (!gameObject.activeInHierarchy)
        {
            SaferioTween.AlphaShow(canvasGroup);
        }

        _progressTween = Tween.Custom(fill.fillAmount, progress, duration: fillDuration, onValueChange: newVal =>
        {
            fill.fillAmount = newVal;
        });

        _lazyProgressTween = Tween.Custom(lazyFill.fillAmount, progress, duration: lazyFillDuration, onValueChange: newVal =>
        {
            lazyFill.fillAmount = newVal;
        })
        .OnComplete(() =>
        {

        });
    }
}
