using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountryItem : MonoBehaviour, ISaferioScrollViewItem
{
    [SerializeField] private Button selectButton;
    [SerializeField] private Image flag;
    [SerializeField] private TMP_Text countryNameText;

    [SerializeField] private CountryDataContainer countryDataContainer;

    private Country _currentCountry;

    public static event Action<Country> selectCountryEvent;

    void Awake()
    {
        selectButton.onClick.AddListener(Select);
    }

    public bool IsValidAtIndex(int index)
    {
        return index >= 0;
    }

    public void Refresh(int index)
    {
        try
        {
            _currentCountry = countryDataContainer.Countries[index];

            flag.sprite = _currentCountry.Flag;
            countryNameText.text = _currentCountry.Name;
        }
        catch (System.Exception e)
        {

        }
    }

    public void Setup(int index, RectTransform parent)
    {
        Refresh(index);
    }

    private void Select()
    {
        selectCountryEvent?.Invoke(_currentCountry);
    }
}
