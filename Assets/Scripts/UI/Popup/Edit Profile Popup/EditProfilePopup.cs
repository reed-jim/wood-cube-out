using System;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private TMP_InputField countrySearch;
    [SerializeField] private Button saveButton;
    [SerializeField] private ObjectPoolingScrollView scrollView;
    [SerializeField] private ObjectPoolingScrollView countryPickerScrollView;

    [SerializeField] private TMP_Text placeholderCountry;
    [SerializeField] private CanvasGroup countryDisplay;

    #region COUNTRY PICKER
    [SerializeField] private Button openCountryPickerButton;
    [SerializeField] private RectTransform countryPicker;
    [SerializeField] private Image selectedCountryFlag;
    [SerializeField] private TMP_Text selectedCountryName;
    [SerializeField] private CountryDataContainer countryDataContainer;
    #endregion

    #region PRIVATE
    private int _selectedCountryIndex;
    private float _initialCountryDropdownTopMostPositionY;
    #endregion

    protected override void MoreActionInAwake()
    {
        CountryItem.selectCountryEvent += SelectCountry;

        saveButton.onClick.AddListener(ConfirmNameAsync);
        openCountryPickerButton.onClick.AddListener(OpenCountryPicker);

        countrySearch.onValueChanged.AddListener(SearchCountry);

        countryPicker.gameObject.SetActive(false);

        _initialCountryDropdownTopMostPositionY = countryPicker.localPosition.y + 0.5f * countryPicker.sizeDelta.y;
    }

    protected override void MoreActionOnDestroy()
    {
        CountryItem.selectCountryEvent -= SelectCountry;
    }

    protected override void Show(Action onCompletedAction = null)
    {
        Reset();

        base.Show(onCompletedAction);
    }

    private void Reset()
    {
        placeholderCountry.gameObject.SetActive(true);
        countryPicker.gameObject.SetActive(false);
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

        if (_selectedCountryIndex == -1)
        {

        }
    }

    private void SetupCountryPicker()
    {
        _selectedCountryIndex = 244;

        selectedCountryFlag.sprite = countryDataContainer.Countries[_selectedCountryIndex].Flag;
        selectedCountryName.text = countryDataContainer.Countries[_selectedCountryIndex].Name;

        countryPickerScrollView.Refresh();
    }

    private void Setup()
    {
        if (_selectedCountryIndex == -1)
        {
            placeholderCountry.gameObject.SetActive(true);
            countryDisplay.gameObject.SetActive(false);

            saveButton.interactable = false;
        }
        else
        {
            countryDisplay.gameObject.SetActive(true);

            countryDisplay.alpha = 0;

            _tweens.Add(Tween.Alpha(placeholderCountry, 0, duration: 0.3f).OnComplete(() =>
            {
                placeholderCountry.gameObject.SetActive(false);
            }));
            _tweens.Add(Tween.Alpha(countryDisplay, 1, duration: 0.3f));

            saveButton.interactable = true;
        }
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

        _tweens.Add(Tween.Custom(0, 1, duration: 0.3f, onValueChange: newVal =>
        {
            countryPicker.localScale = new Vector3(1, newVal, 1);

            UIUtil.SetLocalPositionY(countryPicker, _initialCountryDropdownTopMostPositionY - 0.5f * countryPicker.sizeDelta.y * newVal);
        }));
    }

    private void HideCountryPicker()
    {
        _tweens.Add(Tween.Custom(1, 0, duration: 0.3f, onValueChange: newVal =>
        {
            countryPicker.localScale = new Vector3(1, newVal, 1);

            UIUtil.SetLocalPositionY(countryPicker, _initialCountryDropdownTopMostPositionY - 0.5f * countryPicker.sizeDelta.y * newVal);
        })
        .OnComplete(() =>
        {
            countryPicker.gameObject.SetActive(false);
        }));
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

        Setup();
    }

    private void SearchCountry(string text)
    {
        GamePersistentVariable.countrySearchQuery = text;

        List<Country> filterCountries =
                    countryDataContainer.Countries.
                        Where(c => c.Name.StartsWith(GamePersistentVariable.countrySearchQuery, StringComparison.OrdinalIgnoreCase)).ToList();

        countryPickerScrollView.SetMaxItemNumber(filterCountries.Count);
        countryPickerScrollView.Refresh();
    }
}
