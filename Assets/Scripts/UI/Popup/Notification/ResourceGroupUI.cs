using TMPro;
using UnityEngine;
using Lean.Localization;

public class ResourceGroupUI : MonoBehaviour
{
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private TMP_Text multiplyQuantityText;
    [SerializeField] private RectTransform strikethroughOldPrice;

    public void SetQuantity(int quantity)
    {
        quantityText.text = $"{quantity}";

        // Prevent Localization script from overwriting our quantity text with a translation string
        var localizedText = quantityText.GetComponent<LeanLocalizedTextMeshProUGUI>();
        if (localizedText != null)
        {
            localizedText.enabled = false;
        }
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

            var localizedText = multiplyQuantityText.GetComponent<LeanLocalizedTextMeshProUGUI>();
            if (localizedText != null)
            {
                localizedText.enabled = false;
            }
        }
    }
}
