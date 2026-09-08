using System;
using System.Collections;
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

    public VoyageData voyageData;

    void Start()
    {
        LoadCurrentVoyage();

        StartCoroutine(PeriodicallySaveVoyageData());
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

        speedText.text = TextUtil.FormatSpeed(speed);
        remaningKmText.text = TextUtil.FormatDistance(remaningKm);

        Vector3 deltaPosition = kmPerUnit * speed * Vector3.right;

        locationContainer.position -= deltaPosition;

        for (int i = 0; i < parallaxBackgrounds.Length; i++)
        {
            parallaxBackgrounds[i].transform.position -= parallaxBackgroundSpeedMultipliers[i] * deltaPosition;
        }

        remaningKm -= speed;
    }

    public void LoadCurrentVoyage()
    {
        voyageData = DataUtility.Load($"{SaveSlotManager.Ins.currentSaveSlotData}_voyageData", StarManager.Ins.GetFirstVoyage());

        float distance = end.position.x - start.position.x;

        totalKm = StarManager.Ins.GetVoyageConfig(voyageData.starPathConfigIndex).distance;
        kmPerUnit = distance / totalKm;
        remaningKm = voyageData.remaningKm;

        locationContainer.position.ChangeX(Mathf.Lerp(start.position.x, end.position.x, 1 - remaningKm / totalKm));
    }

    public void SaveCurrentVoyage()
    {
        voyageData.remaningKm = remaningKm;

        DataUtility.Save($"{SaveSlotManager.Ins.currentSaveSlotData}_voyageData", voyageData);
    }

    public IEnumerator PeriodicallySaveVoyageData()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(3);

        while (true)
        {
            yield return waitForSeconds;

            SaveCurrentVoyage();
        }
    }
}

[Serializable]
public class VoyageData
{
    public int starPathConfigIndex;
    public float remaningKm;
}
