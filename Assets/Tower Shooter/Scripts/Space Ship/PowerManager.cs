using System;
using System.Collections.Generic;
using PrimeTween;
using TMPro;
using UnityEngine;

public class PowerManager : Singleton<PowerManager>
{
    [Header("Customize")]
    public float spaceShipSpeedMultiplier;

    [Header("UI")]
    public TMP_Text totalPowerText;
    public TMP_Text powerOutputPerSecondText;

    [Header("Text Anim")]
    [SerializeField] private float targetScale = 1.15f;
    [SerializeField] private float duration = 0.8f;

    [Header("Text Juice / Shake")]
    [SerializeField] private float maxShakeOffset = 0.12f;  // Maximum scale distortion cap
    [SerializeField] private float shakeDecayRate = 12f;    // How fast the shake dies down (higher = snappier)
    [SerializeField] private float shakeFrequency = 35f;    // How fast/tight the vibration oscillation is

    private float currentShakeIntensity = 0f;

    [Header("Text Juice / Shake")]
    [SerializeField] private float basePunchAmount = 0.1f;
    [SerializeField] private float maxPunchAmount = 0.35f;

    private Tween textTween;

    private Vector3 originalScale;
    private float shakeIntensity = 0f;
    private float shakeTimer = 0f;
    private double previousTotalValue = -1;

    private double powerOutputPerSecond;
    private string primaryHex;
    private string onPrimaryHex;
    private string primaryContainerHex;

    private float updateTimer = 0f;
    private const float updateInterval = 1f / 3; // Update once per second

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        NodeManager.nodeManagerInitEvent += OnNodeManagerInit;
    }

    void OnDestroy()
    {
        NodeManager.nodeManagerInitEvent -= OnNodeManagerInit;
    }

    void Start()
    {
        // Cache the original scale of the text so we can return to it
        originalScale = totalPowerText.transform.localScale;

        primaryHex = DynamicThemeManager.Ins.PrimaryHex;
        onPrimaryHex = DynamicThemeManager.Ins.OnPrimaryHex;
        primaryContainerHex = DynamicThemeManager.Ins.PrimaryContainerHex;
    }

    void Update()
    {
        updateTimer += Time.deltaTime;

        // Cache the previous value before we check for changes
        double oldDisplayValue = previousTotalValue;

        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;

            double deltaChange = powerOutputPerSecond * updateInterval;
            double value = ResourceManager.Ins.ChangeResource(ResourceType.Coin, deltaChange);

            string powerOutputPerSecondString = $"+{CommonUtil.FormatNumber(powerOutputPerSecond)}/s";
            totalPowerText.text = $"<sprite=0 color={onPrimaryHex}>{CommonUtil.FormatNumber(value)} <color={primaryContainerHex}>({powerOutputPerSecondString})";

            previousTotalValue = value;
        }

        // --- Immediate Scale Feedback on Value Change ---
        // If you want it to trigger the exact frame the resource/value changes:
        if (previousTotalValue > 0 && previousTotalValue != oldDisplayValue)
        {
            double absoluteChange = previousTotalValue - oldDisplayValue;
            double percentChange = absoluteChange / previousTotalValue;

            // Calculate strength based on delta magnitude
            float punchStrength = Mathf.Clamp((float)(percentChange * 30f), 0.0f, 0.15f);

            // Stop any active tween immediately
            textTween.Stop();

            // 1. Instantly pop to the expanded scale the exact frame it changes
            totalPowerText.transform.localScale = originalScale + Vector3.one * punchStrength;

            // 2. Tween back down smoothly to the original scale
            textTween = Tween.Scale(
                target: totalPowerText.transform,
                endValue: originalScale,
                duration: updateInterval
            );
        }
    }

    public void AddPower(float power)
    {
        // ResourceManager.Ins.ChangeResource(ResourceType.Coin, power);
    }

    public void UpdateUI()
    {
        totalPowerText.text = $"{ResourceManager.Ins.GetResourceValue(ResourceType.Coin):F2}";
    }

    public double GetTotalPowerOutputPerSecond()
    {
        List<BaseCharacter> nodes = NodeManager.Ins.nodes;

        powerOutputPerSecond = 0;

        BaseCharacter mainNode = null;

        for (int i = 0; i < nodes.Count; i++)
        {
            BaseCharacter node = nodes[i];

            if (!node.nodeData.isUnlocked)
            {
                continue;
            }

            mainNode = node;

            CharacterStats stats = nodes[i].serviceLocator.stats;

            float power = stats.GetStat(StatType.Damage).Value;
            float cycleSpeed = stats.GetStat(StatType.AttackSpeed).Value;
            float critChance = stats.GetStat(StatType.CritChance).Value;
            float CritMultiplier = stats.GetStat(StatType.CritMultiplier).Value;

            // Calculate average DPS for this node
            float averageDamagePerHit = power * (1f + (critChance * (CritMultiplier - 1f)));
            float nodeDps = averageDamagePerHit * cycleSpeed * GameConstants.nodeSpeedMultiplier;

            powerOutputPerSecond += nodeDps;
        }

        double nodePower = mainNode.serviceLocator.stats.GetStat(StatType.Damage).Value;

        double powerMultiplier = GetPowerMultiplierBasedOnTier(mainNode, GetPowerTier(nodePower));

        powerOutputPerSecond *= powerMultiplier;

        float powerProgressForShader = GetPowerProgressForShader(nodePower);

        mainNode.serviceLocator.nodeEffectController.SetPower(powerProgressForShader);
        NodeEnergyParticleController.Ins.SetPower(powerProgressForShader);
        NodeEnergyParticleController.Ins.SetMultiplier(powerMultiplier);

        return powerOutputPerSecond;
    }

    public void ShowTotalPowerOutputPerSecond()
    {
        string notificationString =
            $"Power Output: <color={primaryHex}>{CommonUtil.FormatNumber(GetTotalPowerOutputPerSecond())} <sprite=0 color={primaryHex}>/s";

        IngameNotificationManager.Ins.ShowNotification(notificationString, isPlayEffect: true);
    }

    public float GetSpaceShipSpeed()
    {
        float totalSpeed = 0;

        for (int i = 0; i < NodeManager.Ins.nodes.Count; i++)
        {
            BaseCharacter node = NodeManager.Ins.nodes[i];

            totalSpeed += node.speedToSpaceShip;
        }

        return spaceShipSpeedMultiplier * totalSpeed;
    }

    private void OnNodeManagerInit()
    {
        GetTotalPowerOutputPerSecond();
    }

    #region Util
    public static int GetPowerTier(double power)
    {
        if (double.IsNaN(power) || power < 10)
            return 0;

        if (double.IsPositiveInfinity(power))
            return 10;

        int tier = (int)(Math.Log10(power) / 2);

        return Math.Clamp(tier, 1, 10);
    }

    public static int GetPowerProgressForShader(double power)
    {
        return 10 * GetPowerTier(power);
    }

    public double GetPowerMultiplierBasedOnTier(BaseCharacter node, int tier)
    {
        return Math.Pow(node.nodeData.powerTierGrowthFactor, tier);
    }
    #endregion
}
