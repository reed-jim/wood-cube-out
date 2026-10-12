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
    public RectTransform rewardAugmentContainer;

    private VoyageData voyageData;
    private StarPathConfig starPathConfig;
    private RewardItemUI rewardItemUI;

    public static event Action goNextStarEvent;

    protected override void RegisterMoreEvent()
    {
        base.RegisterMoreEvent();

        SpaceShip.reachStarEvent += Init;

        goNextStarButton.onClick.AddListener(GoNextStar);
    }

    protected override void UnregisterMoreEvent()
    {
        base.UnregisterMoreEvent();

        SpaceShip.reachStarEvent -= Init;
    }

    private void Init(VoyageData voyageData)
    {
        Show();

        starPathConfig = StarManager.Ins.GetVoyageConfig(voyageData.starPathConfigIndex);

        starName.text = $"You've reached {starPathConfig.starName}";

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

        for (int i = 0; i < unlockedAugments.Count; i++)
        {
            NodeDetailAugmentItemUI augmentItemUIs = ObjectPoolingEverything.GetFromPool<NodeDetailAugmentItemUI>(GameConstants.nodeDetailAugmentItemUI);

            augmentItemUIs.transform.SetParent(rewardAugmentContainer);
            augmentItemUIs.transform.localScale = Vector3.one;

            augmentItemUIs.Init(unlockedAugments[i]);
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

        goNextStarEvent?.Invoke();

        Hide();
    }
}
