using UnityEngine;
using UnityEngine.UI;

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