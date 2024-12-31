using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;

public class ScaleTransition : MonoBehaviour, ISaferioUIAnimation
{
    [SerializeField] private RectTransform target;

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

        target.localScale = Vector3.zero;

        // SaferioTween.ScaleAsync(target, 1.1f * Vector3.one, duration: 0.5f * duration, onCompletedAction: () =>
        // {
        //     SaferioTween.ScaleAsync(target, Vector3.one, duration: 0.5f * duration);
        // });

        Tween.Scale(target, 1.1f, duration: 0.5f * duration)
            .Chain(Tween.Scale(target, 1f, duration: 0.5f * duration).OnComplete(() =>
            {
                _isInTransition = false;
            }));

        // SaferioTween.LocalPositionAsync(target, Vector2.zero, duration: duration);
    }

    public void Hide()
    {
        if (_isInTransition)
        {
            return;
        }
        else
        {
            _isInTransition = true;
        }

        // SaferioTween.ScaleAsync(target, Vector3.zero, duration: 0.5f * duration, onCompletedAction: () =>
        // {
        //     target.gameObject.SetActive(false);
        // });

        Tween.Scale(target, 0, duration: duration).OnComplete(() =>
        {
            target.gameObject.SetActive(false);

            _isInTransition = false;
        });

        // SaferioTween.LocalPositionAsync(target, new Vector2(-canvasSize.Value.x, 0), duration: duration, onCompletedAction: () =>
        // {
        //     target.gameObject.SetActive(false);
        // });
    }
}
