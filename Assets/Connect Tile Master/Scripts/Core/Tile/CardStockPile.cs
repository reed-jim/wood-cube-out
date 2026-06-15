using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardStockPile : Singleton<CardStockPile>
{
    [SerializeField] private TMP_Text numCardLeftText;
    [SerializeField] private Image cardImage;
    [SerializeField] private Sprite[] cardSprite;
    public int numCardLeft;

    public void UpdateText()
    {
        int numCardLeft = TileManager.Ins.itemCategoryPool.Count;
        if (numCardLeft > 1)
        {
            gameObject.SetActive(true);
            cardImage.sprite = cardSprite[0];

        }
        else if (numCardLeft == 1)
        {
            gameObject.SetActive(true);
            cardImage.sprite = cardSprite[1];
        }
        else
        {
            gameObject.SetActive(false);
        }
        numCardLeftText.text = $"{TileManager.Ins.itemCategoryPool.Count}";
    }

    public void DrawNewCard(BaseTile toReplaceCard)
    {
        UpdateText();
    }
}
