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

    private Tween _progressTween;

    public void SetProgress(float progress)
    {
        if (_progressTween.isAlive)
        {
            _progressTween.Stop();
        }

        if (!gameObject.activeInHierarchy)
        {
            SaferioTween.AlphaShow(canvasGroup);
        }

        fill.fillAmount = progress;

        _progressTween = Tween.Custom(lazyFill.fillAmount, progress, duration: 1f, onValueChange: newVal =>
        {
            lazyFill.fillAmount = newVal;
        })
        .OnComplete(() =>
        {

        });
    }
}
