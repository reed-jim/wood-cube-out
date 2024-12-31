using UnityEngine;

public class VehicleServiceLocator : MonoBehaviour
{
    [SerializeField] private BaseVehicle vehicle;

    public BaseVehicle Vehicle
    {
        get => vehicle;
    }
}
