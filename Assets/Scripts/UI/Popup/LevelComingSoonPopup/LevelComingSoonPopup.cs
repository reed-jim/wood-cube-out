using System;
using NamPhuThuy.Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class LevelComingSoonPopup : BasePopup
{
    [SerializeField] private Button returnHomeButton;

    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private LevelDataContainer levelDataContainer;

    public static event Action replayLevelEvent;

    protected override void MoreActionInAwake()
    {
        returnHomeButton.onClick.AddListener(ReplayLastLevel);
    }

    private void ReturnHome()
    {
        returnHomeButton.interactable = false;

        Addressables.LoadSceneAsync(GameConstants.MENU_SCENE);
    }

    private void ReplayLastLevel()
    {
        Hide();

        currentLevel.Value = levelDataContainer.MaxLevel;

        replayLevelEvent?.Invoke();
    }
}
