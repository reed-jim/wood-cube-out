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
    [SerializeField] private List<Image> listCards;
    public int numCardLeft;

    public void UpdateText()
    {
        int numCardLeft = TileManager.Ins.itemCategoryPool.Count;

        if (numCardLeft > 0)
        {
            gameObject.SetActive(true);
            for (int i = 0; i < listCards.Count; i++)
            {
                if (i == 0)
                {
                    listCards[i].gameObject.SetActive(true);
                }
                else
                {
                    listCards[i].gameObject.SetActive(i < numCardLeft);
                }
            }
        }
        else
        {
            gameObject.SetActive(false);
        }

        numCardLeftText.text = $"{numCardLeft}";
    }

    public void DrawNewCard(BaseTile toReplaceCard)
    {
        UpdateText();
        int numCardLeft = TileManager.Ins.itemCategoryPool.Count;

        if (numCardLeft >= 3 && listCards.Count > 1)
        {
            Image cardToFade = listCards[0];

            cardToFade.DOKill();
            cardToFade.color = new Color(cardToFade.color.r, cardToFade.color.g, cardToFade.color.b, 1f);

            Sequence seq = DOTween.Sequence();
            seq.Append(cardToFade.DOFade(0f, 1f));
            seq.Append(cardToFade.DOFade(1f, 1f));
        }
    }
}
