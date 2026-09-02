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

    public List<StatModifier> upgrades;
    public List<Augment> ownedAugments;

    void Awake()
    {
        upgrades = new List<StatModifier>();
    }

    void Start()
    {
        StartCoroutine(Generating());
    }

    private IEnumerator Generating()
    {
        RectTransform container = serviceLocator.nodeUI.container;
        TMP_Text powerText = serviceLocator.nodeUI.powerText;
        Image circleProgress = serviceLocator.nodeUI.circleProgress;

        while (true)
        {
            int power = (int)stats.GetStat(StatType.Damage).Value;
            float speed = stats.GetStat(StatType.AttackSpeed).Value;

            float interval = 1f / speed;

            powerText.text = $"{power}";

            Tween.Custom(0, 1, duration: interval, ease: Ease.Linear, onValueChange: newVal =>
            {
                circleProgress.fillAmount = newVal;
            })
            .OnComplete(() =>
            {
                PowerManager.Ins.AddPower(power);

                DamageText damageText = ObjectPoolingEverything.GetFromPool<DamageText>(GameConstants.damageText);

                damageText.transform.SetParent(container);

                damageText.ShowDamage(power, container.position);
            });

            yield return new WaitForSeconds(interval);
        }
    }
}
