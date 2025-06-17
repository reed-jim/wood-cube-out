using System;
using PrimeTween;
using UnityEngine;

public class AdsBreakScreen : SaferioTweenBehaviour
{
    public static AdsBreakScreen Instance;

    [SerializeField] private RectTransform container;
    [SerializeField] private CanvasGroup canvasGroup;

    protected override void MoreActionInAwake()
    {
        if (Instance != null && Instance != this)
        {
            gameObject.SetActive(false);

            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        container.gameObject.SetActive(false);
    }

    public void Show(Action onCompleteAction = null)
    {
        container.gameObject.SetActive(true);

        canvasGroup.alpha = 0;

        AddTween(Tween.Alpha(canvasGroup, 1, duration: 0.3f)
            .OnComplete(() =>
            {
                AddTween(Tween.Delay(1).OnComplete(() =>
                {
                    onCompleteAction?.Invoke();

                    // container.gameObject.SetActive(false);
                }));
            })
        );
    }

    public void Hide()
    {
        // container.gameObject.SetActive(false);

        if (!container.gameObject.activeInHierarchy)
        {
            return;
        }

        AddTween(Tween.Alpha(canvasGroup, 0, startDelay: 0.7f, duration: 0.3f)
            .OnComplete(() =>
            {
                container.gameObject.SetActive(false);
            })
        );
    }

    public void HideAnim()
    {
        if (!container.gameObject.activeInHierarchy)
        {
            return;
        }

        AddTween(Tween.Alpha(canvasGroup, 0, startDelay: 0.7f, duration: 0.3f)
            .OnComplete(() =>
            {
                container.gameObject.SetActive(false);
            })
        );
    }
}
