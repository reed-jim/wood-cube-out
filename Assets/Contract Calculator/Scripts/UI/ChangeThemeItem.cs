using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChangeThemeItem : MonoBehaviour
{
    public Button changeThemeButton;
    public Image background;
    public TMP_Text text;

    private Theme theme;

    void Awake()
    {
        changeThemeButton.onClick.AddListener(SwitchTheme);
    }

    public void Setup(ThemeThemeDetailKeyValuePair theme)
    {
        background.color = theme.value.colorPerUIElement.First(item => item.key == UIElement.PrimaryButton).value;
        text.color = theme.value.colorPerUIElement.First(item => item.key == UIElement.PrimaryText).value;

        this.theme = theme.key;
    }

    private void SwitchTheme()
    {
        DynamicTheme.Ins.ChangeTheme(theme);
    }
}
