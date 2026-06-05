using PrimeTween;
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
    
    public Image cachedImage;
    public TMP_Text cachedText;

    void Awake()
    {
        DynamicTheme.changeThemeEvent += OnThemeChanged;
    }

    void OnDestroy()
    {
        DynamicTheme.changeThemeEvent -= OnThemeChanged;
    }

    void Start()
    {
        AssignColor();
    }

    public void AssignColor()
    {
        Color color = DynamicTheme.Ins.GetColorForUIElement(UIElement);

        if (cachedImage != null)
        {
            Tween.Color(cachedImage, color, duration: 0.3f);

            return;
        }
        else
        {
            cachedImage = GetComponent<Image>();

            if (cachedImage != null)
            {
                Tween.Color(cachedImage, color, duration: 0.3f);

                return;
            }
        }

        if (cachedText != null)
        {
            Tween.Color(cachedText, color, duration: 0.3f);

            return;
        }
        else
        {
            cachedText = GetComponent<TMP_Text>();

            if (cachedText != null)
            {
                Tween.Color(cachedText, color, duration: 0.3f);
            }
        }
    }

    private void OnThemeChanged()
    {
        AssignColor();
    }
}
