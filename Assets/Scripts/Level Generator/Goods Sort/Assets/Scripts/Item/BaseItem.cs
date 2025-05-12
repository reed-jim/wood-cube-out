using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;

public class BaseItem : MonoBehaviour
{
    private BoxCollider2D _boxCollider;

    private BaseShelf _currentShelf;

    private bool _isSnapping;
    private bool _isStopping;
    private bool _isMoving;
    private Vector3 _offsetToTouch;
    private Vector3 _lastSnapPosition;

    private Tween _delayAutoGoBackLastSnapPosition;

    void Awake()
    {
        _boxCollider = GetComponent<BoxCollider2D>();
    }

    public void Follow(Vector2 position)
    {
        if (!_isSnapping)
        {
            if (!_isMoving)
            {
                _isMoving = true;

                _lastSnapPosition = transform.position;
                _offsetToTouch = ((Vector3)position - transform.position).ChangeZ(0);
            }

            transform.position = new Vector3(position.x, position.y, transform.position.z) - _offsetToTouch;
        }
    }

    public void Stop()
    {
        _isStopping = true;

        Vector3 lossyScale = transform.lossyScale;

        Collider2D[] hits =
            Physics2D.OverlapBoxAll(transform.position + lossyScale.x * (Vector3)_boxCollider.offset, transform.lossyScale.x * _boxCollider.size, 0);

        bool isFound = false;
        bool isValid = true;

        BaseShelf shelf = null;

        foreach (var hit in hits)
        {
            if (hit == _boxCollider)
            {
                continue;
            }

            shelf = hit?.GetComponent<BaseShelf>();

            if (_currentShelf == null || shelf != _currentShelf)
            {
                if (shelf != null)
                {
                    isFound = true;
                }

                if (hit.GetComponent<BaseItem>() != null)
                {
                    isValid = false;

                    break;
                }
            }
        }

        if (isFound && isValid)
        {
            transform.SetParent(shelf.transform);

            float finalX = -0.35f;
            float currentNearestX = Mathf.Abs(finalX - transform.localPosition.x);
            float nearestX = currentNearestX;

            currentNearestX = Mathf.Abs(0 - transform.localPosition.x);

            if (currentNearestX < nearestX)
            {
                nearestX = currentNearestX;
                finalX = 0;
            }

            currentNearestX = Mathf.Abs(0.35f - transform.localPosition.x);

            if (currentNearestX < nearestX)
            {
                finalX = 0.35f;
            }

            Tween.LocalPosition(transform, new Vector3(finalX, -0.55f, -1), duration: 0.3f)
            .OnComplete(() =>
            {
                _isSnapping = false;
            });

            _currentShelf = shelf;

            _isSnapping = true;
            _isStopping = false;
            _isMoving = false;
        }
        else
        {
            Tween.Position(transform, _lastSnapPosition, duration: 0.3f);

            _isStopping = false;
            _isMoving = false;
        }

        // _delayAutoGoBackLastSnapPosition =
        //     Tween.Delay(0.3f).OnComplete(() =>
        //     {
        // Tween.Position(transform, _lastSnapPosition, duration: 0.3f);

        // _isStopping = false;
        // _isMoving = false;
        //     });
    }

    public void Snap()
    {

    }

    // void OnTriggerEnter2D(Collider2D other)
    // {
    //     if (!_isStopping)
    //     {
    //         return;
    //     }

    // BaseShelf shelf = other.GetComponent<BaseShelf>();

    // if (_currentShelf == null || shelf != _currentShelf)
    // {
    //     if (shelf != null)
    //     {
    //         if (_delayAutoGoBackLastSnapPosition.isAlive)
    //         {
    //             _delayAutoGoBackLastSnapPosition.Stop();
    //         }

    //         transform.SetParent(shelf.transform);

    //         float finalX = -0.35f;
    //         float currentNearestX = Mathf.Abs(finalX - transform.localPosition.x);
    //         float nearestX = currentNearestX;

    //         currentNearestX = Mathf.Abs(0 - transform.localPosition.x);

    //         if (currentNearestX < nearestX)
    //         {
    //             nearestX = currentNearestX;
    //             finalX = 0;
    //         }

    //         currentNearestX = Mathf.Abs(0.35f - transform.localPosition.x);

    //         if (currentNearestX < nearestX)
    //         {
    //             finalX = 0.35f;
    //         }

    //         Tween.LocalPosition(transform, new Vector3(finalX, -0.55f, -1), duration: 0.3f)
    //         .OnComplete(() =>
    //         {
    //             _isSnapping = false;
    //         });

    //         _currentShelf = shelf;

    //         _isSnapping = true;
    //         _isStopping = false;
    //         _isMoving = false;
    //     }
    // }
    // }
}
