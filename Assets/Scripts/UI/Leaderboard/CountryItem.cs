using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
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
            // List<Country> countries = countryDataContainer.Countries;

            // if (!string.IsNullOrEmpty(GamePersistentVariable.countrySearchQuery))
            // {
            //     List<Country> filterCountries =
            //         countryDataContainer.Countries.
            //             Where(c => c.Name.StartsWith(GamePersistentVariable.countrySearchQuery, StringComparison.OrdinalIgnoreCase)).ToList();

            //     countries = filterCountries;
            // }

            // _currentCountry = countries[index];

            // flag.sprite = _currentCountry.Flag;
            // countryNameText.text = _currentCountry.Name;

            // gameObject.SetActive(true);
        }
        catch (System.Exception e)
        {
            gameObject.SetActive(false);

            Debug.Log("SAFERIO " + e.Message);
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
