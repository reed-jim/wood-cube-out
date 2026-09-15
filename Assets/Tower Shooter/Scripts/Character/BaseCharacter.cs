using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseCharacter : MonoBehaviour
{
    public CharacterServiceLocator serviceLocator;
    public CharacterStats stats;

    public float speedToSpaceShip;

    public NodeData nodeData;

    public List<StatModifier> upgrades;
    public List<Augment> ownedAugments;

    [Header("State")]
    public int currentCycle;
    public float lastPowerGenerated;
    public bool isJustCrit;
    public bool isGuaranteedCrit;
    public NodeGroup nodeGroup;

    [Header("Energy Path")]
    public List<Vector2> energyPoints;

    public static event Action<BaseCharacter> cycleCompleteEvent;

    void Awake()
    {
        upgrades = new List<StatModifier>();
    }

    public void Init(NodeData nodeData, NodeGroup nodeGroup)
    {
        this.nodeData = nodeData;

        Init(nodeGroup);
    }

    public void Init(NodeGroup nodeGroup = null)
    {
        if (nodeData.isUnlocked)
        {
            serviceLocator.nodeUI.Unlock();

            StartCoroutine(Generating());
        }
        else
        {
            serviceLocator.nodeUI.Lock();
        }

        serviceLocator.nodeUI.InitEnergyPath();

        if (nodeGroup != null)
        {
            this.nodeGroup = nodeGroup;
        }

        // load augments
        for (int i = 0; i < AugmentManager.Ins.augmentContainer.augments.Count; i++)
        {
            Augment augment = AugmentManager.Ins.augmentContainer.augments[i];

            if (nodeData.activeAugmentsIds.Contains(augment.id))
            {
                ownedAugments.Add(augment);
            }
        }
    }

    public void Reset()
    {

    }

    public void Unlock()
    {
        nodeData.isUnlocked = true;

        Init();
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
            float power = stats.GetStat(StatType.Damage).Value;
            float speed = stats.GetStat(StatType.AttackSpeed).Value;
            float efficiency = stats.GetStat(StatType.Efficiency).Value;

            speedToSpaceShip = efficiency * (power * speed);

            float interval = 1f / speed;

            powerText.text = $"{power:F2}";

            Tween.Custom(0, 1, duration: interval, ease: Ease.Linear, onValueChange: newVal =>
            {
                circleProgress.fillAmount = newVal;
            })
            .OnComplete(() =>
            {
                power = stats.GetStat(StatType.Damage).Value;
                float criticalChance = stats.GetStat(StatType.CritChance).Value;
                float criticalMultiplier = stats.GetStat(StatType.CritMultiplier).Value;
                float chanceForMorePowerOnCircleCompletion = stats.GetStat(StatType.ChanceForMorePowerOnCycleCompletion).Value;
                float powerOnCircleCompletionMultiplier = stats.GetStat(StatType.PowerOnCircleCompletionMultiplier).Value;
                float chanceToRepeatLastCyclePower = stats.GetStat(StatType.ChanceToRepeatLastCyclePower).Value;
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

                // chance for repeat last cycle
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

                ApplyAugmentEffectsOnCycleCompletion();
                nodeGroup.ApplyEffectWhenAnyNodeInGroupCompletesCycle(this);

                currentCycle++;

                #region Energy Cube
                EnergyCube energyCube = ObjectPoolingEverything.GetFromPool<EnergyCube>(GameConstants.energyCube);

                energyCube.transform.SetParent(transform);
                energyCube.transform.localPosition = Vector3.zero;
                energyCube.Move(energyPoints);
                #endregion

                cycleCompleteEvent?.Invoke(this);
            });

            yield return new WaitForSeconds(interval);
        }
    }

    public void GenerateInstantPower(float power, float delay = 0.3f)
    {
        Tween.Delay(delay).OnComplete(() =>
        {
            RectTransform container = serviceLocator.nodeUI.container;

            PowerManager.Ins.AddPower(power);

            ShowGeneratedPowerText(power, isCritical: false, container.position, container.parent);
        });
    }

    private void ShowGeneratedPowerText(float power, bool isCritical, Vector3 position, Transform parent)
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
}
