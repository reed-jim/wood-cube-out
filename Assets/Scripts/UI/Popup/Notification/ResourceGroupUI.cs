using TMPro;
using UnityEngine;

public class ResourceGroupUI : MonoBehaviour
{
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private TMP_Text multiplyQuantityText;
    [SerializeField] private RectTransform strikethroughOldPrice;

    public void SetQuantity(int quantity)
    {
        quantityText.text = $"{quantity}";
    }

    public void SetMultiplyQuantity(int quantity)
    {
        if (multiplyQuantityText != null)
        {
            strikethroughOldPrice.sizeDelta = new Vector2(
                0.8f * quantityText.preferredWidth,
                strikethroughOldPrice.sizeDelta.y
            );

            multiplyQuantityText.text = $"{quantity}";
        }
    }
}
