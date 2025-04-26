using UnityEngine;

public enum GameResourceType
{
    Coin,
    AddHoleBooster,
    BreakObjectBooster,
    ClearHolesBooster
}

[CreateAssetMenu(fileName = "reward_", menuName = "ScriptableObjects/RewardBundleData")]
public class RewardBundleData : ScriptableObject
{
    [SerializeField] private GameResourceType resourceType;
    [SerializeField] private int quantity;

    public GameResourceType ResourceType
    {
        get => resourceType;
    }

    public int Quantity
    {
        get => quantity;
    }
}
