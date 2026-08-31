using System;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class MultiverseIdleGameplayScreen : MonoBehaviour
{
    public Button openAugmentSelectionButton;

    public static event Action<ScreenRoute> switchRouteEvent;

    void Awake()
    {
        openAugmentSelectionButton.onClick.AddListener(OpenAugmentSelection);
    }

    private void OpenAugmentSelection()
    {
        switchRouteEvent?.Invoke(ScreenRoute.AugmentSelection);
    }
}
