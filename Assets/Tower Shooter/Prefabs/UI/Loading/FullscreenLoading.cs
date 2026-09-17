using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FullscreenLoading : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public Image progress;
    public TMP_Text progressText;

    [Header("Customize")]
    public float duration;

    void Start()
    {
        Reset();
        Load();
    }

    private void Reset()
    {
        canvasGroup.alpha = 1;
        progress.fillAmount = 0;
        progressText.text = $"0%";
    }

    private void Load()
    {
        Tween.Custom(0, 1, duration: duration, onValueChange: newVal =>
        {
            progress.fillAmount = newVal;
            progressText.text = $"{newVal * 100:F0}%";
        })
        .OnComplete(() =>
        {
            Tween.Alpha(canvasGroup, 0, duration: 0.3f).OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
        });
    }
}
