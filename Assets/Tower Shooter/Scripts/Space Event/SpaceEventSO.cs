using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpaceEvent_", menuName = "Space Game/Event Config")]
public class SpaceEventSO : ScriptableObject
{
    public string eventID;
    public string eventTitle;
    public string eventDescription;
    public Sprite eventIcon;
    public float durationInSeconds;

    public List<StatModifierData> buffs;
}
