using UnityEngine;
using UnityEngine.AI;

public class CharacterNavigation : MonoBehaviour
{
    [SerializeField] private NavMeshAgent navMeshAgent;

    public void MoveTo(Vector3 position)
    {
        navMeshAgent.destination = position;
    }
}
