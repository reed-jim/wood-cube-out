using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwitchableScreenButton : MonoBehaviour
{
    [Header("Identifier")]
    [SerializeField] private string id;

    [Header("UI")]
    [SerializeField] private Button switchButton;
    [SerializeField] private List<ThemeableUI> themeableElements;

    [Header("Lock State")]
    [SerializeField] private CanvasGroup lockOverlay;
    [SerializeField] private FeatureType featureType;

    [Header("Customize")]
    public SwitchableScreenType switchableScreenType;

    private bool isUnlocked;
    private SpaceObjectType spaceObjectTypeToUnlock;

    public static event Action<SwitchableScreenType> switchScreenEvent;

    void Awake()
    {
        switchScreenEvent += OnScreenSwitch;
        FeatureUnlockManager.loadFeatureLockStateDataEvent += OnLoadFeatureLockStateData;

        switchButton.onClick.AddListener(SwitchScreen);
    }

    void OnDestroy()
    {
        switchScreenEvent -= OnScreenSwitch;
        FeatureUnlockManager.loadFeatureLockStateDataEvent -= OnLoadFeatureLockStateData;
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

    private void OnLoadFeatureLockStateData()
    {
        if (lockOverlay != null)
        {
            isUnlocked = FeatureUnlockManager.Ins.IsFeatureUnlocked(featureType, out spaceObjectTypeToUnlock);

            lockOverlay.gameObject.SetActive(!isUnlocked);
        }
        else
        {
            isUnlocked = true;
        }
    }

    protected virtual bool IsUnlock()
    {
        return isUnlocked;
    }

    protected virtual string GetLockDescription()
    {
        string primaryHex = DynamicThemeManager.Ins.PrimaryHex;

        return $"Reach <color={primaryHex}>{spaceObjectTypeToUnlock}</color> to unlock this feature!";
    }
}
