using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private RectTransform canvas;
    [SerializeField] private RectTransform container;
    [SerializeField] private RectTransform progressFillRT;
    [SerializeField] private RectTransform loadingTextRT;
    [SerializeField] private Image fadeBackground;
    [SerializeField] private Image blackBackground;
    [SerializeField] private Image progressFill;
    [SerializeField] private TMP_Text loadingText;

    [Header("CUSTOMIZE")]
    [SerializeField] private float startDelay;
    [SerializeField] private float fadeOutDuration;

    private Vector2 _canvasSize;
    private List<Tween> _tweens;
    private bool _isFirstTimeEnterScene;


    private void Awake()
    {
        LevelLoader.showSceneTransitionEvent += BlackScreenTransition;

        _tweens = new List<Tween>();

        _canvasSize = canvas.sizeDelta;

        Transition();

        blackBackground.gameObject.SetActive(false);

        _isFirstTimeEnterScene = true;
    }

    private void OnDestroy()
    {
        LevelLoader.showSceneTransitionEvent -= BlackScreenTransition;

        CommonUtil.StopAllTweens(_tweens);
    }

    private void Transition()
    {
        container.gameObject.SetActive(true);

        _tweens.Add(Tween.Custom(0, 1, duration: 0.95f * startDelay, onValueChange: newVal =>
        {
            progressFill.fillAmount = newVal;

            loadingText.text = $"{(int)(newVal * 100)}%";
        }));

        SaferioTween.DelayAsync(startDelay, onCompletedAction: (() =>
        {
            SaferioTween.LocalPositionAsync(progressFillRT, new Vector3(0, -_canvasSize.y), duration: fadeOutDuration);
            SaferioTween.AlphaAsync(fadeBackground, 0, duration: fadeOutDuration, onCompletedAction: () =>
            {
                container.gameObject.SetActive(false);
            });
        }));
    }

    private void BlackScreenTransition()
    {
        if (_isFirstTimeEnterScene)
        {
            _isFirstTimeEnterScene = false;

            return;
        }

        blackBackground.color = ColorUtil.WithAlpha(blackBackground.color, 1);

        blackBackground.gameObject.SetActive(true);

        // Tween.Alpha(blackBackground, 0, 1, duration: 0.1f * fadeOutDuration)
        Tween.Delay(fadeOutDuration)
        .Chain(Tween.Alpha(blackBackground, 1, 0, duration: fadeOutDuration).OnComplete(() =>
        {
            blackBackground.gameObject.SetActive(false);
        }));
    }
}
