using System;
using UnityEngine;
using static ThemeableUI;

public class DynamicThemeManager : Singleton<DynamicThemeManager>
{
    public ThemeDataContainer themeDataContainer;

    [SerializeField] private ThemeConfig currentTheme;
    private ThemeData themeData;

    public static event Action<ThemeConfig> OnThemeChanged;

    public ThemeConfig CurrentTheme => currentTheme;

    void Start()
    {
        LoadThemeData();
    }

    public void SetTheme(ThemeConfig newTheme)
    {
        if (newTheme == null || currentTheme == newTheme) return;

        currentTheme = newTheme;

        OnThemeChanged?.Invoke(currentTheme);
    }

    public void LoadThemeData()
    {
        themeData = DataUtility.Load($"{SaveSlotManager.Ins.currentSaveSlotData}_ThemeData", new ThemeData());

        SetTheme(themeDataContainer.items[themeData.currentThemeIndex]);
    }

    public void SaveThemeData()
    {
        themeData.currentThemeIndex = themeDataContainer.items.IndexOf(currentTheme);

        DataUtility.Save($"{SaveSlotManager.Ins.currentSaveSlotData}_ThemeData", themeData);
    }

    #region Util
    public string GetHex(ElementRole elementRole)
    {
        return ColorUtil.ColorToHex(CurrentTheme.GetColor(elementRole));
    }
    #endregion
}

public class ThemeData
{
    public int currentThemeIndex;
}
