using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AugmentSetBuffConfig", menuName = "Idle/Augment/AugmentSetBuffConfig")]
public class AugmentSetBuffConfig : ScriptableObject
{
    public SpaceObjectType spaceObjectType;
    public int numSameSpaceObjectTypeRequired;

    public Augment buff;
}
