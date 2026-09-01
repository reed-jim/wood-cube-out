using System.Collections.Generic;
using UnityEngine;

public class StackingUI : MonoBehaviour
{
    public RectTransform container;

    public Dictionary<DamageOverTimeType, StackingItemUI> items;

    void Awake()
    {
        items = new Dictionary<DamageOverTimeType, StackingItemUI>();
    }

    public void UpdateUI(DamageOverTimeItem damageOverTimeItem)
    {
        if (items.ContainsKey(damageOverTimeItem.DamageOverTimeType))
        {
            items[damageOverTimeItem.DamageOverTimeType].UpdateUI();
        }
        else
        {
            StackingItemUI stackingItemUI = ObjectPoolingEverything.GetFromPool<StackingItemUI>(GameConstants.stackingItemUI);

            stackingItemUI.transform.SetParent(container);

            stackingItemUI.Init(damageOverTimeItem);

            items.Add(damageOverTimeItem.DamageOverTimeType, stackingItemUI);
        }
    }
}
