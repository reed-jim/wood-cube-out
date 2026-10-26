using TMPro;
using UnityEngine;

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
