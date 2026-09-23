using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngameEventDetailUI : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text description;
    [SerializeField] private Image descriptionImage;
    [SerializeField] private RectTransform descriptionContainer;

    private LeadershipCharacterUI electionWinner;

    public StarVoyageContainer starVoyageContainer;

    public void ShowIngameEventDetail(IngameEventType eventType, params object[] args)
    {
        if (eventType == IngameEventType.NewStarDiscovered)
        {
            int starPathConfigId = (int)args[0];

            ShowStarDiscoveryEvent(starPathConfigId);
        }
        else if (eventType == IngameEventType.ElectionComplete)
        {
            LeadershipCharacterData winnerData = (LeadershipCharacterData)args[0];

            ShowElectionCompleteEvent(winnerData);
        }
    }

    private void ShowStarDiscoveryEvent(int starPathConfigId)
    {
        StarPathConfig starPathConfig = starVoyageContainer.starPathConfigs.First(item => item.id == starPathConfigId);

        titleText.text = $"BREAKING NEWS: {starPathConfig.starName} has just been discoverd!";
    }

    private void ShowElectionCompleteEvent(LeadershipCharacterData winnerData)
    {
        titleText.text = $"BREAKING NEWS: {winnerData.characterName} has won the election to become the new {winnerData.leadershipPosition}!";
        description.text = $"{winnerData.characterName} {winnerData.birthDayString}";

        if (electionWinner != null)
        {
            ObjectPoolingEverything.ReturnToPool(GameConstants.leadershipCharacterUI, electionWinner.gameObject);
        }

        LeadershipCharacterUI leadershipCharacterUI =
                ObjectPoolingEverything.GetFromPool<LeadershipCharacterUI>(GameConstants.leadershipCharacterUI);

        leadershipCharacterUI.Init(winnerData);

        CommonUtil.SetParentAndResetScale(leadershipCharacterUI.transform, descriptionContainer, Vector3.zero);

        electionWinner = leadershipCharacterUI;
    }
}
