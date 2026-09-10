using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AugmentSetContainer", menuName = "Idle/Augment/AugmentSetContainer")]
public class AugmentSetContainer : ScriptableObject
{
    public SerializableDictionary<SpaceObjectType, List<Augment>> items;
}
