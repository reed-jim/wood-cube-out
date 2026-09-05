using System.Collections.Generic;
using UnityEngine;

public class AugmentManager : Singleton<AugmentManager>
{
    public BaseCharacter player;


    public void ApplyAugment(Augment augment)
    {
        if (augment.isNeedCharacterToApply)
        {

        }
        else
        {
            augment.Apply(null);
        }
    }

    public void AddAugment(Augment augment)
    {
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
