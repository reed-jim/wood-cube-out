using TMPro;
using UnityEngine;

public class PassengerCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text passengerLeftText;

    private void Awake()
    {
        PassengerQueue.updatePassengerLeftEvent += SetPassengerLeftText;
    }

    private void OnDestroy()
    {
        PassengerQueue.updatePassengerLeftEvent -= SetPassengerLeftText;
    }

    private void SetPassengerLeftText(int passengerLeft)
    {
        passengerLeftText.text = $"{passengerLeft}";
    }
}
