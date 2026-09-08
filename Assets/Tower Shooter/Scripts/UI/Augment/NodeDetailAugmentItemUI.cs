using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodeDetailAugmentItemUI : MonoBehaviour
{
    public Image container;
    public TMP_Text nameText;

    public void Init(Augment augment)
    {
        Color qualityColor = ThemeManager.GetRarityColor(augment.augmentQuality);

        container.color = qualityColor;
        nameText.color = 0.5f * qualityColor;

        nameText.text = augment.augmentName;
    }
}
