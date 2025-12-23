using System;
using NamPhuThuy.Common;
using PrimeTween;
using UnityEngine;

public class AdsBreakScreen : SaferioTweenBehaviour
{
    public static AdsBreakScreen Instance;

    [SerializeField] private RectTransform container;
    [SerializeField] private CanvasGroup canvasGroup;

    #region MonoBehaviour Callback

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

    private void OnEnable()
    {
        // DebugLogger.Log(message:$"");
    }

    private void OnDisable()
    {
        // DebugLogger.Log(message:$"");
    }

    #endregion

    public void Show(Action onCompleteAction = null)
    {
        DebugLogger.Log();
        container.gameObject.SetActive(true);

        canvasGroup.alpha = 0;

        AddTween(Tween.Alpha(canvasGroup, 1, duration: 0.3f)
            .OnComplete(() =>
            {
                AddTween(Tween.Delay(1).OnComplete(() =>
                {
                    onCompleteAction?.Invoke();
                    DebugLogger.Log(message: $"AdsBreakScreen onComplete");
                }));
            })
        );
    }

    public void Hide()
    {
        // container.gameObject.SetActive(false);
        DebugLogger.Log();

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
