using UnityEngine;

public class ParkingSlot : MonoBehaviour
{
    private BaseVehicle _parkedVehicle;

    public BaseVehicle ParkedVehicle
    {
        get => _parkedVehicle;
        set => _parkedVehicle = value;
    }

    public bool IsEmpty
    {
        get => _parkedVehicle == null;
    }

    private void Awake()
    {
        Bus.vehicleLeftParkingSlotEvent += VehicleLeft;
    }

    private void OnDestroy()
    {
        Bus.vehicleLeftParkingSlotEvent -= VehicleLeft;
    }

    public void VehicleLeft(BaseVehicle vehicle)
    {
        if (vehicle == _parkedVehicle)
        {
            _parkedVehicle = null;
        }
    }
}
