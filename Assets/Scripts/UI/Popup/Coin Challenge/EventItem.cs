using System;
using PrimeTween;
using UnityEngine;

public class EventItem : SaferioTweenBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform container;

    public void ShowUp(Action onCompleteAction)
    {
        container.gameObject.SetActive(true);

        container.localPosition = Vector3.zero;
        canvasGroup.alpha = 1;

        StopAllTweens();

        AddTween(Tween.LocalPositionY(container, 300, duration: 1f)
        .OnComplete(() =>
        {
            AddTween(Tween.Alpha(canvasGroup, 0, duration: 0.3f)
            .OnComplete(() =>
            {
                container.gameObject.SetActive(false);

                onCompleteAction?.Invoke();
            }));
        }));
    }
}
