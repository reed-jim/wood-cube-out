using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountryItem : MonoBehaviour, ISaferioScrollViewItem
{
    [SerializeField] private Image flag;
    [SerializeField] private TMP_Text countryNameText;

    public bool IsValidAtIndex(int index)
    {
        return index >= 0;
    }

    public void Refresh(int index)
    {
        try
        {
            flag.sprite = CountryDataContainer.Countries[index].Flag;
            countryNameText.text = CountryDataContainer.Countries[index].Name;
        }
        catch (System.Exception e)
        {
            Debug.Log("SAFERIO exception " + e.Message);
        }
    }

    public void Setup(int index, RectTransform parent)
    {
        // throw new System.NotImplementedException();
    }
}
