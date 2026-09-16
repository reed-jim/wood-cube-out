using System;
using System.Collections;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using static GameEnum;

public class SpaceShip : MonoBehaviour
{
    public Transform start;
    public Transform end;

    [Header("Parallax Background")]
    public Transform locationContainer;
    public Transform[] parallaxBackgrounds;

    public float[] parallaxBackgroundSpeedMultipliers;

    [Header("State")]
    public bool isFlying;

    [Header("Voyage")]
    public double totalKm;
    public double remaningKm;
    public double kmPerUnit;
    public float progress;

    public TMP_Text speedText;
    public TMP_Text remaningKmText;

    public VoyageData voyageData;

    public static event Action<VoyageData> reachStarEvent;

    void Awake()
    {
        ReachStarPopup.goNextStarEvent += StartNewVoyage;
    }

    void OnDestroy()
    {
        ReachStarPopup.goNextStarEvent -= StartNewVoyage;
    }

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
        if (!isFlying)
        {
            return;
        }

        if (voyageData.isReachedLastStar)
        {
            remaningKmText.text = $"You're {voyageData.lightyearFromEarth} ly away from The Earth";

            return;
        }

        if (remaningKm <= 0)
        {
            remaningKmText.text = $"You're at {StarManager.Ins.GetVoyageConfig(voyageData.starPathConfigIndex).starName}";

            reachStarEvent?.Invoke(voyageData);

            isFlying = false;

            return;
        }

        float speed = PowerManager.Ins.GetSpaceShipSpeed();

        speedText.text = TextUtil.FormatSpeed(speed);
        remaningKmText.text = TextUtil.FormatDistance(remaningKm);

        // Vector3 deltaPosition = kmPerUnit * speed * Vector3.right;

        // locationContainer.position -= deltaPosition;

        // for (int i = 0; i < parallaxBackgrounds.Length; i++)
        // {
        //     parallaxBackgrounds[i].transform.position -= parallaxBackgroundSpeedMultipliers[i] * deltaPosition;
        // }

        remaningKm -= speed;
        progress = (float)(1 - remaningKm / totalKm);

        VoyageMilestoneUI.Ins.SetVoyageProgress(progress);
    }

    public void StartNewVoyage()
    {
        voyageData.lightyearFromEarth += totalKm / GameConstants.KmPerLightYear;

        if (StarManager.Ins.IsLastVoyage(voyageData.starPathConfigIndex))
        {
            voyageData.isReachedLastStar = true;

            SaveCurrentVoyage();
        }
        else
        {
            voyageData.starPathConfigIndex++;

            LoadCurrentVoyage();
        }

        isFlying = true;
    }

    public void LoadCurrentVoyage()
    {
        voyageData = DataUtility.Load($"{SaveSlotManager.Ins.currentSaveSlotData}_voyageData", StarManager.Ins.GetFirstVoyage());

        float distance = end.position.x - start.position.x;

        totalKm = GameConstants.KmPerLightYear * StarManager.Ins.GetVoyageConfig(voyageData.starPathConfigIndex).distanceInLightyear;
        kmPerUnit = distance / totalKm;
        remaningKm = voyageData.remaningKm;

        // locationContainer.position.ChangeX(Mathf.Lerp(start.position.x, end.position.x, 1 - remaningKm / totalKm));

        VoyageMilestoneUI.Ins.InitVoyageMilestones();

        isFlying = true;
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
    public double remaningKm;
    public bool isReachedLastStar;
    public double lightyearFromEarth;
}
