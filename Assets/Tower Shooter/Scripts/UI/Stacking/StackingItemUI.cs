using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StackingItemUI : MonoBehaviour
{
    public RectTransform container;
    public Image icon;
    public TMP_Text stackingNumber;

    public DamageOverTimeItem damageOverTimeItem;

    public void Init(DamageOverTimeItem damageOverTimeItem)
    {
        SaferioTween.ShowWithPunch(container, 1);

        this.damageOverTimeItem = damageOverTimeItem;

        UpdateUI();
    }

    public void UpdateUI()
    {
        stackingNumber.text = $"{damageOverTimeItem.currentStack}";
    }
}
