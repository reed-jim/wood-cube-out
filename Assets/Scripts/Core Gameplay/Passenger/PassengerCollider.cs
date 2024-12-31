using System;
using com.unity3d.mediation;
using UnityEngine;

public class PassengerCollider : MonoBehaviour
{
    [SerializeField] private PassengerFaction passengerFaction;

    #region ACTION
    public static event Action<Passenger, BaseVehicle> passengerGotInVehicleEvent;
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        BaseVehicle vehicle = other.GetComponentInParent<BaseVehicle>();

        if (vehicle != null)
        {
            if (vehicle.GetVehicleFaction() == passengerFaction.Faction && vehicle.IsParked)
            {
                gameObject.SetActive(false);

                vehicle.GetInVehicle();

                passengerGotInVehicleEvent?.Invoke(gameObject.GetComponent<Passenger>(), vehicle);
            }
        }
    }
}
