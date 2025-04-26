using UnityEngine;

[CreateAssetMenu(fileName = "Gold Pass Milestone_", menuName = "ScriptableObjects/GoldPassMilestoneData")]
public class GoldPassMilestoneData : ScriptableObject
{
    [SerializeField] private int keyRequired;
    [SerializeField] private bool isPassed;

    public int KeyRequired
    {
        get => keyRequired;
    }

    public bool IsPassed
    {
        get => isPassed;
    }
}
