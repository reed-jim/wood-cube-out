using UnityEngine;

[CreateAssetMenu(fileName = "gold_pass_milestone_", menuName = "ScriptableObjects/GoldPassMilestoneData")]
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
