using System;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;

public class OpacityTransition : MonoBehaviour, ISaferioUIAnimation
{
    [SerializeField] private RectTransform target;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private Vector2Variable canvasSize;

    [Header("CUSTOMIZE")]
    [SerializeField] private float duration;

    #region PRIVATE FIELD
    private bool _isInTransition;
    #endregion

    public void Show()
    {
        if (_isInTransition)
        {
            return;
        }
        else
        {
            _isInTransition = true;
        }

        target.gameObject.SetActive(true);

        canvasGroup.interactable = false;
        canvasGroup.alpha = 0;

        Tween.Alpha(canvasGroup, 1, duration: duration).OnComplete(() =>
        {
            _isInTransition = false;

            canvasGroup.interactable = true;
        });
    }

    public void Show(Action onCompletedAction)
    {
        if (_isInTransition)
        {
            return;
        }
        else
        {
            _isInTransition = true;
        }

        target.gameObject.SetActive(true);

        canvasGroup.interactable = false;
        canvasGroup.alpha = 0;

        Tween.Alpha(canvasGroup, 1, duration: duration).OnComplete(() =>
        {
            _isInTransition = false;

            canvasGroup.interactable = true;

            onCompletedAction?.Invoke();
        });
    }

    public void Hide()
    {
        Hide(onCompletedAction: null);
    }

    public void Hide(Action onCompletedAction)
    {
        if (_isInTransition)
        {
            return;
        }
        else
        {
            _isInTransition = true;
        }

        canvasGroup.interactable = false;
        canvasGroup.alpha = 0;

        Tween.Alpha(canvasGroup, 0, duration: duration).OnComplete(() =>
        {
            target.gameObject.SetActive(false);

            _isInTransition = false;

            onCompletedAction?.Invoke();
        });
    }
}
