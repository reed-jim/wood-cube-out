using System.Collections.Generic;
using UnityEngine;

public class AugmentManager : Singleton<AugmentManager>
{
    public CharacterStats playerCharacterStats;

    public List<Augment> augments;

    void Start()
    {
        ApplyAugments();
    }

    public void ApplyAugments()
    {
        for (int i = 0; i < augments.Count; i++)
        {
            Augment augment = augments[i];

            augment.Apply(playerCharacterStats);
        }
    }
}
