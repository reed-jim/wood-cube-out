using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class MultiverseIdleGameplayScreen : MonoBehaviour
{
    public Button openAugmentSelectionButton;
    public Button openUpgradeButton;

    [Header("Resource")]
    public TMP_Text upgradePointText;

    public static event Action<ScreenRoute> switchRouteEvent;

    void Awake()
    {
        openAugmentSelectionButton.onClick.AddListener(OpenAugmentSelection);
        openUpgradeButton.onClick.AddListener(OpenUpgrade);
    }

    private void OpenAugmentSelection()
    {
        switchRouteEvent?.Invoke(ScreenRoute.AugmentSelection);
    }

    private void OpenUpgrade()
    {
        switchRouteEvent?.Invoke(ScreenRoute.Upgrade);
    }

    public void UpdateUpgradePointText(int upgradePoint)
    {
        upgradePointText.text = $"{upgradePoint}";
    }
}
