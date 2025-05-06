using UnityEngine;
using UnityEngine.UI;

public class GoldPassIntroPopup : BasePopup
{
    [SerializeField] private Button continueButton;

    protected override void RegisterMoreEvent()
    {
        continueButton.onClick.AddListener(Continue);
    }

    private void Continue()
    {
        GoldPassData goldPassData = DataUtility.Load(GameConstants.GOLD_PASS_DATA_CONTAINER, new GoldPassData());

        goldPassData.isIntroDone = true;

        DataUtility.Save(GameConstants.GOLD_PASS_DATA_CONTAINER, goldPassData);

        GamePersistentVariable.goldPassData = goldPassData;

        SwitchRoute(GameEnum.ScreenRoute.GoldPass);
    }
}
