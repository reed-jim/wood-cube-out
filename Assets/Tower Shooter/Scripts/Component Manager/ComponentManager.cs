using UnityEngine;

public class ComponentManager : MonoBehaviour
{
    public NodeManager nodeManager;
    public LeadershipManager leadershipManager;

    void Start()
    {
        nodeManager.Init();
        leadershipManager.LoadData();
    }
}
