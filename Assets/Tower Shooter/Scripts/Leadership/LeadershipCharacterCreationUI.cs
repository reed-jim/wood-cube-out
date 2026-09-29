using System;
using System.Collections.Generic;
using System.Linq;
using PrimeTween;
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
    public Button createCharacterButton;
    public Button boostAbilityPotentialButton;

    [Header("Ability")]
    public CanvasGroup abilityCanvasGroup;
    public Image rankImage;
    public TMP_Text rankText;
    public RectTransform buffContainer;
    public RectTransform traitContainer;
    public Button confirmAbilityButton;

    [Header("UI")]
    public TMP_Text abilityPotentialText;
    public ProceduralHumanAvatarUI proceduralHumanAvatarUI;

    [Header("SO")]
    public CharacterTraitContainer characterTraitContainer;
    public CountryFlagDatabase countryFlagDatabase;
    public CharacterRankAssetContainer characterRankAssetContainer;

    private LeadershipCharacterData leadershipCharacterData;

    private string primaryHex;

    private string nameInputString;
    private Country selectedCountry;
    private float abilityPotential;
    private CharacterRank rank;
    private List<StatModifier> buffs;
    private List<CharacterTrait> characterTraits;

    public static Action<string, Action> showConfirmPopupEvent;

    private List<StatItemUI> statItemUIs = new List<StatItemUI>();
    private List<CharacterTraitItemUI> characterTraitItemUIs = new List<CharacterTraitItemUI>();

    protected override void RegisterMoreEvent()
    {
        base.RegisterMoreEvent();

        LeadershipManager.openCharacterCreationEvent += ShowCharacterCreation;

        nameInput.onEndEdit.AddListener(GetNameInput);
        selectCountryButton.onClick.AddListener(OpenCountryPicker);
        randomizeNameButton.onClick.AddListener(RandomizeName);
        randomizeFaceButton.onClick.AddListener(RandomizeFace);
        randomizeAllButton.onClick.AddListener(RandomizeAll);
        boostAbilityPotentialButton.onClick.AddListener(BoostAbilityPotential);
        createCharacterButton.onClick.AddListener(ConfirmCreateCharacter);
        confirmAbilityButton.onClick.AddListener(ConfirmAbility);
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
        abilityPotential = 0.5f;

        UpdateAbilityPotentialText();

        proceduralHumanAvatarUI.GenerateNewRandomCharacter();

        countryPicker.gameObject.SetActive(false);
        abilityCanvasGroup.gameObject.SetActive(false);
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

        leadershipCharacterData.faceSeed = proceduralHumanAvatarUI.seed;
        leadershipCharacterData.electionPoint = 0;
        leadershipCharacterData.chanceToGainElectionPoint = UnityEngine.Random.RandomRange(0.1f, 0.9f);

        RevealCharacterAbility(leadershipCharacterData);

        LeadershipManager.Ins.AddNewCharacter(leadershipCharacterData);

        ShowCharacterAbility();
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

    #region Ability
    private void BoostAbilityPotential()
    {
        if (abilityPotential < 1)
        {
            abilityPotential += 0.05f;

            UpdateAbilityPotentialText();
        }
        else
        {
            IngameNotificationManager.Ins.ShowNotification(GameConstants.WARNING, "Max boosted!");
        }
    }

    private void UpdateAbilityPotentialText()
    {
        abilityPotentialText.text = $"Change to get good traits <color={primaryHex}>{(int)(abilityPotential * 100)}%";
    }

    private void RevealCharacterAbility(LeadershipCharacterData leadershipCharacterData)
    {
        rank = CharacterRank.FlightCadet;

        buffs = new List<StatModifier>();

        StatModifier powerBuff = StatModifierGenerator.Generate(StatType.Damage, abilityPotential);
        StatModifier cycleSpeedBuff = StatModifierGenerator.Generate(StatType.AttackSpeed, abilityPotential);
        StatModifier critChanceBuff = StatModifierGenerator.Generate(StatType.CritChance, abilityPotential);
        StatModifier critMultiplierBuff = StatModifierGenerator.Generate(StatType.CritMultiplier, abilityPotential);

        buffs.Add(powerBuff);
        buffs.Add(cycleSpeedBuff);
        buffs.Add(critChanceBuff);
        buffs.Add(critMultiplierBuff);

        List<CharacterTrait> characterTraits = characterTraitContainer.GetRandomTraits(abilityPotential);
        List<int> characterTraitIndexes = new List<int>();

        for (int j = 0; j < characterTraits.Count; j++)
        {
            characterTraitIndexes.Add(characterTraits[j].id);
        }

        leadershipCharacterData.rank = rank;
        leadershipCharacterData.buffs = buffs;
        leadershipCharacterData.traitIndexes = characterTraitIndexes;
    }

    private void ShowCharacterAbility()
    {
        rankImage.sprite = characterRankAssetContainer.GetRankSprite(rank);
        rankText.text = $"Level {(int)rank + 1}";

        CommonUtil.ReturnListToPool(statItemUIs, item => item.gameObject);
        CommonUtil.ReturnListToPool(characterTraitItemUIs, item => item.gameObject);

        for (int i = 0; i < buffs.Count; i++)
        {
            StatModifier buff = buffs[i];

            StatItemUI statItemUI = ObjectPoolingEverything.GetFromPool<StatItemUI>(GameConstants.statItemUI);

            CommonUtil.SetParentAndResetScale(statItemUI.transform, buffContainer);

            statItemUI.Init(buff.Type, buff.ModType, buff.Value);

            statItemUIs.Add(statItemUI);
        }

        for (int i = 0; i < characterTraits.Count; i++)
        {
            CharacterTraitItemUI item = ObjectPoolingEverything.GetFromPool<CharacterTraitItemUI>(GameConstants.characterTraitItemUI);

            CommonUtil.SetParentAndResetScale(item.transform, traitContainer);
            item.Init(characterTraits[i]);

            characterTraitItemUIs.Add(item);
        }

        abilityCanvasGroup.gameObject.SetActive(true);

        abilityCanvasGroup.interactable = false;

        Tween.Alpha(abilityCanvasGroup, 1, duration: 0.3f)
        .OnComplete(() =>
        {
            abilityCanvasGroup.interactable = true;
        });
    }

    private void ConfirmAbility()
    {
        Hide();
    }
    #endregion
}
