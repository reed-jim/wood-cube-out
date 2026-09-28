using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeadershipCharacterCreationUI : BasePopup
{
    [Header("Input")]
    public TMP_InputField nameInput;
    public Button selectCountryButton;
    [SerializeField] private PooledCountryScrollView countryPicker;
    public Image flag;
    public Slider ageSlider;
    public TMP_Text ageText;
    public Button randomizeNameButton;
    public Button randomizeFaceButton;
    public Button randomizeAllButton;
    public Button addNewCharacterButton;
    public Button boostGoodTraitChanceButton;

    [Header("UI")]
    public TMP_Text goodTraitChanceText;
    public ProceduralHumanAvatarUI proceduralHumanAvatarUI;

    [Header("Exp")]
    public Image expFill;
    public TMP_Text expText;

    public CountryFlagDatabase countryFlagDatabase;
    public CharacterTraitContainer characterTraitContainer;

    private LeadershipCharacterData leadershipCharacterData;

    private string primaryHex;

    private string nameInputString;
    private Country selectedCountry;
    private float goodTraitChance;

    public static Action<string, Action> showConfirmPopupEvent;

    protected override void RegisterMoreEvent()
    {
        base.RegisterMoreEvent();

        LeadershipManager.openCharacterCreationEvent += ShowCharacterCreation;

        nameInput.onEndEdit.AddListener(GetNameInput);
        selectCountryButton.onClick.AddListener(OpenCountryPicker);
        randomizeNameButton.onClick.AddListener(RandomizeName);
        randomizeFaceButton.onClick.AddListener(RandomizeFace);
        randomizeAllButton.onClick.AddListener(RandomizeAll);
        boostGoodTraitChanceButton.onClick.AddListener(BoostGoodTraitChance);
        addNewCharacterButton.onClick.AddListener(ConfirmCreateCharacter);
        ageSlider.onValueChanged.AddListener(OnAgeSliderChanged);
    }

    protected override void UnregisterMoreEvent()
    {
        base.UnregisterMoreEvent();

        LeadershipManager.openCharacterCreationEvent -= ShowCharacterCreation;
    }

    private void Init()
    {
        primaryHex = DynamicThemeManager.Ins.GetHex(ThemeableUI.ElementRole.Primary);

        selectedCountry = Country.UnitedStates;
        proceduralHumanAvatarUI.age = 0.18f;
        goodTraitChance = 0.5f;

        goodTraitChanceText.text = $"Change to get good traits <color={primaryHex}>{(int)(goodTraitChance * 100)}%";

        countryPicker.gameObject.SetActive(false);

        proceduralHumanAvatarUI.GenerateNewRandomCharacter();
    }

    private void ShowCharacterCreation()
    {
        Show();

        Init();
    }

    private void GetNameInput(string name)
    {
        nameInputString = name;
    }

    private void OnAgeSliderChanged(float value)
    {
        proceduralHumanAvatarUI.age = value;
        proceduralHumanAvatarUI.UpdateCharacter(value);

        ageText.text = $"Age {(int)(value * 100)}";
    }

    private void ConfirmCreateCharacter()
    {
        float cost = CurveManager.Ins.GetHireCharactersCost(StarManager.Ins.currentStarPathConfig.id);

        if (ResourceManager.Ins.GetResourceValue(ResourceType.Coin) >= cost)
        {

        }
        else
        {

        }

        showConfirmPopupEvent?.Invoke($"Are you sure to spend {cost} to create this character", () => CreateCharacter());
    }

    private void CreateCharacter()
    {
        LeadershipCharacterData leadershipCharacterData = new LeadershipCharacterData();

        leadershipCharacterData.characterName = nameInputString;
        leadershipCharacterData.country = selectedCountry;

        DateTime birthday = CommonUtil.RandomBirthday(2026 - 55, 2026 - 16);

        birthday = new DateTime(
            2026 - (int)(ageSlider.value * 100),
            birthday.Month,
            birthday.Day
        );

        leadershipCharacterData.birthDayString = birthday.ToString("dd/MM/yyyy");

        List<CharacterTrait> characterTraits = characterTraitContainer.GetRandomTraits(goodTraitChance);
        List<int> characterTraitIndexes = new List<int>();

        for (int j = 0; j < characterTraits.Count; j++)
        {
            characterTraitIndexes.Add(characterTraits[j].id);
        }

        leadershipCharacterData.traitIndexes = characterTraitIndexes;
        leadershipCharacterData.faceSeed = proceduralHumanAvatarUI.seed;
        leadershipCharacterData.electionPoint = 0;
        leadershipCharacterData.chanceToGainElectionPoint = UnityEngine.Random.RandomRange(0.1f, 0.9f);

        LeadershipManager.Ins.AddNewCharacter(leadershipCharacterData);

        Hide();
    }

    private void OpenCountryPicker()
    {
        countryPicker.gameObject.SetActive(true);

        countryPicker.Initialize(onSelect: country =>
        {
            selectedCountry = country;

            flag.sprite = countryFlagDatabase.GetFlag(selectedCountry);

            countryPicker.gameObject.SetActive(false);
        });
    }

    private void RandomizeName()
    {
        string name = CountryNameGenerator.Ins.GetRandomName(selectedCountry);

        nameInput.text = name;
        nameInputString = name;
    }

    private void RandomizeFace()
    {
        proceduralHumanAvatarUI.GenerateNewRandomCharacter();
    }

    private void RandomizeAll()
    {
        selectedCountry = CommonUtil.GetRandomEnumValue<Country>();

        flag.sprite = countryFlagDatabase.GetFlag(selectedCountry);
        
        RandomizeName();

        proceduralHumanAvatarUI.age = UnityEngine.Random.RandomRange(0.18f, 0.8f);
        ageSlider.value = proceduralHumanAvatarUI.age;
        ageText.text = $"Age {(int)(proceduralHumanAvatarUI.age * 100)}";

        RandomizeFace();
    }

    private void BoostGoodTraitChance()
    {
        if (goodTraitChance < 1)
        {
            goodTraitChance += 0.05f;

            goodTraitChanceText.text = $"Change to get good traits <color={primaryHex}>{(int)(goodTraitChance * 100)}%";
        }
        else
        {
            IngameNotificationManager.Ins.ShowNotification(GameConstants.WARNING, "Max boosted!");
        }
    }
}
