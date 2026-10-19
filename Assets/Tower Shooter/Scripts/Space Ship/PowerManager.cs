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

    private Vector3 originalScale;
    private float shakeIntensity = 0f;
    private float shakeTimer = 0f;
    private double previousTotalValue = -1;

    // private float totalPowerOutput;
    private float powerOutputPerSecond;
    private string primaryHex;
    private string onPrimaryHex;
    private Tween pulseTween;

    private float updateTimer = 0f;
    private const float updateInterval = 1f / 24; // Update once per second

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
    }

    void Update()
    {
        updateTimer += Time.deltaTime;

        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;

            double deltaChange = powerOutputPerSecond * updateInterval;
            double value = ResourceManager.Ins.ChangeResource(ResourceType.Coin, deltaChange);

            totalPowerText.text = $"<sprite=0 color={onPrimaryHex}>{CommonUtil.FormatNumber(value)}";
            powerOutputPerSecondText.text = $"+{CommonUtil.FormatNumber(powerOutputPerSecond)}<sprite=0 color={primaryHex}>/s";

            // --- Percentage-Based Shake Logic ---
            if (previousTotalValue > 0)
            {
                // Calculate percent change (e.g., 10 / 10000 = 0.001, which is 0.1%)
                double percentChange = deltaChange / previousTotalValue;

                // Minimum percent threshold to trigger shake (e.g., 0.005 = 0.5%)
                double minPercentThreshold = 0.005;

                if (percentChange >= minPercentThreshold)
                {
                    // Scale intensity based on the percentage change (capped so it doesn't get out of hand)
                    shakeIntensity = Mathf.Min((float)percentChange * 2f, 0.3f);
                    shakeTimer = 0.15f; // Duration of the shake effect
                }
            }

            previousTotalValue = value;
        }

        // Process the shake effect every frame if active
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;

            float scaleOffset = UnityEngine.Random.Range(-shakeIntensity, shakeIntensity);
            totalPowerText.transform.localScale = originalScale + Vector3.one * scaleOffset;
        }
        else
        {
            totalPowerText.transform.localScale = originalScale;
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

    public float GetTotalPowerOutputPerSecond()
    {
        List<BaseCharacter> nodes = NodeManager.Ins.nodes;

        powerOutputPerSecond = 0;

        for (int i = 0; i < nodes.Count; i++)
        {
            BaseCharacter node = nodes[i];

            if (!node.nodeData.isUnlocked)
            {
                continue;
            }

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

        return powerOutputPerSecond;
    }

    public void ShowTotalPowerOutputPerSecond()
    {
        string notificationString =
            $"Power Output: <color={primaryHex}>{TextUtil.FormatFloat(GetTotalPowerOutputPerSecond())} <sprite=0 color={primaryHex}>/s";

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
}
