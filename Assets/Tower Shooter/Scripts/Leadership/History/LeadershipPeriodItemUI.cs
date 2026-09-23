using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeadershipPeriodItemUI : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text periodText;
    public TMP_Text activityText;
    public Image flag;
    public ProceduralHumanAvatarUI proceduralHumanAvatarUI;

    [Header("SO")]
    public CountryFlagDatabase countryFlagDatabase;
    public StarVoyageContainer starVoyageContainer;

    private string primaryHex;

    public void Init(LeadershipPeriodData leadershipPeriodData)
    {
        primaryHex = DynamicThemeManager.Ins.GetHex(ThemeableUI.ElementRole.Primary);

        nameText.text = $"CAPTAIN {leadershipPeriodData.leader.characterName}";
        if (string.IsNullOrEmpty(leadershipPeriodData.endDateTimeString))
        {
            periodText.text = $"{leadershipPeriodData.startDateTimeString} - Now";
        }
        else
        {
            periodText.text = $"{leadershipPeriodData.startDateTimeString} - {leadershipPeriodData.endDateTimeString}";
        }

        StringBuilder activityContent = new StringBuilder();

        if (leadershipPeriodData.starDiscoveryEventDataItems.Count == 0)
        {
            activityText.text = "No activity.";
        }
        else
        {
            for (int i = 0; i < leadershipPeriodData.starDiscoveryEventDataItems.Count; i++)
            {
                StarDiscoveryEventDataItem starDiscoveryEventDataItem = leadershipPeriodData.starDiscoveryEventDataItems[i];

                StarPathConfig starPathConfig = null;

                for (int j = 0; j < starVoyageContainer.starPathConfigs.Count; j++)
                {
                    if (starVoyageContainer.starPathConfigs[j].id == starDiscoveryEventDataItem.starPathConfigId)
                    {
                        starPathConfig = starVoyageContainer.starPathConfigs[j];

                        break;
                    }
                }

                activityContent.AppendLine($"{starDiscoveryEventDataItem.dateTimeString}: Discovered <color={primaryHex}>{starPathConfig?.starName}");
            }

            activityText.text = activityContent.ToString().TrimEnd();
        }

        flag.sprite = countryFlagDatabase.GetFlag(leadershipPeriodData.leader.country);

        System.DateTime birthDay = System.DateTime.ParseExact(
            leadershipPeriodData.leader.birthDayString,
            "dd/MM/yyyy",
            System.Globalization.CultureInfo.InvariantCulture
        );
        int age = CommonUtil.GetAge(birthDay, IdleTimeManager.Ins.CurrentDateTime);

        proceduralHumanAvatarUI.age = Mathf.Min(age, 1);
        proceduralHumanAvatarUI.GenerateNewRandomCharacterWithSeed(leadershipPeriodData.leader.faceSeed);
    }
}
