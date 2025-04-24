using TMPro;
using UnityEngine;

public class LeaderboardItem : MonoBehaviour, ISaferioScrollViewItem
{
    [SerializeField] private TMP_Text nameText;

    public bool IsValidAtIndex(int index)
    {
        return index >= 0;
    }

    public void Refresh(int index)
    {
        nameText.text = $"Player {index}";
    }

    public void Setup(int index, RectTransform parent)
    {
        nameText.text = $"Player {index}";
    }
}
