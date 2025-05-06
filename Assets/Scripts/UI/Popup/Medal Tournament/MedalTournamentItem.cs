using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MedalTournamentItem : MonoBehaviour, ISaferioScrollViewItem
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Image medalIcon;
    [SerializeField] private Image rewardIcon;

    [SerializeField] private Sprite[] medalSprites;
    [SerializeField] private Sprite[] rewardSprites;

    public bool IsValidAtIndex(int index)
    {
        if (LeaderboardDataContainer.Items != null)
        {
            return index >= 0 && index < LeaderboardDataContainer.Items.Length;
        }
        else
        {
            return index >= 0;
        }
    }

    public void Refresh(int index)
    {
        try
        {
            gameObject.SetActive(true);

            string originalName = LeaderboardDataContainer.Items[index].name;
            int truncateIndex = originalName.IndexOf('#');
            string playerName = truncateIndex >= 0 ? originalName.Substring(0, truncateIndex) : originalName;

            nameText.text = $"{playerName}";
            rankText.text = $"{index + 1}";
            scoreText.text = $"{LeaderboardDataContainer.Items[index].score}";

            if (index >= 2)
            {
                medalIcon.sprite = medalSprites[3];
                rewardIcon.sprite = rewardSprites[3];
            }
            else
            {
                medalIcon.sprite = medalSprites[index];
                rewardIcon.sprite = rewardSprites[index];
            }
        }
        catch (System.Exception)
        {
            gameObject.SetActive(false);
        }
    }

    public void Setup(int index, RectTransform parent)
    {
        Refresh(index);
    }
}
