using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CountryItemUI : MonoBehaviour
{
    [SerializeField] private Image flagImage;
    [SerializeField] private TextMeshProUGUI countryNameText;
    [SerializeField] private Button selectButton;

    private Country countryData;
    private System.Action<Country> onSelectedCallback;

    private void Awake()
    {
        selectButton.onClick.AddListener(OnButtonClicked);
    }

    public void Setup(Country country, Sprite flag, System.Action<Country> onSelected)
    {
        countryData = country;
        onSelectedCallback = onSelected;

        // Display formatted name with spaces between CamelCase words
        countryNameText.text = System.Text.RegularExpressions.Regex.Replace(country.ToString(), "([a-z])([A-Z])", "$1 $2");
        flagImage.sprite = flag;
        flagImage.enabled = flag != null;
    }

    private void OnButtonClicked()
    {
        onSelectedCallback?.Invoke(countryData);
    }
}
