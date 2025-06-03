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

        container.localPosition = new Vector3(0, 300, 0);
        canvasGroup.alpha = 0;

        StopAllTweens();

        AddTween(Tween.Alpha(canvasGroup, 1, duration: 0.3f)
        .OnComplete(() =>
        {
            AddTween(Tween.LocalPositionY(container, 600, duration: 1f)
            .OnComplete(() =>
            {
                AddTween(Tween.Alpha(canvasGroup, 0, duration: 0.3f)
                .OnComplete(() =>
                {
                    container.gameObject.SetActive(false);

                    onCompleteAction?.Invoke();
                }));
            }));
        }));
    }
}
