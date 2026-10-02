using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReachStarPopup : BasePopup
{
    public TMP_Text starName;
    public Button goNextStarButton;
    public RectTransform rewardContainer;
    public RectTransform rewardAugmentContainer;

    private VoyageData voyageData;
    private StarPathConfig starPathConfig;

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

        for (int i = 0; i < starPathConfig.rewards.Count; i++)
        {
            ResourceDataItem rewardData = starPathConfig.rewards[i];
            RewardItemUI rewardItemUI = ObjectPoolingEverything.GetFromPool<RewardItemUI>(GameConstants.rewardItemUI);

            rewardItemUI.transform.SetParent(rewardContainer);
            rewardItemUI.transform.localScale = Vector3.one;

            rewardItemUI.Init(rewardData);
        }

        for (int i = 0; i < starPathConfig.unlockedAugments.Count; i++)
        {
            NodeDetailAugmentItemUI augmentItemUIs = ObjectPoolingEverything.GetFromPool<NodeDetailAugmentItemUI>(GameConstants.nodeDetailAugmentItemUI);

            augmentItemUIs.transform.SetParent(rewardAugmentContainer);
            augmentItemUIs.transform.localScale = Vector3.one;

            augmentItemUIs.Init(starPathConfig.unlockedAugments[i]);
        }

        this.voyageData = voyageData;
    }

    private void GoNextStar()
    {
        for (int i = 0; i < starPathConfig.rewards.Count; i++)
        {
            ResourceDataItem resourceDataItem = starPathConfig.rewards[i];

            ResourceManager.Ins.ChangeResource(resourceDataItem.resourceType, resourceDataItem.value);
        }

        for (int i = 0; i < starPathConfig.unlockedAugments.Count; i++)
        {
            Augment augment = starPathConfig.unlockedAugments[i];

            augment.isUnlocked = true;
        }

        goNextStarEvent?.Invoke();

        Hide();
    }
}
