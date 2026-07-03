using System;
using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static GameEnum;
using Spine.Unity;

public class TileSlotContainer : MonoBehaviour
{
    [SerializeField] private RectTransform categoryContainer;
    [SerializeField] public TMP_Text category;
    [SerializeField] private TMP_Text tileCountText;
    [SerializeField] private Image currentTileImage;
    [SerializeField] private Image fakeTileImage;
    [SerializeField] private TMP_Text currentTileText;
    [SerializeField] private TMP_Text fakeTileText;
    public Image lockImage;
    [SerializeField] private Image glow;
    [SerializeField] private Image glowUnlockTheme;
    [SerializeField] private Image highlightTheme;
    public Image GlowUnlockTheme => glowUnlockTheme;

    [SerializeField] private Image lockObject;
    [SerializeField] private Image unlockObject;
    [SerializeField] private ParticleSystem fxCompleteCard;
    [SerializeField] private ParticleSystem fxCompleteTheme;
    [SerializeField] private ParticleSystem fxUnlock;

    [SerializeField] private SkeletonGraphic skeletonGraphicBack;
    [SerializeField] private SkeletonGraphic skeletonGraphicFront;


    [SerializeField] private GameObject animUnlock;
    [SerializeField] private ItemCategory itemCategory;
    [SerializeField] private bool isActive;
    public bool isLocked;
    public bool isMoving;
    public bool isHidden;
    public bool isCompleted;
    public int maxTile;

    private float timeComplete = 0.15f;
    public List<BaseTile> tiles;

    public TileIconContainer tileIconContainer;

    [Header("Text Config")]
    [Tooltip("Nếu true, chữ hiển thị sẽ tự động viết hoa toàn bộ")]
    [SerializeField] private bool isTextUppercase = true;

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
    private Tween _highlightScaleTween;
    private Tween _highlightShakeTween;
    [SerializeField] private Image wrongGlow;
    private Tween _wrongShakeTween;

    void Awake()
    {
        originalPositionY = transform.localPosition.y;
        originalIsLocked = isLocked;
        BuyBoosterPopup.unlockScrewBoxEvent += Unlock;
        if (animUnlock != null) animUnlock.SetActive(false);
        //if (tileCountText != null)
        //{
        //    Canvas canvas = tileCountText.gameObject.GetComponent<Canvas>();
        //    if (canvas == null)
        //    {
        //        canvas = tileCountText.gameObject.AddComponent<Canvas>();
        //    }
        //    canvas.overrideSorting = true;
        //    canvas.sortingOrder = 2;
        //}

        if (wrongGlow == null)
        {
            wrongGlow = GetComponentsInChildren<Image>(true).FirstOrDefault(img => img.gameObject.name == "Wrong");
        }
        if (wrongGlow != null) wrongGlow.gameObject.SetActive(false);
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

    public void Init(ItemCategory itemCategory = ItemCategory.Fast_Food, int targetCardAmount = 4, bool resetLockState = true)
    {
        ClearTiles();

        if (tileCountText != null)
        {
            Canvas canvas = tileCountText.gameObject.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.overrideSorting = false;
            }
        }

        this.itemCategory = itemCategory;
        this.maxTile = targetCardAmount;
        this.isCompleted = false;
        this.isHidden = false;
        this.isMoving = false;
        if (resetLockState)
        {
            this.isLocked = originalIsLocked;
        }

        if (_highlightScaleTween.isAlive) _highlightScaleTween.Stop();
        if (_wrongShakeTween.isAlive) _wrongShakeTween.Stop();
        if (glow != null) glow.gameObject.SetActive(false);
        if (wrongGlow == null)
        {
            wrongGlow = GetComponentsInChildren<Image>(true).FirstOrDefault(img => img.gameObject.name == "Wrong");
        }
        if (wrongGlow != null) wrongGlow.gameObject.SetActive(false);
        if (unlockObject != null) unlockObject.transform.localScale = Vector3.one;
        category.text = "???";
        category.transform.localScale = Vector3.one;
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

        bool hasIcon;
        bool hasTextOrder = !string.IsNullOrEmpty(iconSet.textOrder);
        bool hasIconOrder;

        if (iconSet.isOnServer)
        {
            // CDN category (index 21+): icons/spriteOrder may be cleared after upload.
            // Consider CDN keys as proof that assets exist.
            hasIcon = (iconSet.icons != null && iconSet.icons.Length > 0)
                      || (iconSet.iconCdnKeys != null && iconSet.iconCdnKeys.Length > 0);
            hasIconOrder = iconSet.spriteOrder != null
                           || !string.IsNullOrEmpty(iconSet.spriteOrderCdnKey);
        }
        else
        {
            // Local category (index 0-20): use sprite references directly.
            hasIcon = iconSet.icons != null && iconSet.icons.Length > 0;
            hasIconOrder = iconSet.spriteOrder != null;
        }

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
                    currentTileText.text = isTextUppercase ? iconSet.textOrder.ToUpper() : iconSet.textOrder;
                }
                else
                {
                    string rndText = containerToUse.GetRandomTextOfCategory(itemCategory);
                    currentTileText.text = isTextUppercase ? rndText.ToUpper() : rndText;
                }
                //AdjustFontSize(currentTileText, currentTileText.text);
            }
        }
        else
        {
            if (currentTileText != null) currentTileText.gameObject.SetActive(false);
            if (currentTileImage != null)
            {
                currentTileImage.gameObject.SetActive(true);

                if (iconSet.isOnServer)
                {
                    // ── CDN path (index 21+) ──────────────────────────────────────
                    // Prefer spriteOrder already loaded in memory
                    if (iconSet.spriteOrder != null)
                    {
                        currentTileImage.sprite = iconSet.spriteOrder;
                    }
                    // Fallback: get spriteOrder from CDN memory cache
                    else if (!string.IsNullOrEmpty(iconSet.spriteOrderCdnKey)
                        && GplayCdn.CdnDownloader.Instance != null)
                    {
                        var cached = GplayCdn.CdnDownloader.Instance.GetCachedSprite(iconSet.spriteOrderCdnKey);
                        if (cached != null)
                        {
                            iconSet.spriteOrder = cached; // write back for next access
                            currentTileImage.sprite = cached;
                        }
                    }
                    // Fallback: use a non-null icon from CDN cache
                    else if (iconSet.iconCdnKeys != null && GplayCdn.CdnDownloader.Instance != null)
                    {
                        foreach (var key in iconSet.iconCdnKeys)
                        {
                            if (string.IsNullOrEmpty(key)) continue;
                            var s = GplayCdn.CdnDownloader.Instance.GetCachedSprite(key);
                            if (s != null) { currentTileImage.sprite = s; break; }
                        }
                    }
                }
                else
                {
                    // ── Local path (index 0-20) ───────────────────────────────────
                    if (iconSet.spriteOrder != null)
                    {
                        currentTileImage.sprite = iconSet.spriteOrder;
                    }
                    else if (iconSet.icons != null && iconSet.icons.Length > 0)
                    {
                        currentTileImage.sprite = iconSet.icons[UnityEngine.Random.Range(0, iconSet.icons.Length)];
                    }
                }
            }
        }

        if (fakeTileImage != null) fakeTileImage.gameObject.SetActive(false);
        if (fakeTileText != null) fakeTileText.gameObject.SetActive(false);

        tileCountText.text = $"{tiles.Count}/{maxTile}";
        if (animUnlock != null) animUnlock.SetActive(false);

        if (skeletonGraphicBack != null)
        {

            var currentAnim = skeletonGraphicBack.AnimationState.GetCurrent(0);
            if (currentAnim == null || currentAnim.Animation.Name != "begin")
            {
                skeletonGraphicBack.Skeleton.SetToSetupPose();
                skeletonGraphicBack.AnimationState.SetAnimation(0, "begin", true);
            }
        }
        if (skeletonGraphicFront != null)
        {

            var currentAnim = skeletonGraphicFront.AnimationState.GetCurrent(0);
            if (currentAnim == null || currentAnim.Animation.Name != "begin")
            {
                skeletonGraphicFront.Skeleton.SetToSetupPose();
                skeletonGraphicFront.AnimationState.SetAnimation(0, "begin", true);
            }
        }
    }

    public void AddTile(BaseTile tile)
    {
        tiles.Add(tile);
        tile.txCount.gameObject.SetActive(false);
    }
    private void PlayFxComplete(BaseTile tile, UnityAction callback = null)
    {
        Color c = tile.glow.color;
        c.a = 0;
        tile.glow.color = c;
        tile.glow.gameObject.SetActive(true);
        if (fxCompleteCard != null)
        {
            fxCompleteCard.Stop();
            fxCompleteCard.Play();
        }
        Tween.Alpha(tile.glow, 1f, duration: timeComplete, ease: Ease.OutQuad).OnComplete(() =>
        {
            Tween.Alpha(tile.glow, 0f, duration: timeComplete, ease: Ease.InQuad).OnComplete(() =>
            {
                callback?.Invoke();
                tile.glow.gameObject.SetActive(false);
                Color c = tile.glow.color;
                c.a = 1;
                tile.glow.color = c;
            });
        });
    }



    private void ScaleCountTextWithTween()
    {
        if (tileCountText != null)
        {
            SettingLayerTextCount();

            tileCountText.text = $"{tiles.Count}/{maxTile}";
            tileCountText.transform.localScale = Vector3.one;
            Tween.Scale(tileCountText.transform, 1.3f, duration: timeComplete, ease: Ease.OutQuad).OnComplete(() =>
            {
                Tween.Scale(tileCountText.transform, 1f, duration: timeComplete, ease: Ease.InQuad).OnComplete(() =>
                {
                    Canvas canvas = tileCountText.gameObject.GetComponent<Canvas>();
                    if (canvas != null)
                    {
                        canvas.overrideSorting = false;
                    }
                });
            });
        }
    }

    private void SettingLayerTextCount()
    {
        if (tileCountText != null)
        {
            Canvas canvas = tileCountText.gameObject.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = tileCountText.gameObject.AddComponent<Canvas>();
            }
            canvas.overrideSorting = true;
            canvas.sortingOrder = 2;
        }
    }

    public void SwitchItem(BaseTile tile)
    {
        SoundManager.Instance.PlaySoundMatchCorrectCard();
        PlayFxComplete(tile);

        ScaleCountTextWithTween();

        Tween.Scale(tile.transform, 1.1f, duration: timeComplete, ease: Ease.OutQuad).OnComplete(() =>
        {
            Tween.Scale(tile.transform, 1f, duration: timeComplete, ease: Ease.InQuad).OnComplete(() =>
            {
                tile.gameObject.SetActive(false);
            });
        });

        if (tile.IsText)
        {
            if (currentTileImage != null) currentTileImage.gameObject.SetActive(false);
            if (fakeTileImage != null) fakeTileImage.gameObject.SetActive(false);

            if (currentTileText != null && fakeTileText != null)
            {
                fakeTileText.gameObject.SetActive(true);
                fakeTileText.text = currentTileText.text;
                fakeTileText.color = Color.white;
                AdjustFontSize(fakeTileText, fakeTileText.text);

                currentTileText.gameObject.SetActive(true);
                currentTileText.text = isTextUppercase ? tile.category.text.ToUpper() : tile.category.text;
                Color txtColor = currentTileText.color;
                currentTileText.color = new Color(txtColor.r, txtColor.g, txtColor.b, 0f);
                //AdjustFontSize(currentTileText, currentTileText.text);

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
                if (tileSlot != null)
                {
                    // Destroy old stacked tiles robustly
                    for (int i = tileSlot.transform.childCount - 1; i >= 0; i--)
                    {
                        Transform child = tileSlot.transform.GetChild(i);
                        if (child.GetComponent<BaseTile>() != null)
                        {
                            Destroy(child.gameObject);
                        }
                    }
                    tileSlot.Tile = null;
                }
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
        if (TileManager.Ins != null && TileManager.Ins.IsAnyTileDragging())
        {
            return;
        }

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

            Tween.Alpha(category, 1, duration: 0.25f);
            Tween.Alpha(lockImage, 0, duration: 0.25f).OnComplete(() =>
            {
                lockImage.gameObject.SetActive(false);
                categoryContainer.gameObject.SetActive(true);
                unlockObject.gameObject.SetActive(true);
                lockObject.gameObject.SetActive(false);
                //SettingLayerTextCount();
                //ScaleCountTextWithTween();
            });

            isLocked = false;

            onCompleteAction?.Invoke();
        }
    }

    public void Complete(ItemCategory nextCategory, Action onCompleteAction)
    {
        Canvas containerCanvas = GetComponent<Canvas>();
        if (containerCanvas == null)
        {
            containerCanvas = gameObject.AddComponent<Canvas>();
        }
        containerCanvas.overrideSorting = true;
        containerCanvas.sortingOrder = 10;

        string oldCategoryName = this.itemCategory.ToString().Replace("_", " ");
        this.itemCategory = nextCategory;
        isMoving = true;
        if (tileCountText != null)
        {
            Canvas canvas = tileCountText.gameObject.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.overrideSorting = false;
            }
        }

        Tween.Alpha(category, 0f, duration: 0.05f).OnComplete(() =>
        {



            float animDelay = 0f;
            if (skeletonGraphicBack != null)
            {
                skeletonGraphicBack.Skeleton.SetToSetupPose();
                var track = skeletonGraphicBack.AnimationState.SetAnimation(0, "action", false);
                if (track != null) animDelay = track.AnimationEnd;
            }
            if (skeletonGraphicFront != null)
            {
                skeletonGraphicFront.Skeleton.SetToSetupPose();
                var track = skeletonGraphicFront.AnimationState.SetAnimation(0, "action", false);
                if (track != null) animDelay = track.AnimationEnd;
            }
            Tween.Delay(animDelay).OnComplete(() =>
            {
                SoundManager.Instance.PlaySoundCompleteTheme();
                HighlightTheme(() =>
                {
                    fxCompleteTheme.Play();
                    category.text = isTextUppercase ? oldCategoryName.ToUpper() : oldCategoryName;
                    Tween.Alpha(category, 1f, duration: 0.05f);
                    category.transform.localScale = Vector3.one;
                    Tween.Scale(category.transform, 1.3f, duration: 0.15f, ease: Ease.OutQuad).OnComplete(() =>
                    {
                        Tween.Scale(category.transform, 1f, duration: 0.15f, ease: Ease.InQuad);
                    });
                }
         );
                if (skeletonGraphicBack != null)
                {
                    skeletonGraphicBack.AnimationState.SetAnimation(0, "end", false);
                }
                if (skeletonGraphicFront != null)
                {
                    skeletonGraphicFront.AnimationState.SetAnimation(0, "end", false);
                }

                Tween.Delay(1f).OnComplete(() =>
            {
                float initialPositionY = originalPositionY;

                Tween.LocalPositionY(transform, initialPositionY - 80f, duration: 0.2f)
                .OnComplete(() =>
                {
                    if (skeletonGraphicBack != null)
                    {
                        skeletonGraphicBack.Skeleton.SetToSetupPose();
                        skeletonGraphicBack.AnimationState.SetAnimation(0, "begin", true);
                    }
                    if (skeletonGraphicFront != null)
                    {
                        skeletonGraphicFront.Skeleton.SetToSetupPose();
                        skeletonGraphicFront.AnimationState.SetAnimation(0, "begin", true);
                    }

                    Tween.LocalPositionY(transform, initialPositionY + 1600f, duration: 0.5f, ease: Ease.InSine)
                    .OnComplete(() =>
                    {
                        Init(nextCategory, TileManager.Ins.GetTargetCardAmount(nextCategory), resetLockState: false);
                        category.alpha = 1f;

                        Tween.LocalPositionY(transform, initialPositionY, duration: 0.3f)
                        .OnComplete(() =>
                        {
                            isMoving = false;
                            Canvas c = GetComponent<Canvas>();
                            if (c != null)
                            {
                                c.overrideSorting = false;
                            }
                            onCompleteAction?.Invoke();
                        });
                    });
                });
            });
            });
        });
    }


    public void Hide(Action onCompleteAction)
    {
        Canvas containerCanvas = GetComponent<Canvas>();
        if (containerCanvas == null)
        {
            containerCanvas = gameObject.AddComponent<Canvas>();
        }
        containerCanvas.overrideSorting = true;
        containerCanvas.sortingOrder = 10;

        isMoving = true;
        isHidden = true;

        if (tileCountText != null)
        {
            Canvas canvas = tileCountText.gameObject.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.overrideSorting = false;
            }
        }
        Tween.Alpha(category, 0f, duration: 0.05f).OnComplete(() =>
        {

            float animDelay = 0f;
            if (skeletonGraphicBack != null)
            {
                skeletonGraphicBack.Skeleton.SetToSetupPose();
                var track = skeletonGraphicBack.AnimationState.SetAnimation(0, "action", false);
                if (track != null) animDelay = track.AnimationEnd;
            }
            if (skeletonGraphicFront != null)
            {
                skeletonGraphicFront.Skeleton.SetToSetupPose();
                var track = skeletonGraphicFront.AnimationState.SetAnimation(0, "action", false);
                if (track != null) animDelay = track.AnimationEnd;
            }

            Tween.Delay(animDelay).OnComplete(() =>
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlaySoundCompleteTheme();

                HighlightTheme(() =>
                {
                    fxCompleteTheme.Play();
                    string catName = this.itemCategory.ToString().Replace("_", " ");
                    category.text = isTextUppercase ? catName.ToUpper() : catName;
                    Tween.Alpha(category, 1f, duration: 0.05f);
                    category.transform.localScale = Vector3.one;
                    Tween.Scale(category.transform, 1.3f, duration: 0.15f, ease: Ease.OutQuad).OnComplete(() =>
                    {
                        Tween.Scale(category.transform, 1f, duration: 0.15f, ease: Ease.InQuad);
                    });
                }
           );
                if (skeletonGraphicBack != null)
                {
                    skeletonGraphicBack.AnimationState.SetAnimation(0, "end", false);
                }
                if (skeletonGraphicFront != null)
                {
                    skeletonGraphicFront.AnimationState.SetAnimation(0, "end", false);
                }

                Tween.Delay(1f).OnComplete(() =>
                {
                    float initialPositionY = originalPositionY;

                    Tween.LocalPositionY(transform, initialPositionY - 80f, duration: 0.2f)
                    .OnComplete(() =>
                    {
                        if (skeletonGraphicBack != null)
                        {
                            skeletonGraphicBack.Skeleton.SetToSetupPose();
                            skeletonGraphicBack.AnimationState.SetAnimation(0, "begin", true);
                        }
                        if (skeletonGraphicFront != null)
                        {
                            skeletonGraphicFront.Skeleton.SetToSetupPose();
                            skeletonGraphicFront.AnimationState.SetAnimation(0, "begin", true);
                        }

                        Tween.LocalPositionY(transform, initialPositionY + 1600f, duration: 0.5f, ease: Ease.InSine)
                        .OnComplete(() =>
                        {
                            isMoving = false;
                            Canvas c = GetComponent<Canvas>();
                            if (c != null)
                            {
                                c.overrideSorting = false;
                            }
                            onCompleteAction?.Invoke();
                        });
                    });
                });
            });
        });
    }

    public void HighlightTheme(UnityAction callback)
    {
        if (highlightTheme != null)
        {
            Color tempColor = highlightTheme.color;

            // Thay đổi giá trị alpha trên biến tạm đó
            tempColor.a = 1f;

            // Gán ngược biến tạm lại cho component
            highlightTheme.color = tempColor;
            Canvas glowCanvas = highlightTheme.GetComponent<Canvas>();
            AdjustGlowSorting(glowCanvas);
            callback?.Invoke();
            Tween.Scale(categoryContainer.transform, 1.07f, duration: 0.15f, ease: Ease.OutQuad).OnComplete(() =>
            {
                Tween.Scale(categoryContainer.transform, 1f, duration: 0.15f, ease: Ease.InQuad).OnComplete(() =>
                {
                    Color tempColor = highlightTheme.color;
                    tempColor.a = 0f;
                    highlightTheme.color = tempColor;
                });
            });
        }
    }

    public void SetHighlight(bool isHighlight)
    {
        if (glow != null)
        {
            if (isHighlight)
            {
                glow.gameObject.SetActive(true);
                Canvas glowCanvas = glow.GetComponent<Canvas>();
                AdjustGlowSorting(glowCanvas);
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

        if (_highlightScaleTween.isAlive) _highlightScaleTween.Stop();

        if (isHighlight)
        {
            Tween.Delay(0.25f).OnComplete(() =>
            {
                if (glow != null && glow.gameObject.activeSelf)
                {
                    StartHighlightScaleLoop();
                    StartHighlightShakeLoop();
                }
            });
        }
        else
        {
            if (unlockObject != null)
            {
                Tween.Scale(unlockObject.transform, Vector3.one, duration: 0.2f, ease: Ease.OutQuad);
            }
        }
    }



    private void StartHighlightScaleLoop()
    {
        if (unlockObject == null || glow == null || !glow.gameObject.activeSelf) return;

        _highlightScaleTween = Tween.Scale(unlockObject.transform, 1.07f, duration: 0.5f, ease: Ease.OutQuad)
            .OnComplete(() =>
            {
                if (glow != null && glow.gameObject.activeSelf && unlockObject != null)
                {
                    _highlightScaleTween = Tween.Scale(unlockObject.transform, 1.0f, duration: 0.25f, ease: Ease.InQuad)
                        .OnComplete(() =>
                        {
                            if (glow != null && glow.gameObject.activeSelf && unlockObject != null)
                            {
                                _highlightScaleTween = Tween.Delay(1.25f).OnComplete(() =>
                                {
                                    StartHighlightScaleLoop();
                                });
                            }
                        });
                }
            });
    }
    private void StartHighlightShakeLoop()
    {
        if (glow == null || !glow.gameObject.activeSelf) return;

        _highlightShakeTween = Tween.ShakeLocalPosition(unlockObject.transform, strength: new Vector3(8f, 8f, 0f), duration: 0.75f, frequency: 7)
            .OnComplete(() =>
            {
                if (glow != null && glow.gameObject.activeSelf)
                {
                    _highlightShakeTween = Tween.Delay(1.25f).OnComplete(() =>
                    {
                        StartHighlightShakeLoop();
                    });
                }
            });
    }

    private void PlayFxUnlock(UnityAction callback)
    {
        Color c = glowUnlockTheme.color;
        c.a = 0;
        glowUnlockTheme.color = c;
        glowUnlockTheme.gameObject.SetActive(true);

        fxUnlock.Play();
        glowUnlockTheme.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        Tween tween = Tween.Scale(glowUnlockTheme.transform, 1f, duration: timeComplete * 2f);
        Tween.Alpha(glowUnlockTheme, 1f, duration: timeComplete * 2f).OnComplete(() =>
        {
            //Tween.Delay(0.5f).OnComplete(() =>
            //{
            Tween.Alpha(glowUnlockTheme, 0f, duration: timeComplete * 2f).OnComplete(() =>
            {
                glowUnlockTheme.gameObject.SetActive(false);
                callback?.Invoke();
            });
            //});
        });

    }

    public void PlayUnlockAnimation(Action onComplete)
    {
        if (animUnlock != null)
        {
            animUnlock.SetActive(true);
            Animator animator = animUnlock.GetComponent<Animator>();
            if (animator != null)
            {
                animator.Play("UnlockBooster", 0, 0f);
                animator.Update(0f);
            }

            Tween.Delay(0.5f).OnComplete(() =>
            {
                //fxUnlock.Play();

                //Tween.Scale(lockObject.transform, 1.2f, duration: timeComplete, ease: Ease.OutQuad).OnComplete(() =>
                //{
                //    Tween.Scale(lockObject.transform, 1f, duration: timeComplete, ease: Ease.InQuad);
                //});
                PlayFxUnlock(() =>
                {
                    //Tween.Delay(0.4f).OnComplete(() =>
                    //{
                    animUnlock.SetActive(false);
                    onComplete?.Invoke();
                    //});
                }
                );


            });
        }
        else
        {
            onComplete?.Invoke();
        }
    }

    private void AdjustFontSize(TMP_Text textComponent, string text)
    {
        if (textComponent == null || string.IsNullOrEmpty(text)) return;

        int length = text.Length;
        float size;
        if (length <= 3) size = 40f;
        else if (length == 4) size = 38f;
        else if (length == 5) size = 36f;
        else if (length == 6) size = 34f;
        else if (length == 7) size = 29f;
        else size = 26f;

        textComponent.fontSize = size;
    }

    public void ShowWrongFeedback()
    {
        if (wrongGlow == null)
        {
            wrongGlow = GetComponentsInChildren<Image>(true).FirstOrDefault(img => img.gameObject.name == "Wrong");
        }

        if (wrongGlow != null)
        {
            wrongGlow.gameObject.SetActive(true);
            Canvas wrongGlowCanvas = wrongGlow.GetComponent<Canvas>();
            AdjustGlowSorting(wrongGlowCanvas);
            Color c = wrongGlow.color;
            c.a = 1f;
            wrongGlow.color = c;
        }

        if (GamePersistentVariable.IsVibrate)
        {
            CandyCoded.HapticFeedback.HapticFeedback.MediumFeedback();
        }

        if (_wrongShakeTween.isAlive) _wrongShakeTween.Stop();

        // Rung nhẹ container
        _wrongShakeTween = Tween.ShakeLocalPosition(transform, strength: new Vector3(10f, 10f, 0f), duration: 0.2f, frequency: 15)
            .OnComplete(() =>
            {
                if (wrongGlow != null)
                {
                    wrongGlow.gameObject.SetActive(false);
                }
            });
    }

    private void AdjustGlowSorting(Canvas canvas)
    {
        if (canvas == null) return;

        bool isTutActive = TileManager.Ins != null && TileManager.Ins.UpToAll != null && TileManager.Ins.UpToAll.gameObject.activeInHierarchy;
        if (isTutActive)
        {
            bool isChildOfUpToAll = transform.parent == TileManager.Ins.UpToAll.transform;
            if (isChildOfUpToAll)
            {
                canvas.overrideSorting = true;
                canvas.sortingOrder = 100;
            }
            else
            {
                canvas.overrideSorting = false;
            }
        }
        else
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = 100;
        }
    }
}
