using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AugmentContainer", menuName = "Stats/AugmentContainer")]
public class AugmentContainer : ScriptableObject
{
    public List<Augment> augments;
}
