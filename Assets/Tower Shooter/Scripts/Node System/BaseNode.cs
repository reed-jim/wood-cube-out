using System.Collections;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseNode : MonoBehaviour
{
    public int power;
    public int speed;

    [Header("UI")]
    public RectTransform container;
    public Image circleProgress;
    public TMP_Text powerText;

    void Start()
    {
        StartCoroutine(Generating());
    }

    private IEnumerator Generating()
    {
        while (true)
        {
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

                // damageText.ShowDamage(power, container.position);
            });

            yield return new WaitForSeconds(interval);
        }
    }
}
