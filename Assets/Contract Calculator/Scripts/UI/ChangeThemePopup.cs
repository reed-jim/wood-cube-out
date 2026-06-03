using UnityEngine;
using UnityEngine.UI;

public class ChangeThemePopup : BasePopup
{
    public ChangeThemeItem itemPrefab;
    public RectTransform itemContainer;

    void Start()
    {
        Init();
    }

    private void Init()
    {
        ThemeThemeDetailKeyValuePair[] allThemes = DynamicTheme.Ins.allThemes;

        for (int i = 0; i < allThemes.Length; i++)
        {
            ChangeThemeItem item = Instantiate(itemPrefab, itemContainer);

            item.Setup(allThemes[i], onThemeChangedAction: () => Hide());
        }
    }
}
