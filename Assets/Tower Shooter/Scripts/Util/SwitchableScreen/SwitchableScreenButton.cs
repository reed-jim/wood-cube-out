using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwitchableScreenButton : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button switchButton;
    [SerializeField] private List<ThemeableUI> themeableElements;

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
        if (IsUnlock())
        {
            switchScreenEvent?.Invoke(switchableScreenType);
        }
        else
        {
            IngameNotificationManager.Ins.ShowNotification(GetLockDescription());
        }
    }

    private void OnScreenSwitch(SwitchableScreenType switchableScreenType)
    {
        for (int i = 0; i < themeableElements.Count; i++)
        {
            if (switchableScreenType == this.switchableScreenType)
            {
                themeableElements[i].ChangeRole(ThemeableUI.ElementRole.Primary);
            }
            else
            {
                themeableElements[i].ChangeRole(ThemeableUI.ElementRole.OnSurface);
            }
        }
    }

    protected virtual bool IsUnlock()
    {
        return true;
    }

    protected virtual string GetLockDescription()
    {
        return "";
    }
}
