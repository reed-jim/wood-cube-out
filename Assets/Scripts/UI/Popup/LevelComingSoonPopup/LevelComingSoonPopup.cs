using NamPhuThuy.GamePlay;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class LevelComingSoonPopup : BasePopup
{
    [SerializeField] private Button okButton;
    // Keep returnHomeButton for backward compatibility but repurpose it as OK if okButton is not assigned
    [SerializeField] private Button returnHomeButton;
    [SerializeField] private IntVariable currentLevel;

    protected override void MoreActionInAwake()
    {
        LevelLoader.showLevelComingSoonEvent += OnShowLevelComingSoon;

        if (okButton != null)
        {
            okButton.onClick.AddListener(OnOkPressed);
        }
        else if (returnHomeButton != null)
        {
            returnHomeButton.onClick.AddListener(OnOkPressed);
        }
    }

    protected override void UnregisterMoreEvent()
    {
        LevelLoader.showLevelComingSoonEvent -= OnShowLevelComingSoon;
    }

    private void OnShowLevelComingSoon()
    {
        Debug.Log("[LevelComingSoonPopup] OnShowLevelComingSoon event received. Showing popup...");
        if (okButton != null) okButton.interactable = true;
        if (returnHomeButton != null) returnHomeButton.interactable = true;
        Show();
    }

    private void OnOkPressed()
    {
        if (okButton != null) okButton.interactable = false;
        if (returnHomeButton != null) returnHomeButton.interactable = false;

        HideImmediately();

        if (currentLevel != null)
        {
            int maxLevel = 15;
            if (LevelManager.Ins != null && LevelManager.Ins.LevelDataContainer != null)
            {
                maxLevel = LevelManager.Ins.LevelDataContainer.MaxLevel;
            }

            currentLevel.Value = maxLevel;
            currentLevel.Save();
            GamePersistentVariable.currentLevel = maxLevel;
            
            // Trigger GoLevel to load the max level
            WinPopup.TriggerGoLevel(maxLevel);
        }
    }
}
