using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;

public class SpaceShip : MonoBehaviour
{
    public Transform start;
    public Transform end;

    [Header("Parallax Background")]
    public Transform locationContainer;
    public Transform[] parallaxBackgrounds;

    public float[] parallaxBackgroundSpeedMultipliers;

    [Header("Voyage")]
    public float totalKm;
    public float remaningKm;
    public float kmPerUnit;

    public TMP_Text speedText;
    public TMP_Text remaningKmText;

    void Start()
    {
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
        if (remaningKm <= 0)
        {
            return;
        }

        float speed = PowerManager.Ins.GetSpaceShipSpeed();

        speedText.text = $"{speed:F2} km/h";
        remaningKmText.text = $"{(int)remaningKm} km";

        Vector3 deltaPosition = kmPerUnit * speed * Vector3.right;

        locationContainer.position -= deltaPosition;

        for (int i = 0; i < parallaxBackgrounds.Length; i++)
        {
            parallaxBackgrounds[i].transform.position -= parallaxBackgroundSpeedMultipliers[i] * deltaPosition;
        }

        remaningKm -= speed;
    }
}
