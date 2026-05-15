using System;
using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseTile : MonoBehaviour
{
    [SerializeField] private RectTransform container;
    [SerializeField] private Button selectButton;
    [SerializeField] private Image containerImage;
    [SerializeField] private Image glow;
    [SerializeField] public Image icon;
    [SerializeField] private TMP_Text category;

    [SerializeField] private ItemCategory itemCategory;
    [SerializeField] private bool isMoving;
    [SerializeField] private bool isOnValidSlot;
    [SerializeField] private bool isOnSlot;
    [SerializeField] private bool isOutside;
    [SerializeField] private bool isDone;

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

    public float LeftBound => container.localPosition.x - 0.5f * container.sizeDelta.x;
    public float RightBound => container.localPosition.x + 0.5f * container.sizeDelta.x;
    public float TopBound => container.localPosition.y + 0.5f * container.sizeDelta.y;
    public float BottomBound => container.localPosition.y - 0.5f * container.sizeDelta.y;

    void Awake()
    {
        selectButton.onClick.AddListener(Select);
    }

    public void Init(ItemCategory itemCategory, Sprite iconSprite)
    {
        isMoving = true;

        SaferioTween.AlphaShow(glow, onCompleteAction: () =>
        {
            this.itemCategory = itemCategory;
            icon.sprite = iconSprite;
            category.text = itemCategory.ToString();

            containerImage.color = containerImage.color.ChangeAlpha(1);

            UIUtil.SetSizeKeepRatioY(icon, 0.8f * container.sizeDelta.x);

            IsOnValidSlot = false;
            IsDone = false;

            SaferioTween.AlphaHide(glow);
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
        if (IsMoving || IsOnSlot)
        {
            return;
        }

        IsMoving = true;

        Tween.Alpha(containerImage, 0, duration: 0.3f);

        transform.SetAsLastSibling();

        TileManager.Ins.MoveTileToSlot(this, onCompleteAction: () =>
        {
            IsMoving = false;
        });

        selectTileEvent?.Invoke(this);
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
