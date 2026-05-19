using UnityEngine;

public class AugmentPopup : BasePopup
{
    [SerializeField] private AugmentItemUI[] augmentItemUIs;

    protected override void RegisterMoreEvent()
    {
        base.RegisterMoreEvent();

        AugmentManager.showAugmentSelectionEvent += ShowSelection;
        AugmentItemUI.selectAugmentEvent += Hide;
    }

    protected override void UnregisterMoreEvent()
    {
        base.UnregisterMoreEvent();

        AugmentManager.showAugmentSelectionEvent -= ShowSelection;
        AugmentItemUI.selectAugmentEvent -= Hide;
    }

    private void ShowSelection(AugmentItem[] augmentItems)
    {
        for (int i = 0; i < augmentItems.Length; i++)
        {
            AugmentItem augmentItem = augmentItems[i];
            AugmentItemUI augmentItemUI = augmentItemUIs[i];

            augmentItemUI.Init(augmentItem);
        }

        Show();
    }
}
