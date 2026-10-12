using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AugmentSelectionPopup : BasePopup
{
    [SerializeField] private Button rerollButton;

    public List<AugmentItemUI> augmentItems;

    public AugmentContainer augmentContainer;

    protected override void RegisterMoreEvent()
    {
        base.RegisterMoreEvent();

        rerollButton.onClick.AddListener(Reroll);
    }

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);

        Init();
    }

    public void Init()
    {
        List<Augment> randomAugments = GetRandomAugments();

        for (int i = 0; i < augmentItems.Count; i++)
        {
            augmentItems[i].Init(randomAugments[i], onAugmentSelected: Hide);
        }
    }

    public List<Augment> GetRandomAugments()
    {
        List<Augment> validAugments = new List<Augment>();

        for (int i = 0; i < augmentContainer.augments.Count; i++)
        {
            Augment augment = augmentContainer.augments[i];

            if (augment.spaceObjectType <= StarManager.Ins.currentStarPathConfig.spaceObjectType)
            {
                validAugments.Add(augment);
            }
        }

        return CommonUtil.GetRandomItems(validAugments, 3);
    }

    private void Reroll()
    {
        Init();
    }
}
