using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Theme
{
    Default,
    Modern,
    Minimalist
}

public class DynamicTheme : Singleton<DynamicTheme>
{
    public Theme currentTheme;

    public ThemeThemeDetailKeyValuePair[] allThemes;

    public Color GetColorForUIElement(UIElement UIElement)
    {
        return allThemes.First(item => item.key == currentTheme).value.colorPerUIElement.First(item => item.key == UIElement).value;
    }
}

[Serializable]
public class ThemeThemeDetailKeyValuePair
{
    public Theme key;
    public ThemeDetail value;
}
