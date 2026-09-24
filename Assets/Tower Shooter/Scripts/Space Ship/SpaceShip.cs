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

    private const double LightSpeedKmPerSecond = 299_792.458;
    private double InversedLightSpeedKmPerSecond;
    private const float FramePerSecond = 60;
    private int updateSpeedTick;

    public static event Action<VoyageData> reachStarEvent;

    void Awake()
    {
        ReachStarPopup.goNextStarEvent += StartNewVoyage;

        InversedLightSpeedKmPerSecond = 1 / LightSpeedKmPerSecond;
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
            StarPathConfig starPathConfig = StarManager.Ins.GetVoyageConfig(voyageData.starPathConfigIndex);

            remaningKmText.text = $"You're at {starPathConfig.starName}";

            reachStarEvent?.Invoke(voyageData);

            isFlying = false;

            StarDiscoveryEventDataItem starDiscoveryEventDataItem = IngameEventManager.Ins.CreateStarDiscoveryEvent(starPathConfig);
            LeadershipManager.Ins.AddStarDiscoveryEventLeadershipPeriod(starDiscoveryEventDataItem);

            return;
        }

        double kmPerFrame = PowerManager.Ins.GetSpaceShipSpeed();

        if (updateSpeedTick % FramePerSecond == 0)
        {
            // to make speed look more active
            float randomValue = UnityEngine.Random.RandomRange(0, 1);

            // speedText.text = TextUtil.FormatSpeed(kmPerFrame * FramePerSecond + randomValue, InversedLightSpeedKmPerSecond);
            speedText.text =
                $"{TextUtil.FormatSpeed(kmPerFrame * FramePerSecond + randomValue, InversedLightSpeedKmPerSecond)} ({TextUtil.GetDurationString(kmPerFrame, remaningKm)})";

            updateSpeedTick = 0;
        }
        else
        {
            updateSpeedTick++;
        }

        remaningKmText.text = TextUtil.FormatDistance(remaningKm);

        // Vector3 deltaPosition = kmPerUnit * speedPerFrame * Vector3.right;

        // locationContainer.position -= deltaPosition;

        // for (int i = 0; i < parallaxBackgrounds.Length; i++)
        // {
        //     parallaxBackgrounds[i].transform.position -= parallaxBackgroundSpeedMultipliers[i] * deltaPosition;
        // }

        remaningKm -= kmPerFrame;
        progress = (float)(1 - remaningKm / totalKm);

        VoyageMilestoneUI.Ins.SetVoyageProgress(progress);
    }

    public void StartNewVoyage()
    {
        voyageData.lightyearFromEarth += totalKm / GameConstants.KmPerLightYear;

        if (StarManager.Ins.IsLastVoyage(voyageData.starPathConfigIndex))
        {
            voyageData.isReachedLastStar = true;
        }
        else
        {
            voyageData.starPathConfigIndex++;
            voyageData.remaningKm = GameConstants.KmPerLightYear * StarManager.Ins.GetVoyageConfig(voyageData.starPathConfigIndex).distanceInLightyear;

            Init();
        }

        SaveCurrentVoyage();

        isFlying = true;
    }

    public void LoadCurrentVoyage()
    {
        voyageData = DataUtility.Load($"{SaveSlotManager.Ins.currentSaveSlotData}_voyageData", StarManager.Ins.GetFirstVoyage());

        Init();
    }

    private void Init()
    {
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
