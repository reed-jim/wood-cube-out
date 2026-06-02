using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum UIElement
{
    Background,
    PrimaryButton,
    PrimaryText,
    SecondaryText
}

public class DynamicThemeItem : MonoBehaviour
{
    public UIElement UIElement;

    void Start()
    {
        AssignColor();
    }

    public void AssignColor()
    {
        Color color = DynamicTheme.Ins.GetColorForUIElement(UIElement);

        Image image = GetComponent<Image>();

        if (image != null)
        {
            image.color = color;

            return;
        }

        TMP_Text text = GetComponent<TMP_Text>();

        if (text != null)
        {
            text.color = color;
        }
    }
}
