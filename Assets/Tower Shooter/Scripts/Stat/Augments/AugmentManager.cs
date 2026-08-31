using System.Collections.Generic;
using UnityEngine;

public class AugmentManager : Singleton<AugmentManager>
{
    public CharacterStats playerCharacterStats;

    public List<Augment> augments;

    void Start()
    {
        Init();
    }

    public void Init()
    {
        augments = new List<Augment>();
    }

    public void ApplyAugments()
    {
        for (int i = 0; i < augments.Count; i++)
        {
            Augment augment = augments[i];

            augment.Apply(playerCharacterStats);
        }
    }

    public void AddAugment(Augment augment)
    {
        augments.Add(augment);

        ApplyAugments();
    }

    public void RemoveAugment(Augment augment)
    {
        augment.Remove(playerCharacterStats);
        augments.Remove(augment);
    }

    public bool IsAugmentEquipped(Augment augment)
    {
        return augments.Contains(augment);
    }
}
