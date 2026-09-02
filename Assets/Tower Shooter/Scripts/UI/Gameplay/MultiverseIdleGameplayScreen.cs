using System;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class MultiverseIdleGameplayScreen : MonoBehaviour
{
    public Button openAugmentSelectionButton;
    public Button openUpgradeButton;

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
}
