using System;
using DG.Tweening;
using UnityEngine;

public static class DOTweenUtil
{
    public static void MoveParabol(Transform target, Vector3 destination, float height = 6, float duration = 0.6f, Action onComplete = null)
    {
        Vector3 start = target.position;
        Vector3 end = destination;

        Vector3 mid = (start + end) * 0.5f + Vector3.up * height;

        Vector3[] path =
        {
            start,
            mid,
            end
        };

        target.DOPath(
                path,
                duration,
                PathType.CatmullRom)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                onComplete?.Invoke();
            });
    }
}
