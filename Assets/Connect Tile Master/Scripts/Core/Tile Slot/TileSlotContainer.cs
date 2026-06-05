using System;
using System.Collections.Generic;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class TileSlotContainer : MonoBehaviour
{
    [SerializeField] private RectTransform categoryContainer;
    [SerializeField] private TMP_Text category;
    [SerializeField] private TMP_Text tileCountText;
    [SerializeField] private Button unlockButton;
    [SerializeField] private Image currentTileImage;
    public Image lockImage;

    [SerializeField] private ItemCategory itemCategory;
    [SerializeField] private bool isActive;
    public bool isLocked;
    public bool isMoving;
    public bool isHidden;
    public int maxTile;
    public List<BaseTile> tiles;

    public ItemCategory ItemCategory
    {
        get => itemCategory;
        set => itemCategory = value;
    }

    public bool IsActive
    {
        get => isActive;
        set => isActive = value;
    }

    [SerializeField] private TileSlot[] tileSlots;

    public TileSlot[] TileSlots
    {
        get => tileSlots;
    }

    public static event Action<BoosterType> showBuyBoosterPopupEvent;

    void Awake()
    {
        BuyBoosterPopup.unlockScrewBoxEvent += Unlock;

        unlockButton.onClick.AddListener(ClickButtonUnlockByAds);
    }

    void OnDestroy()
    {
        BuyBoosterPopup.unlockScrewBoxEvent -= Unlock;
    }

    public void Init(ItemCategory itemCategory = ItemCategory.Vegetable)
    {
        this.itemCategory = itemCategory;
        category.text = $"{itemCategory}";

        lockImage.gameObject.SetActive(isLocked);
        categoryContainer.gameObject.SetActive(!isLocked);
    }

    public void AddTile(BaseTile tile)
    {
        tiles.Add(tile);

        tileCountText.text = $"{tiles.Count}/{maxTile}";
        currentTileImage.sprite = tile.icon.sprite;
    }

    public TileSlot GetAvailableTileSlot()
    {
        for (int i = 0; i < tileSlots.Length; i++)
        {
            TileSlot tileSlot = tileSlots[i];
            TileSlotContainer tileSlotContainer = tileSlot.tileSlotContainer;

            if (tileSlotContainer != null)
            {
                if (tileSlotContainer.isMoving || tileSlotContainer.isHidden)
                {
                    continue;
                }
            }

            if (tileSlot.Tile == null)
            {
                return tileSlot;
            }
        }

        return null;
    }

    public bool IsLastSlot(TileSlot slot)
    {
        return slot == tileSlots[tileSlots.Length - 1];
    }

    public void ClearTiles()
    {
        foreach (var tileSlot in tileSlots)
        {
            tileSlot.ClearTile();
        }
    }

    private void ClickButtonUnlockByAds()
    {
        if (isLocked)
        {
            showBuyBoosterPopupEvent?.Invoke(BoosterType.UnlockScrewBox);
        }
    }

    public void Unlock()
    {
        Unlock(onCompleteAction: null);
    }

    public void Unlock(Action onCompleteAction = null)
    {
        if (isLocked)
        {
            category.gameObject.SetActive(true);
            category.alpha = 0;

            Tween.Alpha(category, 1, duration: 0.3f);
            Tween.Alpha(lockImage, 0, duration: 0.3f).OnComplete(() =>
            {
                lockImage.gameObject.SetActive(false);
                categoryContainer.gameObject.SetActive(true);
            });

            isLocked = false;

            onCompleteAction?.Invoke();
        }
    }

    public void Complete(ItemCategory nextCategory, Action onCompleteAction)
    {
        Init(nextCategory);

        float initialPositionY = transform.localPosition.y;

        isMoving = true;

        Tween.LocalPositionY(transform, initialPositionY + 1600, duration: 0.7f)
        .OnComplete(() =>
        {
            Tween.LocalPositionY(transform, initialPositionY, duration: 0.3f)
            .OnComplete(() =>
            {
                isMoving = false;

                onCompleteAction?.Invoke();
            });
        });
    }

    public void Hide(Action onCompleteAction)
    {
        float initialPositionY = transform.localPosition.y;

        isMoving = true;
        isHidden = true;

        Tween.LocalPositionY(transform, initialPositionY + 1600, duration: 0.7f)
        .OnComplete(() =>
        {
            isMoving = false;

            onCompleteAction?.Invoke();
        });
    }
}
