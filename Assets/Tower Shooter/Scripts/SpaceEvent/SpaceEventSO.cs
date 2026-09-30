using UnityEngine;

[CreateAssetMenu(fileName = "SpaceEvent_", menuName = "Space Game/Event Config")]
public class SpaceEventSO : ScriptableObject
{
    public string eventID;
    public string eventTitle;
    public string eventDescription;
    public Sprite eventIcon;
    public float durationInSeconds;

    // Place your custom buff data/struct here (e.g. SpeedMultiplier, MiningMultiplier)
    public float buffValue;
}
