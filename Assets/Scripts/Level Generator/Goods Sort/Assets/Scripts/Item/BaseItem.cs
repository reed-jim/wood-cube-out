using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;

public class BaseItem : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _boxCollider;

    private BaseShelf _currentShelf;

    private bool _isSnapping;
    private bool _isStopping;
    private bool _isMoving;
    [SerializeField] private int _layer;
    private Vector3 _offsetToTouch;
    private Vector3 _lastSnapPosition;

    [SerializeField] private string _type;


    #region PROPERTY
    public SpriteRenderer SpriteRenderer
    {
        get => _spriteRenderer;
    }

    public string TYPE
    {
        get => _type;
        set => _type = value;
    }

    public int LAYER
    {
        get => _layer;
        set => _layer = value;
    }
    #endregion

    private Tween _delayAutoGoBackLastSnapPosition;

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _boxCollider = GetComponent<BoxCollider2D>();

        _type = _spriteRenderer.sprite.name;
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

                BaseItem otherItem = hit.GetComponent<BaseItem>();

                if (otherItem != null)
                {
                    if (otherItem.IsActive() && otherItem.LAYER == 0)
                    {
                        isValid = false;

                        break;
                    }
                }
            }
        }

        if (isFound && isValid)
        {
            transform.parent.GetComponent<BaseShelf>().TryBringUpAllItemsLayer();
            transform.SetParent(shelf.transform);

            shelf.AddItem(this);

            float finalX = -1.1f;
            float currentNearestX = Mathf.Abs(finalX - transform.localPosition.x);
            float nearestX = currentNearestX;

            currentNearestX = Mathf.Abs(0 - transform.localPosition.x);

            if (currentNearestX < nearestX)
            {
                nearestX = currentNearestX;
                finalX = 0;
            }

            currentNearestX = Mathf.Abs(1.1f - transform.localPosition.x);

            if (currentNearestX < nearestX)
            {
                finalX = 1.1f;
            }

            Tween.LocalPosition(transform, new Vector3(finalX, -0.7f, -1), duration: 0.3f)
            .OnComplete(() =>
            {
                Tween.Scale(transform, 1.1f * transform.localScale, duration: 0.15f, cycles: 2, cycleMode: CycleMode.Yoyo)
                .OnComplete(() =>
                {
                    shelf.CheckMatchedItems();

                    _isSnapping = false;
                });
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

    private void SnapToShelf()
    {

    }

    public void Snap()
    {

    }

    public bool IsActive()
    {
        return gameObject.activeInHierarchy;
    }
}
