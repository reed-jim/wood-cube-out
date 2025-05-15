using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpaceRaceItem : MonoBehaviour
{
    [SerializeField] private RectTransform container;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private RectTransform spaceShip;
    [SerializeField] private Image rewardIcon;
    [SerializeField] private Image medalIcon;

    [SerializeField] private Sprite[] rewardSprites;
    [SerializeField] private Sprite[] medalSprites;

    public void Setup(int index)
    {
        SpaceRaceData spaceRaceData = DataUtility.Load(GameConstants.SPACE_RACE_DATA, new SpaceRaceData());

        // Find rank
        int raceRank = 0;

        LeaderboardUserData[] sortedArray = spaceRaceData.spaceRacePlayerGroup.OrderBy(item => item.score).ToArray();

        for (int i = 0; i < sortedArray.Length; i++)
        {
            if (sortedArray[i].score == spaceRaceData.spaceRacePlayerGroup[index].score)
            {
                raceRank = i + 1;

                break;
            }
        }

        playerNameText.text = GetTruncatedPlayerName(spaceRaceData.spaceRacePlayerGroup[index].name);
        rankText.text = $"{raceRank}";

        if (raceRank == 4)
        {
            rewardIcon.gameObject.SetActive(false);
        }
        else
        {
            rewardIcon.gameObject.SetActive(true);

            rewardIcon.sprite = rewardSprites[raceRank - 1];
            medalIcon.sprite = medalSprites[raceRank - 1];
        }

        int numLevelToWin = 15;

        float progress = (float)(spaceRaceData.spaceRacePlayerGroup[index].score / numLevelToWin);

        UIUtil.SetLocalPositionX(spaceShip, spaceShip.localPosition.x + (1 - progress) * 1.5f * spaceShip.sizeDelta.x);
    }

    private string GetTruncatedPlayerName(string originalName)
    {
        try
        {
            gameObject.SetActive(true);

            int truncateIndex = originalName.IndexOf('#');
            string playerName = truncateIndex >= 0 ? originalName.Substring(0, truncateIndex) : originalName;

            return playerName;
        }
        catch (System.Exception)
        {
            gameObject.SetActive(false);
        }

        return originalName;
    }
}
