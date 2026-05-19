using System;
using System.Linq;
using Dreamteck.Utilities;
using PrimeTween;
using UnityEngine;
using static GameEnum;

public class AugmentManager : Singleton<AugmentManager>
{
    public static event Action<AugmentItem[]> showAugmentSelectionEvent;

    [SerializeField] private AugmentItem[] augmentItems;

    void Start()
    {
        Tween.Delay(1).OnComplete(() =>
        {
            StartAugmentSelection();
        });
    }

    private void StartAugmentSelection()
    {
        augmentItems.Shuffle();

        AugmentItem[] randomAugments = augmentItems.Take(3).ToArray();

        for (int i = 0; i < randomAugments.Length; i++)
        {
            AugmentItem augmentItem = randomAugments[i];

            augmentItem.level = UnityEngine.Random.Range(0, 3);
        }

        showAugmentSelectionEvent?.Invoke(randomAugments);
    }
}
