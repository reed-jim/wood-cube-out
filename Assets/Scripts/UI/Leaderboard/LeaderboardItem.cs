using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardItem : MonoBehaviour, ISaferioScrollViewItem
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Image medalIcon;
    [SerializeField] private Image flag;

    [SerializeField] private CountryDataContainer countryDataContainer;

    [SerializeField] private Sprite[] medalSprites;

    private int _index;

    void Awake()
    {
        GameLeaderboardManager.fetchLeaderboardDataCompletedEvent += OnLeaderboardDataFetchCompleted;
    }

    void OnDestroy()
    {
        GameLeaderboardManager.fetchLeaderboardDataCompletedEvent -= OnLeaderboardDataFetchCompleted;
    }

    public bool IsValidAtIndex(int index)
    {
        if (LeaderboardDataContainer.Items != null)
        {
            return index >= 0 && index < LeaderboardDataContainer.Items.Count();
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
            }
            else
            {
                medalIcon.sprite = medalSprites[index];
            }

            flag.sprite = countryDataContainer.Countries[LeaderboardDataContainer.Items[index].countryIndex].Flag;
        }
        catch (System.Exception)
        {
            gameObject.SetActive(false);
        }

        _index = index;
    }

    public void Setup(int index, RectTransform parent)
    {
        Refresh(index);
    }

    public async Task UpdateUI(LeaderboardUserData data)
    {
        int rank = await GameLeaderboardManager.Instance.GetCurrentPlayerRankAsync();

        try
        {
            gameObject.SetActive(true);

            string originalName = data.name;
            int truncateIndex = originalName.IndexOf('#');
            string playerName = truncateIndex >= 0 ? originalName.Substring(0, truncateIndex) : originalName;

            nameText.text = $"{playerName}";
            rankText.text = $"{rank + 1}";
            scoreText.text = $"{data.score}";

            if (rank >= 2)
            {
                medalIcon.sprite = medalSprites[3];
            }
            else
            {
                medalIcon.sprite = medalSprites[rank];
            }

            flag.sprite = countryDataContainer.Countries[data.countryIndex].Flag;
        }
        catch (System.Exception)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnLeaderboardDataFetchCompleted()
    {
        Refresh(_index);
    }
}
