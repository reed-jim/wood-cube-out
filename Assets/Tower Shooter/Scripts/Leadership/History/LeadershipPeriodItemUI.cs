using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeadershipPeriodItemUI : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text periodText;
    public Image flag;
    public ProceduralHumanAvatarUI proceduralHumanAvatarUI;

    [Header("SO")]
    public CountryFlagDatabase countryFlagDatabase;

    public void Init(LeadershipPeriodData leadershipPeriodData)
    {
        nameText.text = $"Captain {leadershipPeriodData.leader.characterName}";
        periodText.text = $"{leadershipPeriodData.startDateTimeString} - {leadershipPeriodData.endDateTimeString}";
        flag.sprite = countryFlagDatabase.GetFlag(leadershipPeriodData.leader.country);

        proceduralHumanAvatarUI.GenerateNewRandomCharacterWithSeed(leadershipPeriodData.leader.faceSeed);
        proceduralHumanAvatarUI.age = 1;
    }
}
