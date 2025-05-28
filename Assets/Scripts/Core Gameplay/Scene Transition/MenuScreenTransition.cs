using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class MenuScreenTransition : MonoBehaviour
{
    [SerializeField] private Image blackOverlay;

    private List<Tween> _tweens;

    void Awake()
    {
        _tweens = new List<Tween>();

        BlackScreenTransition();
    }

    private void OnDestroy()
    {
        CommonUtil.StopAllTweens(_tweens);
    }

    private void BlackScreenTransition()
    {
        blackOverlay.color = ColorUtil.WithAlpha(blackOverlay.color, 1);

        blackOverlay.gameObject.SetActive(true);

        CommonUtil.StopAllTweens(_tweens);

        _tweens.Add(Tween.Alpha(blackOverlay, 1, 0, startDelay: 0.5f, duration: 0.3f).OnComplete(() =>
        {
            blackOverlay.gameObject.SetActive(false);
        }));
    }
}
