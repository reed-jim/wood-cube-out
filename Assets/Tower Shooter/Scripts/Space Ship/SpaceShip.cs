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
    public MilestoneFinishEffect milestoneFinishEffect;

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
        LoadVoyageData();

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

            milestoneFinishEffect.ReachMilestone(voyageData);

            // reachStarEvent?.Invoke(voyageData);

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
            voyageData.remaningKm = GameConstants.KmPerLightYear * CurveManager.Ins.GetVoyageDistance(voyageData.starPathConfigIndex);

            StarManager.Ins.SetStarPathConfig(voyageData.starPathConfigIndex);

            Init();
        }

        SaveVoyageData();

        isFlying = true;
    }

    private void Init()
    {
        float distance = end.position.x - start.position.x;

        totalKm = GameConstants.KmPerLightYear * CurveManager.Ins.GetVoyageDistance(voyageData.starPathConfigIndex);
        kmPerUnit = distance / totalKm;
        remaningKm = voyageData.remaningKm;

        // locationContainer.position.ChangeX(Mathf.Lerp(start.position.x, end.position.x, 1 - remaningKm / totalKm));

        VoyageMilestoneUI.Ins.InitVoyageMilestones();

        isFlying = true;
    }

    #region Save/Load
    public void LoadVoyageData()
    {
        voyageData = DataUtility.Load($"{SaveSlotManager.Ins.currentSaveSlotData}_voyageData", StarManager.Ins.GetFirstVoyage());

        StarManager.Ins.SetStarPathConfig(voyageData.starPathConfigIndex);

        Init();
    }

    public void SaveVoyageData()
    {
        voyageData.remaningKm = remaningKm;

        DataUtility.Save($"{SaveSlotManager.Ins.currentSaveSlotData}_voyageData", voyageData);
    }

    public void ResetVoyageData()
    {
        voyageData = StarManager.Ins.GetFirstVoyage();

        SaveVoyageData();
    }
    #endregion

    public IEnumerator PeriodicallySaveVoyageData()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(3);

        while (true)
        {
            yield return waitForSeconds;

            SaveVoyageData();
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
