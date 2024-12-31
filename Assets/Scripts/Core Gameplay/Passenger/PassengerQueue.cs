using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dreamteck.Splines;
using PrimeTween;
using UnityEngine;
using static GameEnum;

public class PassengerQueue : MonoBehaviour
{
    [SerializeField] private GameObject passengerPrefab;
    [SerializeField] private Transform container;

    [Header("COUPLING")]
    [SerializeField] private ParkingSlotManager parkingSlotManager;

    private Passenger[] _passengerPool;
    private Queue<Passenger> _passengers;

    [Header("VEHICLE")]
    private List<GameFaction> _passengerFactionPool;
    private List<GameFaction> _remainingPassengersFaction;


    [SerializeField] private SplineComputer pathSpline;

    [SerializeField] private int maxPassenger;

    private List<Tween> _tweens;

    #region ACTION
    public static event Action<int, GameFaction> setPassengerFactionEvent;
    public static event Action<GameFaction> noPassengerLeftForFactionEvent;
    public static event Action<int, CharacterAnimationState> changeAnimationEvent;
    public static event Action<int> updatePassengerLeftEvent;
    #endregion

    private void Awake()
    {
        BaseVehicle.vehicleReachParkingSlotEvent += OnVehicleArrivedParkingSlot;
        VehicleFaction.vehicleFactionSetEvent += AddPassengerFactionPool;
        PassengerCollider.passengerGotInVehicleEvent += OnPassengerGotInVehicle;

        _passengerPool = new Passenger[maxPassenger];
        _passengers = new Queue<Passenger>();
        _passengerFactionPool = new List<GameFaction>();
        _remainingPassengersFaction = new List<GameFaction>();

        _tweens = new List<Tween>();

        DelayInit();
    }

    private void OnDestroy()
    {
        BaseVehicle.vehicleReachParkingSlotEvent -= OnVehicleArrivedParkingSlot;
        VehicleFaction.vehicleFactionSetEvent -= AddPassengerFactionPool;
        PassengerCollider.passengerGotInVehicleEvent -= OnPassengerGotInVehicle;
    }

    private async void DelayInit()
    {
        while (_passengerFactionPool.Count == 0)
        {
            await Task.Delay(200);
        }

        SpawnPassengers();

        await Task.Delay(2000);

        MoveToPosititon();
    }

    private void SpawnPassengers()
    {
        for (int i = 0; i < maxPassenger; i++)
        {
            if (i >= _passengerFactionPool.Count)
            {
                return;
            }

            GameObject passenger = Instantiate(passengerPrefab, container);

            passenger.name = $"Passenger {i}";

            passenger.transform.position = new Vector3(0, 0, 100 + 5 * i);

            Passenger passengerComponent = passenger.GetComponent<Passenger>();

            passengerComponent.SetPathToFollow(pathSpline);

            setPassengerFactionEvent?.Invoke(passenger.GetInstanceID(), GetPassengerFactionFromPool());

            _passengerPool[i] = passengerComponent;
            _passengers.Enqueue(passengerComponent);

            // passenger.gameObject.SetActive(false);
        }
    }

    private void MoveToPosititon()
    {
        Passenger[] passengersArray = _passengers.ToArray();

        float distancePercentBetweenTwoPassenger = 1f / maxPassenger;

        foreach (var item in _tweens)
        {
            if (item.isAlive)
            {
                item.Stop();
            }
        }

        for (int i = 0; i < passengersArray.Length; i++)
        {
            int index = i;

            Passenger passenger = passengersArray[i];

            changeAnimationEvent?.Invoke(passenger.gameObject.GetInstanceID(), CharacterAnimationState.Walking);

            float time = 0.3f + (2 + 0.2f * index) * (1 - passenger.CurrentPathPercent);

            time = Mathf.Min(time, 2.5f);

            Tween tween = Tween.Custom(passenger.CurrentPathPercent, 1 - distancePercentBetweenTwoPassenger * index, duration: time, ease: Ease.Linear, onValueChange: newVal =>
            {
                passenger.PathFollower.SetPercent(newVal);

                passenger.CurrentPathPercent = newVal;
            })
            .OnComplete(() =>
            {
                changeAnimationEvent?.Invoke(passenger.gameObject.GetInstanceID(), CharacterAnimationState.Idle);

                if (passenger.CurrentPathPercent == 1)
                {
                    OnVehicleArrivedParkingSlot();
                }
            });

            _tweens.Add(tween);
        }
    }

    private void MoveInQueue(Passenger passenger)
    {
        Passenger[] passengersArray = _passengers.ToArray();

        float distancePercentBetweenTwoPassenger = 1f / maxPassenger;

        for (int i = 0; i < passengersArray.Length; i++)
        {
            int index = i;

            if (passenger == passengersArray[i])
            {
                changeAnimationEvent?.Invoke(passenger.gameObject.GetInstanceID(), CharacterAnimationState.Walking);

                float time = 0.3f + (2 + 0.2f * index) * (1 - passenger.CurrentPathPercent);

                time = Mathf.Min(time, 2.5f);

                Tween tween = Tween.Custom(passenger.CurrentPathPercent, 1 - distancePercentBetweenTwoPassenger * index, duration: time, ease: Ease.Linear, onValueChange: newVal =>
                {
                    passenger.PathFollower.SetPercent(newVal);

                    passenger.CurrentPathPercent = newVal;
                })
                .OnComplete(() =>
                {
                    passenger.CurrentPathPercent = 1 - distancePercentBetweenTwoPassenger * index;

                    changeAnimationEvent?.Invoke(passenger.gameObject.GetInstanceID(), CharacterAnimationState.Idle);

                    // if (passenger.CurrentPathPercent == 1)
                    // {
                    //     OnVehicleArrivedParkingSlot();
                    // }
                });

                _tweens.Add(tween);

                break;
            }
        }
    }
    
    private async void OnVehicleArrivedParkingSlot()
    {
        while (_passengers.Count > 0)
        {
            bool IsFound = false;

            foreach (var parkingSlot in parkingSlotManager.ParkingSlots)
            {
                BaseVehicle vehicle = parkingSlot.ParkedVehicle;

                if (vehicle == null)
                {
                    continue;
                }

                if (!vehicle.IsParked)
                {
                    continue;
                }

                if (vehicle.IsFull())
                {
                    continue;
                }

                GameFaction faction = vehicle.GetVehicleFaction();

                Passenger passenger = _passengers.Peek();

                if (faction == passenger.GetFaction() && passenger.CurrentPathPercent == 1)
                {
                    _passengers.Dequeue().GetInVehicle(vehicle);
                    vehicle.FillSeat();

                    IsFound = true;

                    await Task.Delay(20);

                    break;
                }

                // if (faction == passenger.GetFaction())
                // {
                //     _passengers.Dequeue().GetInVehicle(vehicle);
                //     vehicle.FillSeat();

                //     IsFound = true;

                //     await Task.Delay(500);

                //     break;
                // }
            }

            if (!IsFound)
            {
                // MoveToPosititon();

                break;
            }

            RespawnPassenger();

            MoveToPosititon();
        }
    }

    private void AddPassengerFactionPool(GameFaction faction, int numberSeat)
    {
        for (int i = 0; i < numberSeat; i++)
        {
            _passengerFactionPool.Add(faction);
            _remainingPassengersFaction.Add(faction);

            updatePassengerLeftEvent?.Invoke(_remainingPassengersFaction.Count);
        }
    }

    private GameFaction GetPassengerFactionFromPool()
    {
        int index = UnityEngine.Random.Range(0, _passengerFactionPool.Count);

        GameFaction faction = _passengerFactionPool[index];

        _passengerFactionPool.RemoveAt(index);

        return faction;
    }

    private void OnPassengerGotInVehicle(Passenger passenger, BaseVehicle vehicle)
    {
        GameFaction faction = vehicle.GetVehicleFaction();

        _remainingPassengersFaction.Remove(faction);

        if (!_remainingPassengersFaction.Contains(faction))
        {
            noPassengerLeftForFactionEvent?.Invoke(faction);
        }

        updatePassengerLeftEvent?.Invoke(_remainingPassengersFaction.Count);






        _passengers.Enqueue(passenger);

        passenger.gameObject.SetActive(true);

        passenger.Reset();


        MoveInQueue(passenger);
    }

    #region POOLING
    private void RespawnPassenger()
    {
        while (_passengerFactionPool.Count > 0)
        {
            bool isPassengerAvailable = false;

            for (int i = 0; i < _passengerPool.Length; i++)
            {
                if (!_passengerPool[i].gameObject.activeSelf)
                {
                    setPassengerFactionEvent?.Invoke(_passengerPool[i].gameObject.GetInstanceID(), GetPassengerFactionFromPool());

                    _passengers.Enqueue(_passengerPool[i]);

                    _passengerPool[i].gameObject.SetActive(true);

                    _passengerPool[i].Reset();

                    isPassengerAvailable = true;

                    break;
                }
            }

            if (!isPassengerAvailable)
            {
                break;
            }
        }
    }
    #endregion
}
