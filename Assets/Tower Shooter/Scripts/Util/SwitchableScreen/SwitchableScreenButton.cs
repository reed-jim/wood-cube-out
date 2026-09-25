using System;
using UnityEngine;
using UnityEngine.UI;

public class SwitchableScreenButton : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button switchButton;
    [SerializeField] private ThemeableUI buttonThemeable;

    [Header("Customize")]
    public SwitchableScreenType switchableScreenType;

    public static event Action<SwitchableScreenType> switchScreenEvent;

    void Awake()
    {
        switchScreenEvent += OnScreenSwitch;

        switchButton.onClick.AddListener(SwitchScreen);
    }

    void OnDestroy()
    {
        switchScreenEvent -= OnScreenSwitch;
    }

    private void SwitchScreen()
    {
        switchScreenEvent?.Invoke(switchableScreenType);
    }

    private void OnScreenSwitch(SwitchableScreenType switchableScreenType)
    {
        if (switchableScreenType == this.switchableScreenType)
        {
            buttonThemeable.ChangeRole(ThemeableUI.ElementRole.Primary);
        }
        else
        {
            buttonThemeable.ChangeRole(ThemeableUI.ElementRole.OnSurface);
        }
    }
}
