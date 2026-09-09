using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReachStarPopup : BasePopup
{
    public TMP_Text starName;
    public TMP_Text coinRewardText;
    public Button goNextStarButton;

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
        starPathConfig = StarManager.Ins.GetVoyageConfig(voyageData.starPathConfigIndex);

        starName.text = $"You've reached {starPathConfig.starName}";
        coinRewardText.text = $"{starPathConfig.coinReward}";

        this.voyageData = voyageData;
    }

    private void GoNextStar()
    {
        ResourceManager.Ins.ChangeResource(ResourceType.Coin, starPathConfig.coinReward);

        goNextStarEvent?.Invoke();

        Hide();
    }
}
