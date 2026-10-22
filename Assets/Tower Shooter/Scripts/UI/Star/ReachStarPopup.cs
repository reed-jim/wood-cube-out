using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReachStarPopup : BasePopup
{
    public TMP_Text starName;
    public Image starPicture;
    public Button goNextStarButton;
    public RectTransform rewardContainer;
    public ScrollableLayoutGroup scrollableUnlockAugmentContainer;

    private VoyageData voyageData;
    private StarPathConfig starPathConfig;
    private RewardItemUI rewardItemUI;

    private List<NodeDetailAugmentItemUI> unlockAugmentItemUIs = new List<NodeDetailAugmentItemUI>();

    public static event Action goNextStarEvent;

    protected override void RegisterMoreEvent()
    {
        base.RegisterMoreEvent();

        MilestoneFinishEffect.reachStarEvent += Init;

        goNextStarButton.onClick.AddListener(GoNextStar);
    }

    protected override void UnregisterMoreEvent()
    {
        base.UnregisterMoreEvent();

        MilestoneFinishEffect.reachStarEvent -= Init;
    }

    private void Init(VoyageData voyageData)
    {
        Show();

        starPathConfig = StarManager.Ins.GetVoyageConfig(voyageData.starPathConfigIndex);

        string primaryHex = DynamicThemeManager.Ins.PrimaryHex;

        starName.text = $"You've reached <color={primaryHex}>{starPathConfig.starName}";

        // starPicture.sprite = starPathConfig.picture;
        // UIUtil.SetSizeKeepRatioY(starPicture, container.sizeDelta.x);

        ResourceDataItem resourceDataItem = new ResourceDataItem(ResourceType.Coin, CurveManager.Ins.GetVoyageReward(voyageData.starPathConfigIndex));

        if (rewardItemUI == null)
        {
            rewardItemUI = ObjectPoolingEverything.GetFromPool<RewardItemUI>(GameConstants.rewardItemUI);

            rewardItemUI.transform.SetParent(rewardContainer);
            rewardItemUI.transform.localScale = Vector3.one;
        }

        rewardItemUI.Init(resourceDataItem);

        List<Augment> unlockedAugments = AugmentManager.Ins.GetUnlockedAugmentsForSpaceObjectType(starPathConfig.spaceObjectType);

        CommonUtil.ReturnListToPool(unlockAugmentItemUIs, item => item.gameObject);

        scrollableUnlockAugmentContainer.ClearAllItems();

        for (int i = 0; i < unlockedAugments.Count; i++)
        {
            NodeDetailAugmentItemUI augmentItemUI = ObjectPoolingEverything.GetFromPool<NodeDetailAugmentItemUI>(GameConstants.nodeDetailAugmentItemUI);

            scrollableUnlockAugmentContainer.AddItem(augmentItemUI.gameObject);

            augmentItemUI.Init(unlockedAugments[i]);

            unlockAugmentItemUIs.Add(augmentItemUI);
        }

        this.voyageData = voyageData;
    }

    private void GoNextStar()
    {
        ResourceDataItem resourceDataItem = new ResourceDataItem(ResourceType.Coin, CurveManager.Ins.GetVoyageReward(voyageData.starPathConfigIndex));

        ResourceManager.Ins.ChangeResource(resourceDataItem.resourceType, resourceDataItem.value);

        for (int i = 0; i < starPathConfig.unlockedAugments.Count; i++)
        {
            Augment augment = starPathConfig.unlockedAugments[i];

            augment.isUnlocked = true;
        }

        Hide(onCompletedAction: () =>
        {
            goNextStarEvent?.Invoke();
        });
    }
}
