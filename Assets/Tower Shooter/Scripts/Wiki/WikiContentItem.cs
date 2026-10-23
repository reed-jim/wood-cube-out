using UnityEngine;
using UnityEngine.UI;
using TMPro;

// --- Abstract Base Class ---
public abstract class WikiContentItem : MonoBehaviour
{
    public abstract void Setup(WikiBlock blockData);
}

// --- Text-Based Content Items (Headers, Paragraphs, Bullet Points) ---
public class WikiTextItem : WikiContentItem
{
    [SerializeField] private TextMeshProUGUI textDisplay;

    public override void Setup(WikiBlock blockData)
    {
        if (textDisplay != null)
        {
            textDisplay.text = blockData.textContent;
        }
    }
}

// --- Image Content Item ---
public class WikiImageItem : WikiContentItem
{
    [SerializeField] private Image imageDisplay;
    [SerializeField] private AspectRatioFitter aspectFitter; // Optional auto-aspect sizing

    public override void Setup(WikiBlock blockData)
    {
        if (imageDisplay != null && blockData.imageSprite != null)
        {
            imageDisplay.sprite = blockData.imageSprite;
            imageDisplay.gameObject.SetActive(true);

            // Optional: Maintain natural aspect ratio automatically
            if (aspectFitter != null && blockData.imageSprite.rect.height > 0)
            {
                float aspectRatio = blockData.imageSprite.rect.width / blockData.imageSprite.rect.height;
                aspectFitter.aspectRatio = aspectRatio;
            }
        }
    }
}