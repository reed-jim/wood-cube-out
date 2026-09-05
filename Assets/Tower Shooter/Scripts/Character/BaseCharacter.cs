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

    void Awake()
    {
        upgrades = new List<StatModifier>();
    }

    public void Init(NodeData nodeData)
    {
        this.nodeData = nodeData;

        Init();
    }

    public void Init()
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
    }

    public void Reset()
    {

    }

    private IEnumerator Generating()
    {
        RectTransform container = serviceLocator.nodeUI.container;
        TMP_Text powerText = serviceLocator.nodeUI.powerText;
        Image circleProgress = serviceLocator.nodeUI.circleProgress;

        if (!NodeManager.Ins.nodes.Contains(this))
        {
            NodeManager.Ins.nodes.Add(this);
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
                float chanceForMorePowerOnCircleCompletion = stats.GetStat(StatType.ChanceForMorePowerOnCircleCompletion).Value;
                float powerOnCircleCompletionMultiplier = stats.GetStat(StatType.PowerOnCircleCompletionMultiplier).Value;
                bool isCritical = false;

                if (Random.value < chanceForMorePowerOnCircleCompletion)
                {
                    power *= powerOnCircleCompletionMultiplier;

                    isCritical = true;
                }

                PowerManager.Ins.AddPower(power);

                DamageText damageText = ObjectPoolingEverything.GetFromPool<DamageText>(GameConstants.damageText);

                damageText.transform.SetParent(container.parent);

                damageText.ShowDamage(power, container.position, isCritical, target: transform);
            });

            yield return new WaitForSeconds(interval);
        }
    }
}
