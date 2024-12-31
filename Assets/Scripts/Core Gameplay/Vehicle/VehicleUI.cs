using TMPro;
using UnityEngine;

public class VehicleUI : MonoBehaviour
{
    [SerializeField] private VehicleServiceLocator vehicleServiceLocator;

    [SerializeField] private TMP_Text seatLeftText;

    private void Awake()
    {
        BaseVehicle.getInVehicleEvent += SetSeatLeft;

        seatLeftText.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        BaseVehicle.getInVehicleEvent -= SetSeatLeft;
    }

    private void SetSeatLeft(int instanceId)
    {
        if (instanceId == gameObject.GetInstanceID())
        {
            if (!seatLeftText.gameObject.activeSelf)
            {
                seatLeftText.gameObject.SetActive(true);
            }

            seatLeftText.text = $"{vehicleServiceLocator.Vehicle.ConfirmedNumberSeatFilled}/{vehicleServiceLocator.Vehicle.NumberSeat}";
        }
    }
}
