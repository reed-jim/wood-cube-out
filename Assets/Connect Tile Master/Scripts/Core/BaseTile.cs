using System;
using System.Linq;
using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BaseTile : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    [SerializeField] private RectTransform container;
    [SerializeField] private Button selectButton;
    [SerializeField] private Image containerImage;
    [SerializeField] public Image glow;
    [SerializeField] public Image wrongGlow;

    [SerializeField] public Image icon;
    [SerializeField] public TMP_Text category;
    [SerializeField] public TMP_Text txCount;

    [SerializeField] private ItemCategory itemCategory;
    [SerializeField] private bool isMoving;
    public bool isFlipping;
    [SerializeField] private bool isOnValidSlot;
    [SerializeField] private bool isOnSlot;
    [SerializeField] private bool isOutside;
    [SerializeField] private bool isDone;
    [SerializeField] private bool isText;
    [SerializeField] private int themeItemId;

    [Header("Init")]
    [SerializeField] public Image ObjShow;
    [SerializeField] public Image ObjHide;

    [Header("Text Config")]
    [Tooltip("Nếu true, chữ hiển thị sẽ tự động viết hoa toàn bộ")]
    [SerializeField] private bool isTextUppercase = true;

    private float dragYOffset = 100f;

    private Tween _highlightScaleTween;
    private Tween _highlightShakeTween;
    private Tween _wrongShakeTween;


    public static event Action<BaseTile> selectTileEvent;
    public Action onInteract;

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

    public int ThemeItemId
    {
        get => themeItemId;
        set => themeItemId = value;
    }

    public float LeftBound => container.localPosition.x - 0.5f * container.sizeDelta.x;
    public float RightBound => container.localPosition.x + 0.5f * container.sizeDelta.x;
    public float TopBound => container.localPosition.y + 0.5f * container.sizeDelta.y;
    public float BottomBound => container.localPosition.y - 0.5f * container.sizeDelta.y;

    private Vector3 _originalPosition;
    private int _originalSiblingIndex;
    private bool _isDragging;

    public bool IsDragging => _isDragging;

    void Awake()
    {
        // selectButton.onClick.AddListener(Select);
        if (wrongGlow == null)
        {
            wrongGlow = GetComponentsInChildren<Image>(true).FirstOrDefault(img => img.gameObject.name == "Wrong");
        }
    }

    public void Init(ItemCategory itemCategory, int themeItemId, Sprite iconSprite, string textData = null)
    {
        this.itemCategory = itemCategory;
        this.themeItemId = themeItemId;
        txCount.gameObject.SetActive(false);
        isText = !string.IsNullOrEmpty(textData);
        if (iconSprite == null)
        {
            Debug.Log("Icon sprite is null for category: " + itemCategory);
            Debug.Log("isText =" + !string.IsNullOrEmpty(textData));
        }
        if (!string.IsNullOrEmpty(textData))
        {
            icon.gameObject.SetActive(false);
            category.gameObject.SetActive(true);
            category.text = isTextUppercase ? textData.ToUpper() : textData;
            //AdjustFontSize(category, textData);
        }
        else
        {
            icon.gameObject.SetActive(true);
            category.gameObject.SetActive(false);
            icon.sprite = iconSprite;
            //UIUtil.SetSizeKeepRatioY(icon, 0.8f * container.sizeDelta.x);
        }

        containerImage.color = containerImage.color.ChangeAlpha(1);

        IsOnValidSlot = false;
        IsOnSlot = false;
        IsOutside = false;
        IsDone = false;
        isFlipping = false;
        _isDragging = false;

        if (_highlightScaleTween.isAlive) _highlightScaleTween.Stop();
        if (_highlightShakeTween.isAlive) _highlightShakeTween.Stop();
        if (_wrongShakeTween.isAlive) _wrongShakeTween.Stop();
        if (glow != null) glow.gameObject.SetActive(false);
        if (wrongGlow == null)
        {
            wrongGlow = GetComponentsInChildren<Image>(true).FirstOrDefault(img => img.gameObject.name == "Wrong");
        }
        if (wrongGlow != null) wrongGlow.gameObject.SetActive(false);
        transform.localScale = Vector3.one;
        if (ObjHide != null)
        {
            ObjHide.transform.localScale = Vector3.one;
        }
    }

    public void PrepareForShuffle()
    {
        isMoving = true;
        isFlipping = true;
        if (_highlightScaleTween.isAlive) _highlightScaleTween.Stop();
        if (_highlightShakeTween.isAlive) _highlightShakeTween.Stop();
        if (_wrongShakeTween.isAlive) _wrongShakeTween.Stop();
        if (glow != null) glow.gameObject.SetActive(false);
        if (wrongGlow == null)
        {
            wrongGlow = GetComponentsInChildren<Image>(true).FirstOrDefault(img => img.gameObject.name == "Wrong");
        }
        if (wrongGlow != null) wrongGlow.gameObject.SetActive(false);
        transform.localScale = Vector3.one;
    }

    public void FlipFaceDown(float delay, Action onComplete, float valueRotateWhenShuffling = -90f)
    {
        PrepareForShuffle();

        float flipDuration = 0.12f;
        Tween.Delay(delay).OnComplete(() => SoundManager.Instance.PlaySoundFlipCard(), warnIfTargetDestroyed: false);
        Tween.LocalEulerAngles(transform, Vector3.zero, new Vector3(0, valueRotateWhenShuffling, 0), flipDuration, startDelay: delay)
            .OnComplete(() =>
            {
                if (gameObject == null)
                {
                    isFlipping = false;
                    onComplete?.Invoke();
                    return;
                }
                SetFaceUp(false);
                // Xoay góc Y về 0 độ để lá bài phẳng trở lại (đang úp mặt)
                Tween.LocalEulerAngles(transform, new Vector3(0, valueRotateWhenShuffling, 0), Vector3.zero, flipDuration)
                    .OnComplete(() =>
                    {
                        isFlipping = false;
                        onComplete?.Invoke();
                    }, warnIfTargetDestroyed: false);
            }, warnIfTargetDestroyed: false);
    }

    public void FlyToDeck(Vector3 deckLocalPos, Action onComplete)
    {
        if (gameObject == null)
        {
            onComplete?.Invoke();
            return;
        }

        // Di chuyển và giữ nguyên scale
        float distance = Vector3.Distance(transform.localPosition, deckLocalPos);
        float speed = 1500f;
        float moveDuration = Mathf.Max(0.2f, distance / speed);

        Tween.LocalPosition(transform, deckLocalPos, moveDuration, ease: Ease.InQuad).OnComplete(() =>
        {
            SoundManager.Instance.PlaySoundShuffleBooster();
        }
        );
        Tween.Scale(ObjHide.transform, Vector3.one, moveDuration, ease: Ease.InQuad);
        // Xoay nghiêng nửa đường đầu sang (0, 70, -70), nửa đường sau xoay về (0, 0, 0) để không bị méo khi vào bộ bài


        float halfDuration = moveDuration / 2f;
        isFlipping = true;
        Tween.LocalEulerAngles(transform, Vector3.zero, new Vector3(0, 70f, -70f), halfDuration, ease: Ease.OutQuad)
            .OnComplete(() =>
            {
                if (gameObject == null)
                {
                    isFlipping = false;
                    onComplete?.Invoke();
                    return;
                }
                Tween.LocalEulerAngles(transform, new Vector3(0, 70f, -70f), Vector3.zero, halfDuration, ease: Ease.InQuad)
                    .OnComplete(() =>
                    {
                        isFlipping = false;
                        onComplete?.Invoke();
                    }, warnIfTargetDestroyed: false);
            }, warnIfTargetDestroyed: false);
    }

    public void Select()
    {
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        SetHighlight(false);
        onInteract?.Invoke();
        onInteract = null;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (TileManager.Ins != null && !TileManager.Ins.isLevelActive) return;
        if (isFlipping || IsMoving || IsOnSlot || IsDone) return;
        if (ObjHide != null && ObjHide.gameObject.activeSelf) return; // Prevent dragging when card is face down (not flipped yet)
        if (TileManager.Ins != null && TileManager.Ins.IsAnyTileDragging()) return;

        SoundManager.Instance.PlaySoundTakeCardFromGrid();

        if (TileManager.Ins != null)
        {
            TileManager.Ins.CurrentlyDraggedTile = this;
        }

        _isDragging = true;
        _originalPosition = transform.position;
        _originalSiblingIndex = transform.GetSiblingIndex();

        transform.SetAsLastSibling();
        //Tween.Scale(transform, 1.1f, duration: 0.15f);

        Canvas canvas = gameObject.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }
        canvas.overrideSorting = true;
        canvas.sortingOrder = 999;

        GraphicRaycaster raycaster = gameObject.GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            raycaster = gameObject.AddComponent<GraphicRaycaster>();
        }

        // Apply visual offset on begin drag
        Vector2 dragPosition = eventData.position + new Vector2(0f, dragYOffset);
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                (RectTransform)transform.parent,
                dragPosition,
                eventData.pressEventCamera,
                out Vector3 worldPoint))
        {
            transform.position = worldPoint;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        if (IsMoving || IsOnSlot || IsDone) return;

        Vector2 dragPosition = eventData.position + new Vector2(0f, dragYOffset);
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                (RectTransform)transform.parent,
                dragPosition,
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

        if (TileManager.Ins != null && TileManager.Ins.CurrentlyDraggedTile == this)
        {
            TileManager.Ins.CurrentlyDraggedTile = null;
        }

        if (IsMoving || IsOnSlot || IsDone) return;

        Vector2 dragPosition = eventData.position + new Vector2(0f, dragYOffset);
        if (TileManager.Ins.IsHoveringValidContainer(this.itemCategory, dragPosition, eventData.pressEventCamera))
        {
            IsMoving = true;

            TileManager.Ins.MoveTileToSlot(this, onCompleteAction: () =>
            {
                IsMoving = false;
                ResetSorting();
            });

            selectTileEvent?.Invoke(this);
        }
        else
        {
            TileSlotContainer hoveredContainer = TileManager.Ins != null ? TileManager.Ins.GetHoveredContainer(dragPosition, eventData.pressEventCamera) : null;
            bool isWrongTheme = hoveredContainer != null && hoveredContainer.ItemCategory != this.itemCategory;

            //if (isWrongTheme)
            //{
            //}

            SoundManager.Instance.PlaySoundDropCardBackToTable();
            IsMoving = true;
            Tween.Scale(transform, Vector3.one, duration: 0.2f);
            Tween.Position(transform, _originalPosition, duration: 0.2f).OnComplete(() =>
            {
                IsMoving = false;
                transform.SetSiblingIndex(_originalSiblingIndex);
                ResetSorting();

                if (isWrongTheme)
                {
                    hoveredContainer.ShowWrongFeedback();

                    ShowWrongFeedback();
                }
            });
        }
    }

    public void CancelDrag()
    {
        if (!_isDragging) return;
        _isDragging = false;

        if (TileManager.Ins != null && TileManager.Ins.CurrentlyDraggedTile == this)
        {
            TileManager.Ins.CurrentlyDraggedTile = null;
        }

        SoundManager.Instance.PlaySoundDropCardBackToTable();
        IsMoving = true;
        Tween.Scale(transform, Vector3.one, duration: 0.2f);
        Tween.Position(transform, _originalPosition, duration: 0.2f).OnComplete(() =>
        {
            IsMoving = false;
            transform.SetSiblingIndex(_originalSiblingIndex);
            ResetSorting();
        });
    }

    public void ResetSorting()
    {
        Canvas canvas = gameObject.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.overrideSorting = false;
        }
    }

    private void OnDisable()
    {
        _isDragging = false;
        if (TileManager.Ins != null && TileManager.Ins.CurrentlyDraggedTile == this)
        {
            TileManager.Ins.CurrentlyDraggedTile = null;
        }
        ResetSorting();
        Tween.StopAll(transform);
    }

    private void OnDestroy()
    {
        Tween.StopAll(transform);
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
    public void SetHighlight(bool isHighlight)
    {
        if (glow != null)
        {
            glow.gameObject.SetActive(isHighlight);
        }

        if (_highlightScaleTween.isAlive) _highlightScaleTween.Stop();
        if (_highlightShakeTween.isAlive) _highlightShakeTween.Stop();

        if (isHighlight)
        {
            Canvas glowCanvas = glow.GetComponent<Canvas>();
            AdjustGlowSorting(glowCanvas);
            Tween.Delay(0.25f).OnComplete(() =>
            {
                if (gameObject != null && glow != null && glow.gameObject.activeSelf)
                {
                    StartHighlightScaleLoop();
                    StartHighlightShakeLoop();
                }
            });
        }
        else
        {
            Tween.Scale(transform, Vector3.one, duration: 0.2f, ease: Ease.OutQuad);
        }
    }

    private void StartHighlightScaleLoop()
    {
        if (glow == null || !glow.gameObject.activeSelf) return;

        _highlightScaleTween = Tween.Scale(transform, 1.07f, duration: 0.5f, ease: Ease.OutQuad)
            .OnComplete(() =>
            {
                if (glow != null && glow.gameObject.activeSelf)
                {
                    _highlightScaleTween = Tween.Scale(transform, 1.0f, duration: 0.25f, ease: Ease.InQuad)
                        .OnComplete(() =>
                        {
                            if (glow != null && glow.gameObject.activeSelf)
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

        _highlightShakeTween = Tween.ShakeLocalPosition(transform, strength: new Vector3(8f, 8f, 0f), duration: 0.75f, frequency: 7)
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

    private void AdjustFontSize(TMP_Text textComponent, string text)
    {
        if (textComponent == null || string.IsNullOrEmpty(text)) return;

        int length = text.Length;
        float size;
        if (length <= 3) size = 40f;
        else if (length == 4) size = 38f;
        else if (length == 5) size = 36f;
        else if (length == 6) size = 34f;
        else if (length == 7) size = 30f;
        else size = 26f;

        textComponent.fontSize = size;
    }

    public void SetFaceUp(bool isFaceUp)
    {
        if (ObjShow != null) ObjShow.gameObject.SetActive(isFaceUp);
        if (ObjHide != null) ObjHide.gameObject.SetActive(!isFaceUp);
    }

    public void ShowWrongFeedback()
    {
        SoundManager.Instance?.PlaySoundWrongCard();

        if (wrongGlow == null)
        {
            wrongGlow = GetComponentsInChildren<Image>(true).FirstOrDefault(img => img.gameObject.name == "Wrong");
        }

        if (wrongGlow != null)
        {
            Canvas wrongGlowCanvas = wrongGlow.GetComponent<Canvas>();
            AdjustGlowSorting(wrongGlowCanvas);
            wrongGlow.gameObject.SetActive(true);
            Color c = wrongGlow.color;
            c.a = 1f;
            wrongGlow.color = c;
        }

        if (_wrongShakeTween.isAlive) _wrongShakeTween.Stop();

        // Rung nhẹ quân bài sau khi đã bay về chỗ trên grid
        _wrongShakeTween = Tween.ShakeLocalPosition(transform, strength: new Vector3(10f, 10f, 0f), duration: 0.2f, frequency: 15)
            .OnComplete(() =>
            {
                if (wrongGlow != null)
                {
                    wrongGlow.gameObject.SetActive(false);
                }
            });
    }
}
