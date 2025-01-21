using System;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class SimpleSceneTransition : MonoBehaviour
{
    [SerializeField] private Image blackBackground;

    [Header("CUSTOMIZE")]
    [SerializeField] private float startDelay;
    [SerializeField] private float fadeOutDuration;

    private List<Tween> _tweens;

    private void Awake()
    {
        _tweens = new List<Tween>();

        BlackScreenTransition(startDelay, onCompletedAction: null);
    }

    private void OnDestroy()
    {
        CommonUtil.StopAllTweens(_tweens);
    }

    private void BlackScreenTransition(float delay, Action onCompletedAction)
    {
        blackBackground.color = ColorUtil.WithAlpha(blackBackground.color, 1);

        blackBackground.gameObject.SetActive(true);

        _tweens.Add(Tween.Alpha(blackBackground, 1, 0, startDelay: delay, duration: fadeOutDuration).OnComplete(() =>
        {
            blackBackground.gameObject.SetActive(false);

            onCompletedAction?.Invoke();
        }));
    }
}
