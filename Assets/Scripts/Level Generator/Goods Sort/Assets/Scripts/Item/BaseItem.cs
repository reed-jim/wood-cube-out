using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;

public class BaseItem : MonoBehaviour
{
    [SerializeField] private ItemServiceLocator itemServiceLocator;
    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _boxCollider;

    private BaseShelf _currentShelf;

    private bool _isSnapping;
    private bool _isStopping;
    private bool _isMoving;
    [SerializeField] private int _layer;
    [SerializeField] private int _slot;
    private Vector3 _offsetToTouch;
    private Vector3 _lastSnapPosition;

    [SerializeField] private string _type;


    #region PROPERTY
    public ItemServiceLocator ItemServiceLocator
    {
        get => itemServiceLocator;
    }

    public SpriteRenderer SpriteRenderer
    {
        get => _spriteRenderer;
    }

    public BoxCollider2D BoxCollider2D
    {
        get => _boxCollider;
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

    public int SLOT
    {
        get => _slot;
        set => _slot = value;
    }
    #endregion

    private Tween _delayAutoGoBackLastSnapPosition;

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _boxCollider = GetComponent<BoxCollider2D>();

        _type = _spriteRenderer.sprite.name;

        GamePersistentVariable.numItemsLeft++;
    }

    void Start()
    {
        GameplayUI.Instance.UpdateNumItems();
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

            Vector3 newPosition = new Vector3(position.x, position.y, transform.position.z) - _offsetToTouch;
            newPosition.z = -1.1f;

            transform.position = newPosition;
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

            BaseShelf hitShelf = hit.GetComponent<BaseShelf>();

            if (hitShelf != null)
            {
                shelf = hitShelf;
            }

            if (_currentShelf == null || shelf != _currentShelf)
            {
                if (shelf != null)
                {
                    isFound = true;
                }

                BaseItem otherItem = hit.GetComponent<BaseItem>();

                if (otherItem != null)
                {
                    if (otherItem.IsActive() && otherItem.LAYER == 0 && IsOverlapOtherItemTooMuch(otherItem))
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

            Vector3 snapPosition;
            int snapSlot;

            GetSnapInfo(out snapPosition, out snapSlot);

            Tween.LocalPosition(transform, snapPosition, duration: 0.3f)
            .OnComplete(() =>
            {
                Vector3 initialLocalPosition = transform.localPosition;
                Vector3 initialScale = transform.localScale;

                Tween.Custom(0, 0.1f, cycles: 2, cycleMode: CycleMode.Yoyo, duration: 0.15f, onValueChange: newVal =>
                {
                    transform.localPosition = initialLocalPosition.ChangeY(initialLocalPosition.y - newVal);
                    transform.localScale = initialScale + new Vector3(newVal, -newVal, 0);
                })
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

    private void GetSnapInfo(out Vector3 snapPosition, out int snapSlot)
    {
        float finalX = -0.9f;
        float currentNearestX = Mathf.Abs(finalX - transform.localPosition.x);
        float nearestX = currentNearestX;

        snapSlot = -1;

        currentNearestX = Mathf.Abs(0 - transform.localPosition.x);

        if (currentNearestX < nearestX)
        {
            nearestX = currentNearestX;
            finalX = 0;
            snapSlot = 0;
        }

        currentNearestX = Mathf.Abs(0.9f - transform.localPosition.x);

        if (currentNearestX < nearestX)
        {
            finalX = 0.9f;
            snapSlot = 1;
        }

        float finalZ = -1 + 0.1f * LAYER;

        snapPosition = new Vector3(finalX, 0.2f, finalZ);
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

    private bool IsOverlapOtherItemTooMuch(BaseItem other)
    {
        Vector3 size = _spriteRenderer.bounds.size;

        return Mathf.Abs(transform.position.x - other.transform.position.x) < 0.5f * size.x;
    }

    #region BOOSTER
    public void MoveToFoodBox(FoodBox foodBox)
    {
        transform.SetParent(foodBox.transform);

        Tween.Position(transform, foodBox.transform.position, duration: 0.3f);
        Tween.Scale(transform, 1, duration: 0.3f)
        .OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }
    #endregion
}
