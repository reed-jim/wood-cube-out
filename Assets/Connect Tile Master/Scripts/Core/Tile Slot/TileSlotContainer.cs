using System;
using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using Saferio.Util.SaferioTween;
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
    [SerializeField] private Image fakeTileImage;
    [SerializeField] private TMP_Text currentTileText;
    [SerializeField] private TMP_Text fakeTileText;
    public Image lockImage;
    [SerializeField] private Image glow;
    [SerializeField] private Image lockObject;
    [SerializeField] private Image unlockObject;
    [SerializeField] private ItemCategory itemCategory;
    [SerializeField] private bool isActive;
    public bool isLocked;
    public bool isMoving;
    public bool isHidden;
    public bool isCompleted;
    public int maxTile;
    public List<BaseTile> tiles;

    public TileIconContainer tileIconContainer;

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

    public float originalPositionY;
    private bool originalIsLocked;

    void Awake()
    {
        originalPositionY = transform.localPosition.y;
        originalIsLocked = isLocked;
        BuyBoosterPopup.unlockScrewBoxEvent += Unlock;
    }

    public void SetOriginalLockState(bool locked)
    {
        isLocked = locked;
        originalIsLocked = locked;
    }

    void OnDestroy()
    {
        BuyBoosterPopup.unlockScrewBoxEvent -= Unlock;
    }

    private BaseTile tileTemp;

    public void Init(ItemCategory itemCategory = ItemCategory.Fast_Food, int targetCardAmount = 4)
    {
        this.itemCategory = itemCategory;
        this.maxTile = targetCardAmount;
        this.isCompleted = false;
        this.isHidden = false;
        this.isMoving = false;
        this.isLocked = originalIsLocked;
        category.text = "???";
        if (category != null) category.alpha = 1f;

        if (lockImage != null)
        {
            lockImage.gameObject.SetActive(isLocked);
            Color c = lockImage.color;
            c.a = 1f;
            lockImage.color = c;
        }

        categoryContainer.gameObject.SetActive(!isLocked);

        if (unlockObject != null)
        {
            unlockObject.gameObject.SetActive(!isLocked);
        }

        if (lockObject != null) lockObject.gameObject.SetActive(isLocked);

        if (tileSlots != null && tileSlots.Length > 0)
        {
            for (int i = 0; i < tileSlots.Length; i++)
            {
                tileSlots[i].gameObject.SetActive(i == 0);
            }
        }

        TileIconContainer containerToUse = tileIconContainer;
        if (TileManager.Ins != null && TileManager.Ins.levelItem != null && TileManager.Ins.levelItem.tileIconContainer != null)
        {
            containerToUse = TileManager.Ins.levelItem.tileIconContainer;
        }

        CategoryIconSet iconSet = containerToUse.categoryIconSets.First(item => item.itemCategory == itemCategory);

        bool hasText = iconSet.text != null && iconSet.text.Length > 0;
        bool hasIcon = iconSet.icons != null && iconSet.icons.Length > 0;

        // Check if specific order text or order sprite is defined
        bool hasTextOrder = !string.IsNullOrEmpty(iconSet.textOrder);
        bool hasIconOrder = iconSet.spriteOrder != null;

        bool useText = false;
        if (hasTextOrder && hasIconOrder)
        {
            useText = UnityEngine.Random.value > 0.5f;
        }
        else if (hasTextOrder)
        {
            useText = true;
        }
        else if (hasIconOrder)
        {
            useText = false;
        }
        else
        {
            // Fallback to random if neither order is set
            if (hasText && hasIcon)
            {
                useText = UnityEngine.Random.value > 0.5f;
            }
            else if (hasText)
            {
                useText = true;
            }
            else if (hasIcon)
            {
                useText = false;
            }
        }

        if (useText)
        {
            if (currentTileImage != null) currentTileImage.gameObject.SetActive(false);
            if (currentTileText != null)
            {
                currentTileText.gameObject.SetActive(true);
                if (hasTextOrder)
                {
                    currentTileText.text = iconSet.textOrder;
                }
                else
                {
                    currentTileText.text = containerToUse.GetRandomTextOfCategory(itemCategory);
                }
            }
        }
        else
        {
            if (currentTileText != null) currentTileText.gameObject.SetActive(false);
            if (currentTileImage != null)
            {
                currentTileImage.gameObject.SetActive(true);
                if (hasIconOrder)
                {
                    currentTileImage.sprite = iconSet.spriteOrder;
                }
                else if (hasIcon)
                {
                    currentTileImage.sprite = iconSet.icons[UnityEngine.Random.Range(0, iconSet.icons.Length)];
                }
            }
        }

        if (fakeTileImage != null) fakeTileImage.gameObject.SetActive(false);
        if (fakeTileText != null) fakeTileText.gameObject.SetActive(false);

        tileCountText.text = $"{tiles.Count}/{maxTile}";
    }

    public void AddTile(BaseTile tile)
    {
        tiles.Add(tile);
        tileCountText.text = $"{tiles.Count}/{maxTile}";
    }

    public void SwitchItem(BaseTile tile)
    {
        if (tile.IsText)
        {
            if (currentTileImage != null) currentTileImage.gameObject.SetActive(false);
            if (fakeTileImage != null) fakeTileImage.gameObject.SetActive(false);

            if (currentTileText != null && fakeTileText != null)
            {
                fakeTileText.gameObject.SetActive(true);
                fakeTileText.text = currentTileText.text;
                fakeTileText.color = Color.white;

                currentTileText.gameObject.SetActive(true);
                currentTileText.text = tile.category.text;
                Color txtColor = currentTileText.color;
                currentTileText.color = new Color(txtColor.r, txtColor.g, txtColor.b, 0f);

                Tween.Alpha(currentTileText, 1f, duration: 0.3f);
                Tween.Alpha(fakeTileText, 0f, duration: 0.3f).OnComplete(() => fakeTileText.gameObject.SetActive(false));
            }
        }
        else
        {
            if (currentTileText != null) currentTileText.gameObject.SetActive(false);
            if (fakeTileText != null) fakeTileText.gameObject.SetActive(false);

            if (currentTileImage != null && fakeTileImage != null)
            {
                fakeTileImage.gameObject.SetActive(true);
                fakeTileImage.sprite = currentTileImage.sprite;
                fakeTileImage.color = Color.white;

                currentTileImage.gameObject.SetActive(true);
                currentTileImage.sprite = tile.icon.sprite;

                SaferioTween.AlphaShow(currentTileImage);
                SaferioTween.AlphaHide(fakeTileImage);
            }
        }
    }

    public TileSlot GetAvailableTileSlot()
    {
        if (isMoving || isHidden || isLocked) return null;

        if (tiles.Count < maxTile && tileSlots != null && tileSlots.Length > 0)
        {
            return tileSlots[0];
        }

        return null;
    }

    public bool IsLastSlot(TileSlot slot)
    {
        return tiles.Count >= maxTile;
    }

    public void ClearTiles()
    {
        tiles.Clear();

        if (tileSlots != null)
        {
            foreach (var tileSlot in tileSlots)
            {
                tileSlot.Tile = null;
            }
        }

        if (tileTemp != null)
        {
            tileTemp.gameObject.SetActive(false);
            tileTemp = null;
        }
    }

    public void UpdateCountText()
    {
        if (tileCountText != null)
        {
            tileCountText.text = $"{tiles.Count}/{maxTile}";
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
                unlockObject.gameObject.SetActive(true);
                lockObject.gameObject.SetActive(false);
            });

            isLocked = false;

            onCompleteAction?.Invoke();
        }
    }

    public void Complete(ItemCategory nextCategory, Action onCompleteAction)
    {
        isMoving = true;

        Tween.Alpha(category, 0f, duration: 0.2f).OnComplete(() =>
        {
            category.text = this.itemCategory.ToString().Replace("_", " ");
            Tween.Alpha(category, 1f, duration: 0.2f).OnComplete(() =>
            {
                float initialPositionY = originalPositionY;

                Tween.LocalPositionY(transform, initialPositionY + 1600, duration: 0.7f, startDelay: 0.5f)
                .OnComplete(() =>
                {
                    Init(nextCategory, TileManager.Ins.GetTargetCardAmount(nextCategory));
                    category.alpha = 1f;

                    Tween.LocalPositionY(transform, initialPositionY, duration: 0.3f)
                    .OnComplete(() =>
                    {
                        isMoving = false;
                        onCompleteAction?.Invoke();
                    });
                });
            });
        });
    }

    public void Hide(Action onCompleteAction)
    {
        isMoving = true;
        isHidden = true;

        Tween.Alpha(category, 0f, duration: 0.2f).OnComplete(() =>
        {
            category.text = this.itemCategory.ToString().Replace("_", " ");
            Tween.Alpha(category, 1f, duration: 0.2f).OnComplete(() =>
            {
                float initialPositionY = originalPositionY;

                Tween.LocalPositionY(transform, initialPositionY + 1600, duration: 0.7f, startDelay: 0.5f)
                .OnComplete(() =>
                {
                    isMoving = false;
                    onCompleteAction?.Invoke();
                });
            });
        });
    }

    public void SetHighlight(bool isHighlight)
    {
        if (glow != null)
        {
            if (isHighlight)
            {
                glow.gameObject.SetActive(true);
                Tween.Alpha(glow, 1f, duration: 0.3f);
            }
            else
            {
                Tween.Alpha(glow, 0f, duration: 0.3f).OnComplete(() =>
                {
                    glow.gameObject.SetActive(false);
                });
            }
        }
    }
}
