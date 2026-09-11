using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AugmentSetBuffConfigContainer", menuName = "Idle/Augment/AugmentSetBuffConfigContainer")]
public class AugmentSetBuffConfigContainer : ScriptableObject
{
    public List<AugmentSetBuffConfig> items;
}
