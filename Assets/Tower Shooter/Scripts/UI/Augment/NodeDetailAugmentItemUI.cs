using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodeDetailAugmentItemUI : MonoBehaviour
{
    public Image container;
    public TMP_Text nameText;

    private Vector2 containerSize;
    private Vector2 nameTextSize;

    void Awake()
    {
        containerSize = container.rectTransform.sizeDelta;
        nameTextSize = nameText.rectTransform.sizeDelta;
    }

    public void Init(Augment augment, float expectedWidth = 0)
    {
        Color qualityColor = ThemeManager.GetRarityColor(augment.augmentQuality);

        container.color = qualityColor;
        nameText.color = 0.5f * qualityColor;

        nameText.text = augment.augmentName;

        if (expectedWidth > 0)
        {
            float ratio = expectedWidth / containerSize.x;
            container.rectTransform.sizeDelta = new Vector2(expectedWidth, containerSize.y);
            nameText.rectTransform.sizeDelta = new Vector2(ratio * nameTextSize.x, nameTextSize.y);
        }
        else
        {
            container.rectTransform.sizeDelta = containerSize;
            nameText.rectTransform.sizeDelta = nameTextSize;
        }
    }
}
