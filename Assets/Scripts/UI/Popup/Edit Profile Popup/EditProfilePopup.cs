using System.Threading.Tasks;
using PrimeTween;
using Saferio.Util;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

public class EditProfilePopup : BasePopup
{
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Button continueButton;
    [SerializeField] private ObjectPoolingScrollView scrollView;
    [SerializeField] private ObjectPoolingScrollView countryPickerScrollView;

    #region COUNTRY PICKER
    [SerializeField] private Button openCountryPickerButton;
    [SerializeField] private RectTransform countryPicker;
    [SerializeField] private Image selectedCountryFlag;
    [SerializeField] private TMP_Text selectedCountryName;
    [SerializeField] private CountryDataContainer countryDataContainer;
    #endregion

    #region PRIVATE
    private int _selectedCountryIndex;
    #endregion

    protected override void MoreActionInAwake()
    {
        CountryItem.selectCountryEvent += SelectCountry;

        continueButton.onClick.AddListener(ConfirmNameAsync);
        openCountryPickerButton.onClick.AddListener(OpenCountryPicker);

        countryPicker.gameObject.SetActive(false);
    }

    protected override void MoreActionOnDestroy()
    {
        CountryItem.selectCountryEvent -= SelectCountry;
    }

    void Start()
    {
        _selectedCountryIndex = DataUtility.Load(GameConstants.SELECTED_COUNTRY_INDEX, -1);

        if (_selectedCountryIndex != -1)
        {
            GamePersistentVariable.selectedCountryIndex = _selectedCountryIndex;
        }
        else
        {
            SetupCountryPicker();
        }
    }

    private void SetupCountryPicker()
    {
        _selectedCountryIndex = 244;

        selectedCountryFlag.sprite = countryDataContainer.Countries[_selectedCountryIndex].Flag;
        selectedCountryName.text = countryDataContainer.Countries[_selectedCountryIndex].Name;

        countryPickerScrollView.Refresh();
    }

    private async void ConfirmNameAsync()
    {
        GamePersistentVariable.selectedCountryIndex = _selectedCountryIndex;

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        await AuthenticationService.Instance.UpdatePlayerNameAsync(nameInput.text);

        await GameLeaderboardManager.Instance.GetLeaderboardData();

        scrollView.Refresh();

        DataUtility.Save(GameConstants.DID_USER_LOGIN, true);
        DataUtility.Save(GameConstants.SELECTED_COUNTRY_INDEX, _selectedCountryIndex);
    }

    private void OpenCountryPicker()
    {
        countryPicker.gameObject.SetActive(true);

        countryPicker.localScale = new Vector3(1, 0, 1);

        Tween.ScaleY(countryPicker, 1, duration: 0.3f);
    }

    private void HideCountryPicker()
    {
        Tween.ScaleY(countryPicker, 0, duration: 0.3f)
        .OnComplete(() =>
        {
            countryPicker.gameObject.SetActive(false);
        });
    }

    private void SelectCountry(Country country)
    {
        selectedCountryFlag.sprite = country.Flag;
        selectedCountryName.text = country.Name;

        UIUtil.SetSizeKeepRatioY(selectedCountryFlag, selectedCountryFlag.rectTransform.sizeDelta.x);

        for (int i = 0; i < countryDataContainer.Countries.Count; i++)
        {
            if (countryDataContainer.Countries[i].Name == country.Name)
            {
                _selectedCountryIndex = i;

                break;
            }
        }

        HideCountryPicker();
    }
}
