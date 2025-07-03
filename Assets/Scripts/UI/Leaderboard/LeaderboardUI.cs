using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
using static GameEnum;

public class LeaderboardUI : Singleton<LeaderboardUI>
{
    [SerializeField] private ObjectPoolingScrollView leaderboardScrollView;

    #region COUNTRY PICKER
    // [SerializeField] private RectTransform countryPicker;
    #endregion

    [SerializeField] private CanvasGroup leaderboardCanvasGroup;

    #region PRIVATE
    private int _numCountriesFetched;
    private int _totalCountries;
    private int _selectedCountryIndex;
    #endregion

    public static event Action<ScreenRoute> switchRouteEvent;

    protected override void MoreActionOnAwake()
    {
        leaderboardCanvasGroup.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        CheckShowLeaderboardAsync();
    }

    // void Start()
    // {
    //     _selectedCountryIndex = DataUtility.Load(GameConstants.SELECTED_COUNTRY_INDEX, -1);

    //     if (_selectedCountryIndex != -1)
    //     {
    //         GamePersistentVariable.selectedCountryIndex = _selectedCountryIndex;
    //     }
    //     else
    //     {
    //         SetupCountryPicker();
    //     }
    // }

    // private void SetupCountryPicker()
    // {
    //     _selectedCountryIndex = 244;

    //     selectedCountryFlag.sprite = countryDataContainer.Countries[_selectedCountryIndex].Flag;
    //     selectedCountryName.text = countryDataContainer.Countries[_selectedCountryIndex].Name;

    //     countryPickerScrollView.Refresh();
    // }

    private async Task CheckShowLeaderboardAsync()
    {
        if (DataUtility.Load(GameConstants.DID_USER_LOGIN, false))
        {
            leaderboardCanvasGroup.gameObject.SetActive(true);

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            await GameLeaderboardManager.Instance.GetLeaderboardData();

            leaderboardScrollView.Refresh();
        }
        else
        {
            leaderboardCanvasGroup.gameObject.SetActive(false);

            switchRouteEvent?.Invoke(ScreenRoute.EditProfile);
        }
    }

    public void ShowLeaderboard()
    {
        leaderboardCanvasGroup.gameObject.SetActive(true);
        leaderboardCanvasGroup.alpha = 0;

        Tween.Alpha(leaderboardCanvasGroup, 1, duration: 0.3f);
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
                // Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                // Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                //                               new Vector2(0.5f, 0.5f));

                // Country country = new Country
                // {
                //     Name = countryName,
                //     FlagTexture = texture,
                //     Flag = sprite
                // };

                // if (countryDataContainer.Countries == null)
                // {
                //     countryDataContainer.Countries = new List<Country>();
                // }

                // countryDataContainer.Countries.Add(country);

                // if (countryDataContainer.Countries.Count == _totalCountries)
                // {
                //     countryDataContainer.Countries = countryDataContainer.Countries.OrderBy(item => item.Name).ToList();

                //     int flagIndex = 0;

                //     foreach (var item in countryDataContainer.Countries)
                //     {
                //         byte[] bytes = item.FlagTexture.EncodeToPNG();
                //         string path = Path.Combine("Assets/Sprites/Country Flags/", $"flag_{flagIndex}.png");
                //         File.WriteAllBytes(path, bytes);

                //         flagIndex++;
                //     }

                //     selectedCountryFlag.sprite = countryDataContainer.Countries[0].Flag;
                //     selectedCountryName.text = countryDataContainer.Countries[0].Name;

                //     countryPickerScrollView.Refresh();
                // }
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
