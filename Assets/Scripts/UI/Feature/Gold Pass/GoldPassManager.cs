using UnityEngine;

public class GoldPassManager : MonoBehaviour
{
    public static GoldPassManager Instance;

    [SerializeField] private GoldPassMilestoneData[] goldPassMilestones;
    [SerializeField] private RewardBundleData[] rewards;

    private GoldPassMilestoneItem[] goldPassMilestoneItems;

    private void Setup() {

    }
}
