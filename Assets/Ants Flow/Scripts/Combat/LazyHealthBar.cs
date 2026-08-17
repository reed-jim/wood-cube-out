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
    public float duration = 0.3f;
    public float lazyFillDurationRatio = 2f;
    public bool isLazy = true;

    private Tween _progressTween;
    private Tween _lazyProgressTween;

    void Awake()
    {
        if (!isLazy)
        {
            lazyFill.gameObject.SetActive(false);
        }
    }

    public void SetProgress(float progress)
    {
        SetProgress(progress, duration);
    }

    public void SetProgress(float progress, float duration)
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

        _progressTween = Tween.Custom(fill.fillAmount, progress, duration: duration, onValueChange: newVal =>
        {
            fill.fillAmount = newVal;
        });

        if (isLazy)
        {
            _lazyProgressTween = Tween.Custom(lazyFill.fillAmount, progress, duration: lazyFillDurationRatio * duration, onValueChange: newVal =>
            {
                lazyFill.fillAmount = newVal;
            })
            .OnComplete(() =>
            {

            });
        }
    }

    public void SetProgressImmediately(float progress)
    {
        fill.fillAmount = progress;
    }
}
