using TMPro;
using UnityEngine;

public class CardStockPile : Singleton<CardStockPile>
{
    [SerializeField] private TMP_Text numCardLeftText;

    public int numCardLeft;

    public void DrawNewCard(BaseTile toReplaceCard)
    {
        numCardLeftText.text = $"{TileManager.Ins.itemCategoryPool.Count}";
    }
}
