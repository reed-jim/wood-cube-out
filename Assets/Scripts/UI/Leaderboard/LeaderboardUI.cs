using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PrimeTween;
using Saferio.Util;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LeaderboardUI : MonoBehaviour
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
    #endregion

    [SerializeField] private CanvasGroup inputNameCanvasGroup;
    [SerializeField] private CanvasGroup leaderboardCanvasGroup;

    #region PRIVATE
    private int _numCountriesFetched;
    private int _totalCountries;
    #endregion  

    void Awake()
    {
        continueButton.onClick.AddListener(ConfirmNameAsync);
        openCountryPickerButton.onClick.AddListener(OpenCountryPicker);

        leaderboardCanvasGroup.gameObject.SetActive(false);
        countryPicker.gameObject.SetActive(false);
    }

    void Start()
    {
        CheckShowLeaderboardAsync();

        StartCoroutine(FetchCountries());
    }

    private async Task CheckShowLeaderboardAsync()
    {
        if (DataUtility.Load(GameConstants.DID_USER_LOGIN, false))
        {
            inputNameCanvasGroup.gameObject.SetActive(false);
            leaderboardCanvasGroup.gameObject.SetActive(true);

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            await GameLeaderboardManager.Instance.GetLeaderboardData();

            scrollView.Refresh();
        }
    }

    private async void ConfirmNameAsync()
    {
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        await AuthenticationService.Instance.UpdatePlayerNameAsync(nameInput.text);

        await GameLeaderboardManager.Instance.GetLeaderboardData();

        leaderboardCanvasGroup.gameObject.SetActive(true);
        leaderboardCanvasGroup.alpha = 0;

        Tween.Alpha(inputNameCanvasGroup, 0, duration: 0.3f);
        Tween.Alpha(leaderboardCanvasGroup, 1, duration: 0.3f);

        scrollView.Refresh();

        DataUtility.Save(GameConstants.DID_USER_LOGIN, true);
    }

    private void OpenCountryPicker()
    {
        countryPicker.gameObject.SetActive(!countryPicker.gameObject.activeInHierarchy);
    }

    IEnumerator FetchCountries()
    {
        string url = "https://restcountries.com/v3.1/all";
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            var apiCountries = JsonConvert.DeserializeObject<List<CountryApiResponse>>(json);

            List<Country> countries = new List<Country>();

            foreach (var apiCountry in apiCountries)
            {
                // Country country = new Country
                // {
                //     Name = apiCountry.name.common,
                //     FlagURL = apiCountry.flags.png
                // };

                // countries.Add(country);

                StartCoroutine(LoadSpriteFromURL(apiCountry.name.common, apiCountry.flags.png));
            }

            _totalCountries = apiCountries.Count;
        }
        else
        {
            Debug.LogError("Failed to fetch countries: " + request.error);
        }
    }

    public IEnumerator LoadSpriteFromURL(string countryName, string url)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {

            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                                              new Vector2(0.5f, 0.5f));

                Country country = new Country
                {
                    Name = countryName,
                    Flag = sprite
                };

                if (CountryDataContainer.Countries == null)
                {
                    CountryDataContainer.Countries = new List<Country>();
                }

                CountryDataContainer.Countries.Add(country);

                if (CountryDataContainer.Countries.Count == _totalCountries)
                {
                    CountryDataContainer.Countries = CountryDataContainer.Countries.OrderBy(item => item.Name.First()).ToList();

                    selectedCountryFlag.sprite = CountryDataContainer.Countries[0].Flag;
                    selectedCountryName.text = CountryDataContainer.Countries[0].Name;

                    countryPickerScrollView.Refresh();
                }
            }
        }
    }

    [System.Serializable]
    public class CountryApiResponse
    {
        public Name name;
        public string cca2;
        public Flags flags;
    }

    [System.Serializable]
    public class Name
    {
        public string common;
    }

    [System.Serializable]
    public class Flags
    {
        public string png;
    }
}
