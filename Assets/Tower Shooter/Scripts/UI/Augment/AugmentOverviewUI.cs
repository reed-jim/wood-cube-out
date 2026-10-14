using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AugmentOverviewUI : BasePopup
{
    public RectTransform augmentContainer;
    public RectTransform augmentSetContainer;

    public TMP_Text powerOutputText;

    private List<NodeDetailAugmentItemUI> nodeDetailAugmentItemUIs = new List<NodeDetailAugmentItemUI>();
    private List<AugmentSetItemUI> augmentSetItemUIs = new List<AugmentSetItemUI>();

    private Coroutine updatingPowerOutputCoroutine;

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);

        Init();
    }

    public void Init()
    {
        for (int i = 0; i < nodeDetailAugmentItemUIs.Count; i++)
        {
            ObjectPoolingEverything.ReturnToPool(GameConstants.nodeDetailAugmentItemUI, nodeDetailAugmentItemUIs[i].gameObject);
        }

        for (int i = 0; i < augmentSetItemUIs.Count; i++)
        {
            ObjectPoolingEverything.ReturnToPool(GameConstants.augmentSetItemUI, augmentSetItemUIs[i].gameObject);
        }

        nodeDetailAugmentItemUIs.Clear();
        augmentSetItemUIs.Clear();

        for (int i = 0; i < AugmentManager.Ins.activeAugments.Count; i++)
        {
            Augment augment = AugmentManager.Ins.activeAugments[i];

            NodeDetailAugmentItemUI nodeDetailAugmentItemUI = ObjectPoolingEverything.GetFromPool<NodeDetailAugmentItemUI>(GameConstants.nodeDetailAugmentItemUI);

            nodeDetailAugmentItemUI.transform.SetParent(augmentContainer);
            nodeDetailAugmentItemUI.transform.localScale = Vector3.one;

            nodeDetailAugmentItemUI.Init(augment, 760);

            nodeDetailAugmentItemUIs.Add(nodeDetailAugmentItemUI);
        }

        foreach (var spaceObjectType in AugmentManager.Ins.augmentOfSameSpaceObjectType.Keys)
        {
            AugmentSetItemUI augmentSetItemUI = ObjectPoolingEverything.GetFromPool<AugmentSetItemUI>(GameConstants.augmentSetItemUI);

            augmentSetItemUI.transform.SetParent(augmentSetContainer);
            augmentSetItemUI.transform.localScale = Vector3.one;

            augmentSetItemUI.Init(spaceObjectType);

            augmentSetItemUIs.Add(augmentSetItemUI);
        }

        if (updatingPowerOutputCoroutine != null)
        {
            StopCoroutine(updatingPowerOutputCoroutine);
        }

        StartCoroutine(UpdatingPowerOutput());
    }

    private IEnumerator UpdatingPowerOutput()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);

        while (true)
        {
            float powerOutput = PowerManager.Ins.GetTotalPowerOutputPerSecond();

            string primaryHex = DynamicThemeManager.Ins.PrimaryHex;

            powerOutputText.text =
                $"Power Output: <color={primaryHex}>{TextUtil.FormatFloat(powerOutput)} <sprite=0 color={primaryHex}>/s";

            yield return waitForSeconds;
        }
    }
}
