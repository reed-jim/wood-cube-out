using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardItem : MonoBehaviour, ISaferioScrollViewItem
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Image flag;

    [SerializeField] private CountryDataContainer countryDataContainer;

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
            scoreText.text = $"{LeaderboardDataContainer.Items[index].score}";

            // flag.sprite = countryDataContainer.Countries[LeaderboardDataContainer.Items[index].countryIndex].Flag;
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
