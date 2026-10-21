using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwitchableScreenButton : MonoBehaviour
{
    [Header("Identifier")]
    [SerializeField] private string id;

    [Header("UI")]
    [SerializeField] private RectTransform container;
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
        FeatureUnlockManager.unlockFeatureEvent += OnUnlockFeature;

        switchButton.onClick.AddListener(SwitchScreen);
    }

    void OnDestroy()
    {
        switchScreenEvent -= OnScreenSwitch;
        FeatureUnlockManager.loadFeatureLockStateDataEvent -= OnLoadFeatureLockStateData;
        FeatureUnlockManager.unlockFeatureEvent -= OnUnlockFeature;
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

    #region Event
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

            SetLockStateVisually(isUnlocked);
        }
        else
        {
            isUnlocked = true;
        }
    }

    private void OnUnlockFeature(FeatureType featureType)
    {
        if (featureType == this.featureType && !IsUnlock())
        {
            UnlockEffectController.Ins.PlayUnlockEffect(container, $"{CommonUtil.ToReadableEnumName(featureType)}");

            SetLockStateVisually(true);
        }
    }
    #endregion

    public void SetLockStateVisually(bool isUnlocked)
    {
        lockOverlay.gameObject.SetActive(!isUnlocked);
    }

    protected virtual bool IsUnlock()
    {
        return FeatureUnlockManager.Ins.IsFeatureUnlocked(featureType);
    }

    protected virtual string GetLockDescription()
    {
        string primaryHex = DynamicThemeManager.Ins.PrimaryHex;

        return $"Reach <color={primaryHex}>{spaceObjectTypeToUnlock}</color> to unlock this feature!";
    }
}
