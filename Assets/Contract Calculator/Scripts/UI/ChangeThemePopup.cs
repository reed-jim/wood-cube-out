using UnityEngine;
using UnityEngine.UI;

public class ChangeThemePopup : BasePopup
{
    public ChangeThemeItem itemPrefab;
    public RectTransform itemContainer;

    protected override void MoreActionInAwake()
    {
        base.MoreActionInAwake();

        Init();
    }

    private void Init()
    {
        ThemeThemeDetailKeyValuePair[] allThemes = DynamicTheme.Ins.allThemes;

        for (int i = 0; i < allThemes.Length; i++)
        {
            Instantiate(itemPrefab, itemContainer);
        }
    }
}
