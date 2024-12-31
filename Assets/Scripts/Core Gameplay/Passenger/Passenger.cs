using System;
using System.Threading.Tasks;
using Dreamteck.Splines;
using UnityEngine;
using UnityEngine.AI;
using static GameEnum;

public class Passenger : MonoBehaviour
{
    [SerializeField] private PassengerFaction passengerFaction;

    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private SplineFollower pathFollower;

    #region PRIVATE FIELD
    private float _currentPathPercent;
    #endregion

    #region ACTION
    public static event Action<int, CharacterAnimationState> changeAnimationEvent;
    #endregion

    public NavMeshAgent NavMeshAgent
    {
        get => navMeshAgent;
    }

    public SplineFollower PathFollower
    {
        get => pathFollower;
        set => pathFollower = value;
    }

    public float CurrentPathPercent
    {
        get => _currentPathPercent;
        set => _currentPathPercent = value;
    }

    private void Awake()
    {
        pathFollower.follow = false;
    }

    private void OnDestroy()
    {

    }

    public void Reset()
    {
        navMeshAgent.enabled = false;

        _currentPathPercent = 0;

        pathFollower.SetPercent(_currentPathPercent);
    }

    public void SetPathToFollow(SplineComputer spline)
    {
        pathFollower.spline = spline;
    }

    public GameFaction GetFaction()
    {
        return passengerFaction.Faction;
    }

    public async void GetInVehicle(BaseVehicle vehicle)
    {
        // navMeshAgent.isStopped = false;
        navMeshAgent.enabled = true;

        navMeshAgent.destination = vehicle.transform.position;

        changeAnimationEvent?.Invoke(gameObject.GetInstanceID(), CharacterAnimationState.Walking);
    }
}
