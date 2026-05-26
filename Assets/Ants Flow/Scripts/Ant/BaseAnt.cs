using Saferio.Util.SaferioTween;
using UnityEngine;
using UnityEngine.AI;

public class BaseAnt : MonoBehaviour
{
    [SerializeField] private NavMeshAgent navMeshAgent;

    public Transform target;
    private NavMeshPath path;

    void Awake()
    {
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;

        path = new NavMeshPath();
    }

    void Start()
    {
        navMeshAgent.destination = target.position;
    }

    void Update()
    {
        Debug.Log($"Ant {navMeshAgent.CalculatePath(target.position, path)}");
    }
}
