using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodeDetailAugmentItemUI : MonoBehaviour
{
    public Image container;
    public Image border;
    public Image background;
    public TMP_Text nameText;
    public TMP_Text rarityText;
    public TMP_Text levelText;
    public Button selectButton;

    private Vector2 containerSize;
    private Vector2 nameTextSize;

    private Augment augment;

    public static event Action<Augment> showAugmentDetailEvent;

    void Awake()
    {
        containerSize = container.rectTransform.sizeDelta;
        nameTextSize = nameText.rectTransform.sizeDelta;

        selectButton.onClick.AddListener(Select);
    }

    public void Init(Augment augment, float expectedWidth = 0)
    {
        AugmentData augmentData = DataUtility.Load(SaveSlotManager.Ins.GetSaveKey($"AugmentData_{augment.id}"), new AugmentData());
        Color qualityColor = ThemeManager.GetRarityColor(augment.augmentQuality);

        border.color = qualityColor;
        // nameText.color = qualityColor;
        rarityText.color = qualityColor;

        nameText.text = augment.augmentName;
        rarityText.text = $"{augment.augmentQuality}";
        levelText.text = $"{augmentData.augmentLevel}";

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

        this.augment = augment;
    }

    public void Select()
    {
        showAugmentDetailEvent?.Invoke(augment);
    }
}
