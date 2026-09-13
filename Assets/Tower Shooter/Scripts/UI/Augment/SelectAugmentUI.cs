using PrimeTween;
using UnityEngine;

public class SelectAugmentUI : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    public void Show()
    {
        canvasGroup.gameObject.SetActive(true);

        Tween.Alpha(canvasGroup, 1, duration: 0.3f);
    }

    public void Hide()
    {
        Tween.Alpha(canvasGroup, 0, duration: 0.3f).OnComplete(() =>
        {
            canvasGroup.gameObject.SetActive(false);
        });
    }
}
