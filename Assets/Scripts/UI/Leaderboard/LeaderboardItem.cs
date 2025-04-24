using System.Linq;
using TMPro;
using UnityEngine;

public class LeaderboardItem : MonoBehaviour, ISaferioScrollViewItem
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text scoreText;

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
            nameText.text = $"{LeaderboardDataContainer.Items[index].name}";
            scoreText.text = $"{LeaderboardDataContainer.Items[index].score}";
        }
        catch (System.Exception)
        {

        }
    }

    public void Setup(int index, RectTransform parent)
    {
        Refresh(index);
    }
}
