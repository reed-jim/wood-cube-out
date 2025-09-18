using System;
using System.Collections.Generic;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;

public class BaseItem : SaferioTweenBehaviour
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

    protected override void MoreActionInAwake()
    {
        BaseShelf.clearItemsEvent += OnTilesCleared;

        _spriteRenderer = GetComponent<SpriteRenderer>();
        _boxCollider = GetComponent<BoxCollider2D>();

        _type = _spriteRenderer.sprite.name;
        _currentShelf = transform.GetComponentInAncestors<BaseShelf>();

        GamePersistentVariable.numItemsLeft++;
    }

    protected override void MoreActionOnDestroy()
    {
        BaseShelf.clearItemsEvent -= OnTilesCleared;
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
        // CheckMoveNextLayerItems();

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

                isFound = true;

                break;
            }

            // if (_currentShelf == null || shelf != _currentShelf)
            // {
            //     if (shelf != null)
            //     {
            //         isFound = true;
            //     }
            // }
        }

        Vector3 snapPosition = Vector3.zero;
        int snapSlot;

        if (isFound)
        {
            isValid = GetSnapInfo(shelf.transform, out snapPosition, out snapSlot);
        }

        if (isValid)
        {
            if (shelf != _currentShelf)
            {
                CheckMoveNextLayerItems();
                // transform.parent.GetComponent<BaseShelf>().TryBringUpAllItemsLayer();
            }

            transform.SetParent(shelf.transform);

            shelf.AddItem(this);

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
                    transform.localPosition = snapPosition;

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
    }

    private bool GetSnapInfo(Transform targetGrill, out Vector3 snapPosition, out int snapSlot)
    {
        List<BaseItem> firstLayerItems = new List<BaseItem>();

        bool isOverlapOtherItemTooMuch = false;

        for (int i = 0; i < targetGrill.childCount; i++)
        {
            BaseItem item = targetGrill.GetChild(i).GetComponent<BaseItem>();

            if (item != this && item != null)
            {
                if (item.gameObject.activeInHierarchy)
                {
                    firstLayerItems.Add(item);

                    if (IsOverlapOtherItemTooMuch(item))
                    {
                        isOverlapOtherItemTooMuch = true;
                    }
                }
            }
        }

        Debug.Log("SAFERIO isOverlapOtherItemTooMuch " + isOverlapOtherItemTooMuch);

        if (isOverlapOtherItemTooMuch)
        {
            float[] snapPositions = new float[3] { -0.9f, 0, 0.9f };

            snapPosition = new Vector3();
            snapSlot = 0;

            for (int i = 0; i < snapPositions.Length; i++)
            {
                int index = i;

                bool isValid = true;

                for (int j = 0; j < firstLayerItems.Count; j++)
                {
                    if (Mathf.Approximately(firstLayerItems[j].transform.localPosition.x, snapPositions[index]))
                    {
                        isValid = false;

                        break;
                    }
                }

                if (isValid)
                {
                    snapPosition = new Vector3(snapPositions[index], 0.2f, -1 + 0.1f * LAYER);
                    snapSlot = index;

                    return true;
                }
            }

            return false;
        }

        Vector3 positionRelativeToTargetGrill = targetGrill.InverseTransformPoint(transform.position);

        float finalX = -0.9f;
        float currentNearestX = Mathf.Abs(finalX - positionRelativeToTargetGrill.x);
        float nearestX = currentNearestX;

        snapSlot = -1;

        currentNearestX = Mathf.Abs(0 - positionRelativeToTargetGrill.x);

        if (currentNearestX < nearestX)
        {
            nearestX = currentNearestX;
            finalX = 0;
            snapSlot = 0;
        }

        currentNearestX = Mathf.Abs(0.9f - positionRelativeToTargetGrill.x);

        if (currentNearestX < nearestX)
        {
            finalX = 0.9f;
            snapSlot = 1;
        }

        float finalZ = -1 + 0.1f * LAYER;

        snapPosition = new Vector3(finalX, 0.2f, finalZ);

        return true;
    }

    private void SnapToShelf()
    {

    }

    public void Snap()
    {

    }

    public virtual void BringToFirstLayer(BaseShelf shelf, int index)
    {
        Vector3 position;

        position.x = -0.9f * (1 - index);
        position.y = 0.2f;
        position.z = -1 + 0.1f * LAYER;

        transform.SetParent(shelf.transform);

        AddTween(Tween.LocalPosition(transform, position, duration: 0.3f));
        AddTween(Tween.Scale(transform, 1, duration: 0.3f));
        AddTween(Tween.Color(SpriteRenderer, Color.white, duration: 0.3f));
    }

    private void CheckMoveNextLayerItems()
    {
        BaseShelf grill = GetComponentInParent<BaseShelf>();

        if (grill != null)
        {
            if (grill.IsAllFirstLayerItemsMoved(this))
            {
                grill.BringUpAllItemsLayer();
            }
        }
    }

    #region UTIL
    public bool IsActive()
    {
        return gameObject.activeInHierarchy;
    }

    private bool IsOverlapOtherItemTooMuch(BaseItem other)
    {
        Vector3 size = _spriteRenderer.bounds.size;

        return Mathf.Abs(transform.position.x - other.transform.position.x) < 1f * size.x;
    }
    #endregion

    #region PROPERTY
    public void SetIsCleared(bool isCleared)
    {
        itemServiceLocator.itemProperty.IsClear = isCleared;
    }

    public virtual bool IsLocked()
    {
        return false;
    }
    #endregion

    #region BOOSTER
    public void MoveToFoodBox(FoodBox foodBox, Action onCompleteAction)
    {
        transform.SetParent(foodBox.transform);

        float duration = 0.6f;

        Tween.Position(transform, foodBox.transform.position.AddZ(0.1f), duration: duration, ease: Ease.InOutSine);
        Tween.Scale(transform, 0.7f, duration: duration)
        .OnComplete(() =>
        {
            gameObject.SetActive(false);

            itemServiceLocator.itemProperty.IsClear = true;

            onCompleteAction?.Invoke();
        });
    }
    #endregion

    #region CALLBACK
    protected virtual void OnTilesCleared(BaseItem[] items)
    {

    }
    #endregion
}
