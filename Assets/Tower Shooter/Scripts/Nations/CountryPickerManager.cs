using UnityEngine;

public class CountryPickerManager : MonoBehaviour
{
    [SerializeField] private PooledCountryScrollView countryScrollView;

    private void Start()
    {
        countryScrollView.Initialize(OnCountrySelected);
    }

    private void OnCountrySelected(Country selectedCountry)
    {
        Debug.Log($"Selected Country: {selectedCountry}");
        // Close picker panel or use selected country here
    }
}
