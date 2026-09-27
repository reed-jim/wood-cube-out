using UnityEngine;

public class ComponentManager : MonoBehaviour
{
    public NodeManager nodeManager;
    public LeadershipManager leadershipManager;
    public OfflineIdleManager offlineIdleManager;

    void Start()
    {
        nodeManager.Init();
        leadershipManager.LoadData();
        offlineIdleManager.Init();
    }
}
