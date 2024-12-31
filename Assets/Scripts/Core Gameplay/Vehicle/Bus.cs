using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using static GameEnum;

public class Bus : BaseVehicle
{
    [SerializeField] private VehicleFaction vehicleFaction;

    [SerializeField] private PassengerInVehicle[] passengers;

    #region PRIVATE FIELD
    private ParkingSlotManager _parkingSlotManager;
    private ParkingSlot _parkingSlot;
    private float _initialScale;
    private bool _isRotatingToTarget;
    #endregion

    #region ACTION
    public static event Action<BaseVehicle> vehicleLeftParkingSlotEvent;
    #endregion

    private void Awake()
    {
        PassengerQueue.noPassengerLeftForFactionEvent += MoveOut;
        ParkingSlotManager.bindParkingSlotManagerEvent += BindParkingSlotManager;

        _initialScale = transform.localScale.x;

        navMeshAgent.enabled = false;
    }

    private void Update()
    {
        if (_isRotatingToTarget)
        {
            Quaternion targetRotation = Quaternion.Euler(new Vector3(0, -25f, 0));

            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                transform.rotation = targetRotation;

                _isRotatingToTarget = false;
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.2f);
            }
        }
    }

    private void OnDestroy()
    {
        PassengerQueue.noPassengerLeftForFactionEvent -= MoveOut;
        ParkingSlotManager.bindParkingSlotManagerEvent -= BindParkingSlotManager;
    }

    private void BindParkingSlotManager(ParkingSlotManager parkingSlotManager)
    {
        _parkingSlotManager = parkingSlotManager;
    }

    public override GameFaction GetVehicleFaction()
    {
        return vehicleFaction.Faction;
    }

    public override async void Park()
    {
        ParkingSlot emptyParkingSlot = _parkingSlotManager.GetEmptyParkingSlot();

        if (CheckFrontObstacle(out float distance))
        {
            Tween.Position(transform, transform.position + 0.5f * distance * transform.forward, duration: 0.3f, cycles: 2, cycleMode: CycleMode.Yoyo);

            // Tween.Scale(transform, 1.1f * _initialScale, duration: 0.2f, cycles: 6, cycleMode: CycleMode.Yoyo);

            AudioSource hitObstacleSound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.HIT_OBSTACLE_SOUND);

            hitObstacleSound.Play();

            return;
        }

        AudioSource engineSound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.VEHICLE_ENGINE_SOUND);

        engineSound.Play();

        if (emptyParkingSlot != null)
        {
            _parkingSlotManager.ParkVehicle(this);

            navMeshObstacle.enabled = false;

            await Task.Delay(100);

            navMeshAgent.enabled = true;
            navMeshAgent.destination = emptyParkingSlot.transform.position;

            _parkingSlot = emptyParkingSlot;

            StartCoroutine(CheckingReachingParkingSlot());
        }
    }

    private bool CheckFrontObstacle(out float distance)
    {
        RaycastHit obstacle;

        Physics.Raycast(transform.position + new Vector3(0, 0.3f, 0), transform.forward, out obstacle, 10);

        if (obstacle.collider == null)
        {
            distance = 0;


            return false;
        }
        else
        {
            distance = (obstacle.point - transform.position).magnitude;

            return true;
        }
    }

    private IEnumerator CheckingReachingParkingSlot()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.2f);

        yield return new WaitForSeconds(1);

        while (!NavMeshUtil.IsReachedDestination(navMeshAgent))
        {
            yield return waitForSeconds;
        }

        _isRotatingToTarget = true;

        navMeshAgent.enabled = false;

        _isParked = true;

        InvokeVehicleReachParkingSlotEvent();
    }

    private void MoveOut()
    {
        navMeshAgent.enabled = true;

        navMeshAgent.destination = transform.position + new Vector3(50, 0, -20);

        vehicleLeftParkingSlotEvent?.Invoke(this);
    }

    private void MoveOut(GameFaction faction)
    {
        if (faction == GetVehicleFaction())
        {
            MoveOut();
        }
    }

    public override void FillSeat()
    {
        _numberSeatFilled++;
    }

    public override void GetInVehicle()
    {
        if (_confirmedNumberSeatFilled == 0)
        {
            directionArrow.SetActive(false);

            InvokeVehicleRoofFillEvent();
        }

        passengers[_confirmedNumberSeatFilled].gameObject.SetActive(true);
        passengers[_confirmedNumberSeatFilled].SetColor(FactionUtility.GetColorForFaction(vehicleFaction.Faction));

        AudioSource getInVehicleSound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.GET_IN_VEHICLE_SOUND);

        getInVehicleSound.Play();

        _confirmedNumberSeatFilled++;

        if (_confirmedNumberSeatFilled == numberSeat)
        {
            AudioSource busMoveOutSound = ObjectPoolingEverything.GetFromPool<AudioSource>(GameConstants.VEHICLE_MOVE_OUT_SOUND);

            busMoveOutSound.Play();

            MoveOut();
        }

        InvokeGetInVehicleEvent();
    }
}
