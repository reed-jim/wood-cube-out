using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReachStarPopup : BasePopup
{
    public TMP_Text starName;
    public TMP_Text coinRewardText;
    public Button goNextStarButton;
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
        coinRewardText.text = $"{starPathConfig.coinReward}";

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
        ResourceManager.Ins.ChangeResource(ResourceType.Coin, starPathConfig.coinReward);

        goNextStarEvent?.Invoke();

        Hide();
    }
}
