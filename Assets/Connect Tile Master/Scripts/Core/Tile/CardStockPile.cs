using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CardStockPile : Singleton<CardStockPile>
{
    [SerializeField] private TMP_Text numCardLeftText;
    [SerializeField] private Image cardImage; // Có thể bạn không cần dùng nữa nếu đã dùng listCards hoàn toàn
    [SerializeField] private Sprite[] cardSprite;
    [SerializeField] private List<CanvasGroup> listCards;
    public int numCardLeft;

    [Header("Animation Settings")]
    [SerializeField] private float slideOffset = 50f;
    [SerializeField] private float bounceOffset = 20f;
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private float bounceDuration = 0.2f;
    [SerializeField] private float delayDraw = 0.2f;
    public Image stockImg;

    private Vector2 originalCardPos;
    private bool hasCachedOriginalPos = false;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();
        CacheOriginalCardPosition();

        if (listCards != null)
        {
            foreach (var cg in listCards)
            {
                if (cg != null)
                {
                    cg.gameObject.SetActive(true);
                }
            }
        }
    }

    private void CacheOriginalCardPosition()
    {
        if (!hasCachedOriginalPos)
        {
            originalCardPos = cardImage.rectTransform.anchoredPosition;
            hasCachedOriginalPos = true;
        }
    }

    private void OnDisable()
    {
        ResetCardToOriginalState();
    }

    private void ResetCardToOriginalState()
    {
        if (hasCachedOriginalPos)
        {
            cardImage.DOKill();
            cardImage.rectTransform.anchoredPosition = originalCardPos;
            cardImage.color = new Color(cardImage.color.r, cardImage.color.g, cardImage.color.b, 1f);
        }
    }

    public void UpdateText(bool updateAllCardVisibilities = true)
    {
        int numCardLeft = TileManager.Ins.itemCategoryPool.Count;

        if (stockImg != null)
        {
            stockImg.gameObject.SetActive(numCardLeft == 1);
        }

        if (numCardLeft > 0)
        {
            gameObject.SetActive(true);
            ResetCardToOriginalState();

            if (updateAllCardVisibilities)
            {
                ApplyCardVisibility(numCardLeft, false);
            }
        }
        else
        {
            gameObject.SetActive(false);
        }

        numCardLeftText.text = $"{numCardLeft}";
    }

    private void SetCardActive(CanvasGroup cg, bool active, bool animate = true)
    {
        if (cg == null) return;
        cg.DOKill();
        if (active)
        {
            if (animate && Mathf.Approximately(cg.alpha, 0f))
            {
                cg.DOFade(1f, slideDuration);
            }
            else if (!animate)
            {
                cg.alpha = 1f;
            }
        }
        else
        {
            if (animate && Mathf.Approximately(cg.alpha, 1f))
            {
                cg.DOFade(0f, slideDuration);
            }
            else if (!animate)
            {
                cg.alpha = 0f;
            }
        }
    }

    private void ApplyCardVisibility(int numLeft, bool animate)
    {
        if (listCards == null || listCards.Count == 0) return;

        // Determine which index should be active based on thresholds:
        // Index 0: < 3
        // Index 1: <= 5 (meaning 3 <= numLeft <= 5)
        // Index 2: < 10 (meaning 6 <= numLeft < 10)
        // Index 3: >= 10
        int activeIndex = -1;
        if (numLeft < 2)
        {
            activeIndex = -1;
        }
        else if (numLeft < 3)
        {
            activeIndex = 0;
        }
        else if (numLeft <= 5)
        {
            activeIndex = 1;
        }
        else if (numLeft < 10)
        {
            activeIndex = 2;
        }
        else // >= 10
        {
            activeIndex = 3;
        }

        // Clamp activeIndex to the maximum index available in listCards
        activeIndex = Mathf.Min(activeIndex, listCards.Count - 1);

        for (int i = 0; i < listCards.Count; i++)
        {
            if (listCards[i] == null) continue;
            bool shouldBeActive = (i == activeIndex);
            SetCardActive(listCards[i], shouldBeActive, animate);
        }
    }

    public void DrawNewCard(BaseTile toReplaceCard)
    {
        UpdateText(false);
        int numCardLeft = TileManager.Ins.itemCategoryPool.Count;
        ApplyCardVisibility(numCardLeft, true);
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySoundTakeCardFromGrid();

        if (numCardLeft >= 2 && listCards.Count > 1)
        {
            Image cardToFade = cardImage;
            CacheOriginalCardPosition();

            cardToFade.DOKill();

            DOVirtual.DelayedCall(delayDraw, () =>
            {
                cardToFade.rectTransform.anchoredPosition = originalCardPos + new Vector2(0, -slideOffset);
                cardToFade.color = new Color(cardToFade.color.r, cardToFade.color.g, cardToFade.color.b, 0f);
                Sequence seq = DOTween.Sequence();
                seq.Append(cardToFade.rectTransform.DOAnchorPos(originalCardPos, slideDuration).SetEase(Ease.OutQuad));
                seq.Join(cardToFade.DOFade(1f, slideDuration));
                seq.Append(cardToFade.rectTransform.DOAnchorPos(originalCardPos + new Vector2(0, bounceOffset), bounceDuration).SetEase(Ease.OutQuad));
                seq.Append(cardToFade.rectTransform.DOAnchorPos(originalCardPos, bounceDuration).SetEase(Ease.InQuad));
            });
        }
    }
}

