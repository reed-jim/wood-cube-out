using TMPro;
using UnityEngine;

public class CardStockPile : Singleton<CardStockPile>
{
    [SerializeField] private TMP_Text numCardLeftText;

    public int numCardLeft;

    public void UpdateText()
    {
        numCardLeftText.text = $"{TileManager.Ins.itemCategoryPool.Count}";
    }

    public void DrawNewCard(BaseTile toReplaceCard)
    {
        UpdateText();
    }
}
