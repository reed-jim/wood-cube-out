using System;
using System.Linq;
using Dreamteck.Utilities;
using UnityEngine;
using static GameEnum;

public class AugmentManager : Singleton<AugmentManager>
{
    public static event Action<AugmentItem[]> showAugmentSelectionEvent;

    private AugmentItem[] augmentItems;

    private void StartAugmentSelection()
    {
        augmentItems.Shuffle();

        AugmentItem[] randomAugments = augmentItems.Take(3).ToArray();

        showAugmentSelectionEvent?.Invoke(randomAugments);
    }
}
