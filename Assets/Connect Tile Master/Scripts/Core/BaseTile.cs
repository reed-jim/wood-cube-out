using System;
using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BaseTile : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform container;
    [SerializeField] private Button selectButton;
    [SerializeField] private Image containerImage;
    [SerializeField] private Image glow;
    [SerializeField] public Image icon;
    [SerializeField] public TMP_Text category;

    [SerializeField] private ItemCategory itemCategory;
    [SerializeField] private bool isMoving;
    [SerializeField] private bool isOnValidSlot;
    [SerializeField] private bool isOnSlot;
    [SerializeField] private bool isOutside;
    [SerializeField] private bool isDone;
    [SerializeField] private bool isText;


    public static event Action<BaseTile> selectTileEvent;

    public ItemCategory ItemCategory
    {
        get => itemCategory;
        set => itemCategory = value;
    }

    public bool IsMoving
    {
        get => isMoving;
        set => isMoving = value;
    }

    public bool IsOnValidSlot
    {
        get => isOnValidSlot;
        set => isOnValidSlot = value;
    }

    public bool IsOnSlot
    {
        get => isOnSlot;
        set => isOnSlot = value;
    }

    public bool IsOutside
    {
        get => isOutside;
        set => isOutside = value;
    }

    public bool IsDone
    {
        get => isDone;
        set => isDone = value;
    }

    public bool IsText
    {
        get => isText;
        set => isText = value;
    }

    public float LeftBound => container.localPosition.x - 0.5f * container.sizeDelta.x;
    public float RightBound => container.localPosition.x + 0.5f * container.sizeDelta.x;
    public float TopBound => container.localPosition.y + 0.5f * container.sizeDelta.y;
    public float BottomBound => container.localPosition.y - 0.5f * container.sizeDelta.y;

    private Vector3 _originalPosition;
    private int _originalSiblingIndex;
    private bool _isDragging;

    void Awake()
    {
        // selectButton.onClick.AddListener(Select);
    }

    public void Init(ItemCategory itemCategory, Sprite iconSprite, string textData = null)
    {
        this.itemCategory = itemCategory;
        isText = !string.IsNullOrEmpty(textData);
        if (!string.IsNullOrEmpty(textData))
        {
            icon.gameObject.SetActive(false);
            category.gameObject.SetActive(true);
            category.text = textData;
        }
        else
        {
            icon.gameObject.SetActive(true);
            category.gameObject.SetActive(false);
            icon.sprite = iconSprite;
            UIUtil.SetSizeKeepRatioY(icon, 0.8f * container.sizeDelta.x);
        }

        containerImage.color = containerImage.color.ChangeAlpha(1);

        IsOnValidSlot = false;
        IsOnSlot = false;
        IsOutside = false;
        IsDone = false;
        _isDragging = false;
    }

    public void Shuffle(ItemCategory itemCategory, Sprite iconSprite, string textData = null)
    {
        isMoving = true;

        SaferioTween.AlphaShow(glow, onCompleteAction: () =>
        {
            this.itemCategory = itemCategory;

            if (!string.IsNullOrEmpty(textData))
            {
                icon.gameObject.SetActive(false);
                category.gameObject.SetActive(true);
                category.text = textData;
            }
            else
            {
                icon.gameObject.SetActive(true);
                category.gameObject.SetActive(false);
                icon.sprite = iconSprite;
                UIUtil.SetSizeKeepRatioY(icon, 0.8f * container.sizeDelta.x);
            }

            containerImage.color = containerImage.color.ChangeAlpha(1);

            IsOnValidSlot = false;
            IsDone = false;

            SaferioTween.AlphaHide(glow, onCompleteAction: () =>
            {
                isMoving = false;
            });
        });

        Vector3 initialPosition = transform.localPosition;
        Vector3 expandedPosition = 1.2f * initialPosition;

        Tween.LocalPosition(transform, expandedPosition, duration: 0.3f)
        .OnComplete(() =>
        {
            Tween.LocalPosition(transform, initialPosition, duration: 0.3f)
            .OnComplete(() =>
            {
                isMoving = false;
            });
        });
    }

    public void Select()
    {
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsMoving || IsOnSlot || IsDone) return;

        _isDragging = true;
        _originalPosition = transform.position;
        _originalSiblingIndex = transform.GetSiblingIndex();

        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        if (IsMoving || IsOnSlot || IsDone) return;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                (RectTransform)transform.parent,
                eventData.position,
                eventData.pressEventCamera,
                out Vector3 worldPoint))
        {
            transform.position = worldPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        _isDragging = false;

        if (IsMoving || IsOnSlot || IsDone) return;

        if (TileManager.Ins.IsHoveringValidContainer(this.itemCategory, eventData.position, eventData.pressEventCamera))
        {
            IsMoving = true;

            Tween.Alpha(containerImage, 0, duration: 0.3f);

            TileManager.Ins.MoveTileToSlot(this, onCompleteAction: () =>
            {
                IsMoving = false;
            });

            selectTileEvent?.Invoke(this);
        }
        else
        {
            IsMoving = true;
            Tween.Position(transform, _originalPosition, duration: 0.2f).OnComplete(() =>
            {
                IsMoving = false;
                transform.SetSiblingIndex(_originalSiblingIndex);
            });
        }
    }

    public void MoveToOutsideBox()
    {
        if (IsMoving)
        {
            return;
        }

        IsMoving = true;

        Tween.Position(transform, TileManager.Ins.outsideBox.transform.position, duration: 0.3f)
        .OnComplete(() =>
        {
            IsMoving = false;
        });
        Tween.Alpha(icon, 0, duration: 0.3f);

        IsOutside = true;
    }
}
