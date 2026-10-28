using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseCharacter : MonoBehaviour
{
    [Header("Identifier")]
    public int id = -1;

    public CharacterServiceLocator serviceLocator;
    public CharacterStats stats;

    public double speedToSpaceShip;

    public NodeData nodeData;

    public List<StatModifier> upgrades;
    public List<Augment> ownedAugments;

    [Header("State")]
    public bool isPaused;
    public bool isIdle;
    public int currentCycle;
    public double lastPowerGenerated;
    public bool isJustCrit;
    public bool isGuaranteedCrit;
    public NodeGroup nodeGroup;

    private Transform initialParent;
    private int siblingIndex;

    [Header("Effect")]
    public NodeEffectController nodeEffectController;

    [Header("Energy Path")]
    public List<Vector2> energyPoints;

    [Header("High-Speed Performance Settings")]
    [Tooltip("Cycles per second threshold before switching from discrete tweening to delta-time lump generation.")]
    public float highSpeedThreshold = 20f;

    [Tooltip("Maximum rate (in seconds) at which floating damage text UI spawns during super high-speed generation.")]
    public float damageTextThrottleInterval = 0.15f;

    // Shader property IDs
    private Material instanceMaterial;
    private static readonly int FillAmountID = Shader.PropertyToID("_FillAmount");
    private static readonly int HighSpeedID = Shader.PropertyToID("_HighSpeed");
    private static readonly int RotationID = Shader.PropertyToID("_Rotation");

    // Throttling and accumulation trackers for high speed
    private float highSpeedTimer;
    private double highSpeedAccumulatedPower;
    private bool highSpeedHasCrit;

    public static event Action<BaseCharacter> cycleCompleteEvent;
    public static event Action<BaseCharacter> selectNodeEvent;
    public static event Action<BaseCharacter> selectNodeToApplyAugmentEvent;

    void Awake()
    {
        upgrades = new List<StatModifier>();
    }

    public void Init(NodeData nodeData, NodeGroup nodeGroup)
    {
        this.nodeData = nodeData;

        Init(nodeGroup);

        initialParent = transform.parent;
        siblingIndex = transform.GetSiblingIndex();
    }

    public void Init(NodeGroup nodeGroup = null)
    {
        if (nodeGroup != null)
        {
            this.nodeGroup = nodeGroup;
        }

        // Load Augments
        if (nodeData.activeAugmentsIds != null)
        {
            for (int i = 0; i < AugmentManager.Ins.augmentContainer.augments.Count; i++)
            {
                Augment augment = AugmentManager.Ins.augmentContainer.augments[i];

                if (nodeData.activeAugmentsIds.Contains(augment.id))
                {
                    ownedAugments.Add(augment);

                    AugmentManager.Ins.AddAugment(augment);

                    augment.Apply(serviceLocator.stats);
                }
            }
        }

        if (id == -1)
        {
            NodeManager.Ins.AllocateIdForNode(this);

            serviceLocator.stats.LoadStats();

            // Load Upgrades
            upgrades = serviceLocator.stats.GetAllModifiersFromSource(StatModifierSource.Upgrade);

            // serviceLocator.nodeUI.InitEnergyPath();
        }

        // Material setup for UI shader parameters
        if (serviceLocator != null && serviceLocator.nodeUI != null && serviceLocator.nodeUI.circleProgress != null)
        {
            Image circleProgress = serviceLocator.nodeUI.circleProgress;
            if (circleProgress.material != null)
            {
                // Create material instance to avoid sharing material parameters across all nodes
                instanceMaterial = new Material(circleProgress.material);
                circleProgress.material = instanceMaterial;
            }
        }

        if (nodeData.isUnlocked)
        {
            serviceLocator.nodeUI.Unlock();

            StartCoroutine(Generating());
        }
        else
        {
            serviceLocator.nodeUI.Lock();
        }
    }

    public void Reset()
    {

    }

    public void Unlock()
    {
        nodeData.isUnlocked = true;

        Init();

        DataManager.Ins.SaveData();
    }

    public void Pause()
    {
        isPaused = true;
    }

    public void Unpause()
    {
        isPaused = false;
    }

    private IEnumerator Generating()
    {
        RectTransform container = serviceLocator.nodeUI.container;
        TMP_Text powerText = serviceLocator.nodeUI.powerText;
        Image circleProgress = serviceLocator.nodeUI.circleProgress;

        if (!NodeManager.Ins.nodes.Contains(this))
        {
            NodeManager.Ins.nodes.Add(this);

            if (UpgradeNodeUI.Ins.selectedNode == null)
            {
                UpgradeNodeUI.Ins.SelectNode(this);
            }
        }

        while (true)
        {
            if (!isPaused)
            {
                double basePower = stats.GetStat(StatType.Damage).Value;
                double speed = stats.GetStat(StatType.AttackSpeed).Value * GameConstants.nodeSpeedMultiplier;
                double efficiency = stats.GetStat(StatType.Efficiency).Value;

                // speedToSpaceShip = efficiency * (basePower * speed);
                speedToSpaceShip = basePower * speed;
                powerText.text = $"{basePower:F2}";

                // --- HIGH SPEED MODE (Delta-time Accumulation) ---
                if (speed >= highSpeedThreshold)
                {
                    UpdateShaderProperties(1.0f, 1.0f, (float)(Time.time * speed * 0.5f));

                    double deltaCycles = speed * Time.deltaTime;
                    double criticalChance = stats.GetStat(StatType.CritChance).Value;
                    double criticalMultiplier = stats.GetStat(StatType.CritMultiplier).Value;
                    double chanceForMorePower = stats.GetStat(StatType.ChanceForMorePowerOnCycleCompletion).Value;
                    double powerMultiplier = stats.GetStat(StatType.PowerOnCircleCompletionMultiplier).Value;
                    double chanceToRepeat = stats.GetStat(StatType.ChanceToRepeatLastCyclePower).Value;

                    // Calculate average power multiplier per cycle
                    double avgCritMultiplier = 1f + (criticalChance * (criticalMultiplier - 1f));
                    double avgMorePowerMultiplier = 1f + (chanceForMorePower * (powerMultiplier - 1f));
                    double avgRepeatMultiplier = 1f + chanceToRepeat;

                    double powerPerCycle = basePower * avgCritMultiplier * avgMorePowerMultiplier * avgRepeatMultiplier;
                    double framePower = powerPerCycle * deltaCycles;

                    PowerManager.Ins.AddPower(framePower);

                    highSpeedAccumulatedPower += framePower;
                    highSpeedTimer += Time.deltaTime;

                    if (criticalChance > 0f) highSpeedHasCrit = true;

                    // Throttled UI Text Creation
                    if (highSpeedTimer >= damageTextThrottleInterval)
                    {
                        ShowGeneratedPowerText(highSpeedAccumulatedPower, highSpeedHasCrit, container.position, container.parent);
                        highSpeedAccumulatedPower = 0f;
                        highSpeedTimer = 0f;
                        highSpeedHasCrit = false;
                    }

                    int completedCyclesThisFrame = Mathf.FloorToInt((float)deltaCycles);
                    for (int i = 0; i < Mathf.Min(completedCyclesThisFrame, 5); i++)
                    {
                        ApplyAugmentEffectsOnCycleCompletion();
                        nodeGroup?.ApplyEffectWhenAnyNodeInGroupCompletesCycle(this);
                        currentCycle++;
                        cycleCompleteEvent?.Invoke(this);
                    }

                    yield return null;
                }
                // --- NORMAL MODE (Discrete Cycles & Tweens) ---
                else
                {
                    float interval = (float)(1f / speed);
                    float highSpeedVisualFactor = (float)(speed / highSpeedThreshold);

                    Tween progressTween = Tween.Custom(0f, 1f, duration: interval, ease: Ease.Linear, onValueChange: newVal =>
                    {
                        circleProgress.fillAmount = newVal;

                        serviceLocator.nodeEffectController.SetPower(newVal * 100);

                        UpdateShaderProperties(newVal, highSpeedVisualFactor, (float)(Time.time * speed));
                    });

                    yield return progressTween.ToYieldInstruction();

                    basePower = stats.GetStat(StatType.Damage).Value;

                    double power = basePower;
                    double criticalChance = stats.GetStat(StatType.CritChance).Value;
                    double criticalMultiplier = stats.GetStat(StatType.CritMultiplier).Value;
                    double chanceForMorePowerOnCircleCompletion = stats.GetStat(StatType.ChanceForMorePowerOnCycleCompletion).Value;
                    double powerOnCircleCompletionMultiplier = stats.GetStat(StatType.PowerOnCircleCompletionMultiplier).Value;
                    double chanceToRepeatLastCyclePower = stats.GetStat(StatType.ChanceToRepeatLastCyclePower).Value;
                    bool isCritical = false;

                    // Crit
                    if (UnityEngine.Random.value < criticalChance || isGuaranteedCrit)
                    {
                        power *= criticalMultiplier;
                        isCritical = true;
                        isGuaranteedCrit = false;
                    }

                    if (UnityEngine.Random.value < chanceForMorePowerOnCircleCompletion)
                    {
                        power *= powerOnCircleCompletionMultiplier;
                        isCritical = true;
                    }

                    PowerManager.Ins.AddPower(power);
                    ShowGeneratedPowerText(power, isCritical, container.position, container.parent);

                    // Chance to repeat last cycle
                    if (UnityEngine.Random.value < chanceToRepeatLastCyclePower)
                    {
                        TweenManager.Ins.AddTween(Tween.Delay(0.3f).OnComplete(() =>
                        {
                            PowerManager.Ins.AddPower(power);
                            ShowGeneratedPowerText(power, isCritical, container.position, container.parent);
                        }));
                    }

                    lastPowerGenerated = power;
                    isJustCrit = isCritical;

                    serviceLocator.recycleModule.Recycle(lastPowerGenerated);

                    ApplyAugmentEffectsOnCycleCompletion();
                    nodeGroup?.ApplyEffectWhenAnyNodeInGroupCompletesCycle(this);

                    currentCycle++;

                    SoundManager.Instance.PlayCycleComplete();

                    cycleCompleteEvent?.Invoke(this);
                }
            }
            else
            {
                yield return new WaitForSeconds(1f / 30);
            }
        }
    }

    private void UpdateShaderProperties(float fillAmount, float highSpeedAmount, float rotation)
    {
        return;

        if (instanceMaterial == null) return;

        instanceMaterial.SetFloat(FillAmountID, fillAmount);
        instanceMaterial.SetFloat(HighSpeedID, highSpeedAmount);
        instanceMaterial.SetFloat(RotationID, rotation);
    }

    public void GenerateInstantPower(double power, float delay = 0.3f)
    {
        Tween.Delay(delay).OnComplete(() =>
        {
            RectTransform container = serviceLocator.nodeUI.container;

            PowerManager.Ins.AddPower(power);

            ShowGeneratedPowerText(power, isCritical: false, container.position, container.parent);
        });
    }

    private void ShowGeneratedPowerText(double power, bool isCritical, Vector3 position, Transform parent)
    {
        DamageText damageText = ObjectPoolingEverything.GetFromPool<DamageText>(GameConstants.damageText);

        damageText.transform.SetParent(parent);

        damageText.ShowDamage(power, position, isCritical, target: transform);
    }

    private void ApplyAugmentEffectsOnCycleCompletion()
    {
        for (int i = 0; i < ownedAugments.Count; i++)
        {
            Augment augment = ownedAugments[i];

            for (int j = 0; j < augment.dynamicEffects.Count; j++)
            {
                AugmentEffect augmentEffect = augment.dynamicEffects[j];

                augmentEffect.ApplyEffectOnCycleCompleted(serviceLocator.stats);
            }
        }
    }

    public void InvokeSelectNodeEvent()
    {
        selectNodeEvent?.Invoke(this);
    }

    public void InvokeSelectNodeToApplyAugmentEvent()
    {
        selectNodeToApplyAugmentEvent?.Invoke(this);
    }

    #region Util 
    public void SetToInitialParent()
    {
        transform.SetParent(initialParent);
        transform.SetSiblingIndex(siblingIndex);
    }

    public double GetTotalPowerOutputPerSecond()
    {
        CharacterStats stats = serviceLocator.stats;

        double power = stats.GetStat(StatType.Damage).Value;
        double cycleSpeed = stats.GetStat(StatType.AttackSpeed).Value;
        double critChance = stats.GetStat(StatType.CritChance).Value;
        double CritMultiplier = stats.GetStat(StatType.CritMultiplier).Value;

        // Calculate average DPS for this node
        double averageDamagePerHit = power * (1f + (critChance * (CritMultiplier - 1f)));
        double nodeDps = averageDamagePerHit * cycleSpeed * GameConstants.nodeSpeedMultiplier;

        return nodeDps;
    }
    #endregion
}
