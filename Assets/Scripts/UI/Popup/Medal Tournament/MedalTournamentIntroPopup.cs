using UnityEngine;
using UnityEngine.UI;

public class MedalTournamentIntroPopup : BasePopup
{
    [SerializeField] private Button continueButton;

    protected override void RegisterMoreEvent()
    {
        continueButton.onClick.AddListener(Continue);
    }

    private void Continue()
    {
        MedalTournamentData medalTournamentData = new MedalTournamentData();

        medalTournamentData.isIntroDone = true;

        DataUtility.Save(GameConstants.MEDAL_TOURNAMENT_DATA, medalTournamentData);

        GamePersistentVariable.medalTournamentData = medalTournamentData;

        SwitchRoute(GameEnum.ScreenRoute.MedalTournament);
    }
}
