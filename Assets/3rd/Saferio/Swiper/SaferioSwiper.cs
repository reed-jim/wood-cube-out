using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;

public class SaferioSwiper : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform target;

    private Vector2 _startPos;
    private float _swipeThreshold = 50f; // Minimum distance for a swipe

    private bool _isSwiping;

    public void OnPointerDown(PointerEventData eventData)
    {
        _startPos = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_isSwiping)
        {
            return;
        }

        Vector2 endPos = eventData.position;
        Vector2 delta = endPos - _startPos;

        if (delta.magnitude >= _swipeThreshold)
        {
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                if (delta.x > 0)
                {
                    Tween.LocalPositionX(target, target.localPosition.x + GamePersistentVariable.canvasSize.x, duration: 0.3f)
                        .OnComplete(() =>
                        {
                            _isSwiping = false;
                        });
                }
                else
                {
                    Tween.LocalPositionX(target, target.localPosition.x - GamePersistentVariable.canvasSize.x, duration: 0.3f)
                        .OnComplete(() =>
                        {
                            _isSwiping = false;
                        });
                }

                _isSwiping = true;
            }
        }
    }
}
