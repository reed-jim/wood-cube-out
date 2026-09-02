using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;

public class SpaceShip : MonoBehaviour
{
    public Transform start;
    public Transform end;

    [Header("Voyage")]
    public float totalKm;
    public float remaningKm;
    public float kmPerUnit;

    public TMP_Text speedText;

    void Start()
    {
        transform.position = start.position.ChangeZ(transform.position.z);

        float distance = end.position.x - start.position.x;

        remaningKm = totalKm;
        kmPerUnit = distance / totalKm;
    }

    void Update()
    {
        FlyToEnd();
    }

    private void FlyToEnd()
    {
        float speed = PowerManager.Ins.GetSpaceShipSpeed();

        speedText.text = $"{speed:F2} km/h";

        transform.position += kmPerUnit * speed * Vector3.right;
    }
}
