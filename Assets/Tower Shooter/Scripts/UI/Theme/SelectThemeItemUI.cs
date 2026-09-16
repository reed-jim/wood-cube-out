using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectThemeItemUI : PageViewItem
{
    [SerializeField] private TMP_Text themeName;
    [SerializeField] private Image[] themePreviewImages;
    [SerializeField] private Button selectThemeButton;

    public ThemeDataContainer themeDataContainer;

    private ThemeConfig themeConfig;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        selectThemeButton.onClick.AddListener(SelectTheme);
    }

    public override void Setup(int index)
    {
        if (index >= themeDataContainer.items.Count)
        {
            gameObject.SetActive(false);

            return;
        }

        gameObject.SetActive(true);

        themeConfig = themeDataContainer.items[index];

        themeName.text = themeConfig.name;

        themePreviewImages[0].color = themeConfig.primary;
        themePreviewImages[1].color = themeConfig.onPrimary;
        themePreviewImages[2].color = themeConfig.surface;
    }

    private void SelectTheme()
    {
        DynamicThemeManager.Ins.SetTheme(themeConfig);
    }
}
