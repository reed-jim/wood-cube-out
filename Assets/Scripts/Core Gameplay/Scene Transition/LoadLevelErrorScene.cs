using System;
using System.Collections.Generic;
using PrimeTween;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LoadLevelErrorScene : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button retryButton;

    private List<Tween> _tweens;

    public static event Action retryLoadLevelEvent;

    void Awake()
    {
        LevelLoader.showLoadLevelErrorEvent += Show;

        retryButton.onClick.AddListener(Retry);

        _tweens = new List<Tween>();

        canvasGroup.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        LevelLoader.showLoadLevelErrorEvent -= Show;

        CommonUtil.StopAllTweens(_tweens);
    }

    private void Retry()
    {
        retryLoadLevelEvent?.Invoke();
    }

    private void Show(bool isShow)
    {
        float endValue = 0;

        if (isShow)
        {
            endValue = 1;
        }

        if (isShow)
        {
            canvasGroup.gameObject.SetActive(true);
        }

        _tweens.Add(Tween.Alpha(canvasGroup, endValue, duration: 0.3f)
        .OnComplete(() =>
        {
            if (!isShow)
            {
                canvasGroup.gameObject.SetActive(false);
            }
        })
        );
    }
}
