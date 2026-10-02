using System.Collections.Generic;
using UnityEngine;

public class PrestigeUI : MonoBehaviour
{
    [SerializeField] private RectTransform buffContainer;

    private List<StatItemUI> statItemUIs = new List<StatItemUI>();

    private void Init()
    {
        List<StatModifier> permanentStatModifiers = PrestigeManager.Ins.GenerateRandomPermanentBuffs();

        CommonUtil.ReturnListToPool(statItemUIs, item => item.gameObject);

        for (int i = 0; i < permanentStatModifiers.Count; i++)
        {
            StatModifier statModifier = permanentStatModifiers[i];
            StatItemUI statItemUI = ObjectPoolingEverything.GetFromPool<StatItemUI>(GameConstants.statItemUI);

            CommonUtil.SetParentAndResetScale(statItemUI.transform, buffContainer);

            statItemUI.Init(statModifier);

            statItemUIs.Add(statItemUI);
        }
    }
}
