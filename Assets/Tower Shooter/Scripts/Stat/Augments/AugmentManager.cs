using System.Collections.Generic;
using UnityEngine;

public class AugmentManager : Singleton<AugmentManager>
{
    public BaseCharacter player;

    void Start()
    {
        Init();
    }

    public void Init()
    {
        player.ownedAugments = new List<Augment>();
    }

    public void ApplyAugments()
    {
        for (int i = 0; i < player.ownedAugments.Count; i++)
        {
            Augment augment = player.ownedAugments[i];

            augment.Apply(player.stats);
        }
    }

    public void AddAugment(Augment augment)
    {
        ApplyAugments();

        player.ownedAugments.Add(augment);
    }

    public void RemoveAugment(Augment augment)
    {
        augment.Remove(player.stats);

        player.ownedAugments.Remove(augment);
    }

    public bool IsAugmentEquipped(Augment augment)
    {
        return player.ownedAugments.Contains(augment);
    }
}
