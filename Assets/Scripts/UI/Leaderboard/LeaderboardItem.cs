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
        LeaderboardUserData[] items = GetLeaderboardDataBasedOnScope();

        if (items != null)
        {
            return index >= 0 && index < items.Count();
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
            LeaderboardUserData[] items = GetLeaderboardDataBasedOnScope();

            gameObject.SetActive(true);

            string originalName = items[index].name;
            int truncateIndex = originalName.IndexOf('#');
            string playerName = truncateIndex >= 0 ? originalName.Substring(0, truncateIndex) : originalName;

            nameText.text = $"{playerName}";
            rankText.text = $"{index + 1}";
            scoreText.text = $"{items[index].score}";

            if (index > 2)
            {
                medalIcon.sprite = medalSprites[3];
            }
            else
            {
                medalIcon.sprite = medalSprites[index];
            }

            flag.sprite = countryDataContainer.Countries[items[index].countryIndex].Flag;
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
        LeaderboardUserData[] items = GetLeaderboardDataBasedOnScope();

        int rank = 0;

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].id == data.id)
            {
                rank = i;

                break;
            }
        }

        try
        {
            gameObject.SetActive(true);

            string originalName = data.name;
            int truncateIndex = originalName.IndexOf('#');
            string playerName = truncateIndex >= 0 ? originalName.Substring(0, truncateIndex) : originalName;

            nameText.text = $"{playerName}";
            rankText.text = $"{rank + 1}";
            scoreText.text = $"{data.score}";

            if (rank > 2)
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

    private LeaderboardUserData[] GetLeaderboardDataBasedOnScope()
    {
        LeaderboardUserData[] items = LeaderboardDataContainer.Items;

        if (GamePersistentVariable.leaderboardScope == GameEnum.LeaderboardScope.Local)
        {
            items = LeaderboardDataContainer.LocalLeaderboardItems;
        }

        return items;
    }

    private void OnLeaderboardDataFetchCompleted()
    {
        Refresh(_index);
    }
}
