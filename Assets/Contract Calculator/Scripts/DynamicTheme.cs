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

    public static event Action changeThemeEvent;

    void OnValidate()
    {
        ChangeTheme();
    }

    public Color GetColorForUIElement(UIElement UIElement)
    {
        return allThemes.First(item => item.key == currentTheme).value.colorPerUIElement.First(item => item.key == UIElement).value;
    }

    public void ChangeTheme()
    {
        changeThemeEvent?.Invoke();
    }

    public void ChangeTheme(Theme theme)
    {
        currentTheme = theme;
        
        changeThemeEvent?.Invoke();
    }
}

[Serializable]
public class ThemeThemeDetailKeyValuePair
{
    public Theme key;
    public ThemeDetail value;
}
