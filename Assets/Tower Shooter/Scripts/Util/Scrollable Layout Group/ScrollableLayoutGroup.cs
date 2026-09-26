using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScrollableLayoutGroup : MonoBehaviour
{
    public enum LayoutType { Vertical, Horizontal }

    [Header("Layout Settings")]
    [SerializeField] private LayoutType layoutType = LayoutType.Vertical;
    [SerializeField] private float spacing = 10f;
    [SerializeField] private RectOffset padding = new RectOffset();

    [Header("Animation Settings")]
    [SerializeField] private float animDuration = 0.35f;
    [SerializeField] private Ease animEase = Ease.OutCubic;

    private readonly List<RectTransform> items = new List<RectTransform>();
    private RectTransform contentRect;

    private void Awake()
    {
        contentRect = GetComponent<RectTransform>();

        // Ensure proper anchor setups for predictable positioning
        if (layoutType == LayoutType.Vertical)
        {
            contentRect.anchorMin = new Vector2(0.5f, 1f);
            contentRect.anchorMax = new Vector2(0.5f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
        }
        else
        {
            contentRect.anchorMin = new Vector2(0f, 0.5f);
            contentRect.anchorMax = new Vector2(0f, 0.5f);
            contentRect.pivot = new Vector2(0f, 0.5f);
        }
    }

    /// <summary>
    /// Adds a new item to the layout with pop-in and reposition animations.
    /// </summary>
    public void AddItem(GameObject item, Vector3 targetScale, int insertIndex = -1)
    {
        CommonUtil.SetParentAndResetScale(item.transform, transform);
        item.transform.localScale = targetScale;

        RectTransform itemRect = item.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = item.GetComponent<CanvasGroup>() ?? item.AddComponent<CanvasGroup>();

        // Set anchors matching the layout orientation
        SetupItemAnchors(itemRect);

        if (insertIndex < 0 || insertIndex >= items.Count)
        {
            items.Add(itemRect);
        }
        else
        {
            items.Insert(insertIndex, itemRect);
            itemRect.SetSiblingIndex(insertIndex);
        }

        // Target positions for the entire collection after insertion
        Vector2 targetPos = CalculateItemTargetPosition(items.IndexOf(itemRect));

        // Initial invisible state
        itemRect.anchoredPosition = targetPos;
        itemRect.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;

        // Animate new item in
        itemRect.DOScale(targetScale, animDuration).SetEase(animEase);
        canvasGroup.DOFade(1f, animDuration).SetEase(animEase);

        // Animate all existing items to their new offsets
        UpdateLayoutPositions(true);
    }

    public void AddItem(GameObject item)
    {
        AddItem(item, Vector3.one);
    }

    public void ClearAllItems()
    {
        items.Clear();

        UpdateLayoutPositions(true);
    }

    /// <summary>
    /// Removes an item at the specified index with a fade/shrink animation, then adjusts layout.
    /// </summary>
    public void RemoveItemAt(int index)
    {
        if (index < 0 || index >= items.Count) return;

        RectTransform itemToRemove = items[index];
        items.RemoveAt(index);

        // CanvasGroup canvasGroup = itemToRemove.GetComponent<CanvasGroup>();

        // // Sequence removal: Shrink & Fade -> Destroy -> Rearrange Layout
        // Sequence removeSeq = DOTween.Sequence();
        // removeSeq.Join(itemToRemove.DOScale(Vector3.zero, animDuration).SetEase(Ease.InCubic));
        // removeSeq.Join(canvasGroup.DOFade(0f, animDuration).SetEase(Ease.InCubic));

        // removeSeq.OnComplete(() =>
        // {
        //     Destroy(itemToRemove.gameObject);
        // });

        // Immediately start shifting remaining items into their new slots
        UpdateLayoutPositions(true);
    }

    public void RemoveItem(RectTransform item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);

            // Immediately start shifting remaining items into their new slots
            UpdateLayoutPositions(true);
        }
    }

    /// <summary>
    /// Recalculates position for every item in the container and resizes the Content RectTransform.
    /// </summary>
    public void UpdateLayoutPositions(bool animate = true)
    {
        float currentOffset = (layoutType == LayoutType.Vertical) ? padding.top : padding.left;

        for (int i = 0; i < items.Count; i++)
        {
            RectTransform item = items[i];
            Vector2 targetPos;

            if (layoutType == LayoutType.Vertical)
            {
                targetPos = new Vector2(0f, -currentOffset);
                currentOffset += item.rect.height + spacing;
            }
            else
            {
                targetPos = new Vector2(currentOffset, 0f);
                currentOffset += item.rect.width + spacing;
            }

            if (animate)
            {
                item.DOAnchorPos(targetPos, animDuration).SetEase(animEase);
            }
            else
            {
                item.anchoredPosition = targetPos;
            }
        }

        // Add trailing padding
        currentOffset += (layoutType == LayoutType.Vertical) ? padding.bottom : padding.right;

        // Adjust Content size for ScrollRect bounds
        Vector2 sizeDelta = contentRect.sizeDelta;
        if (layoutType == LayoutType.Vertical)
            sizeDelta.y = currentOffset;
        else
            sizeDelta.x = currentOffset;

        if (animate)
            contentRect.DOSizeDelta(sizeDelta, animDuration).SetEase(animEase);
        else
            contentRect.sizeDelta = sizeDelta;
    }

    private Vector2 CalculateItemTargetPosition(int index)
    {
        float offset = (layoutType == LayoutType.Vertical) ? padding.top : padding.left;

        for (int i = 0; i < index; i++)
        {
            if (layoutType == LayoutType.Vertical)
                offset += items[i].rect.height + spacing;
            else
                offset += items[i].rect.width + spacing;
        }

        return (layoutType == LayoutType.Vertical) ? new Vector2(0f, -offset) : new Vector2(offset, 0f);
    }

    private void SetupItemAnchors(RectTransform item)
    {
        if (layoutType == LayoutType.Vertical)
        {
            item.anchorMin = new Vector2(0.5f, 1f);
            item.anchorMax = new Vector2(0.5f, 1f);
            item.pivot = new Vector2(0.5f, 1f);
        }
        else
        {
            item.anchorMin = new Vector2(0f, 0.5f);
            item.anchorMax = new Vector2(0f, 0.5f);
            item.pivot = new Vector2(0f, 0.5f);
        }
    }
}
