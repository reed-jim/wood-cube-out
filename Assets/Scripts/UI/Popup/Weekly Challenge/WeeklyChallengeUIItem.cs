using UnityEngine;

public class WeeklyChallengeUIItem : MonoBehaviour, ISaferioScrollViewItem
{
    public bool IsValidAtIndex(int index)
    {
        return index >= 0;
    }

    public void Refresh(int index)
    {

    }

    public void Setup(int index, RectTransform parent)
    {

    }
}
