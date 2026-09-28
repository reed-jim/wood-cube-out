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
    public TMP_Text selectedCountryText;
    public Image flag;

    [Header("UI")]
    public TMP_Text nameText;
    public TMP_Text ageText;
    public TMP_Text leadershipPositionText;
    public TMP_Text chanceGainElectionPointText;
    public TMP_Text efficiencyText;
    public RectTransform traitContainer;
    public ProceduralHumanAvatarUI proceduralHumanAvatarUI;

    [Header("Exp")]
    public Image expFill;
    public TMP_Text expText;

    public CountryFlagDatabase countryFlagDatabase;

    private LeadershipCharacterData leadershipCharacterData;

    private string primaryHex;

    private string nameInputString;
    private Country selectedCountry;

    private List<CharacterTraitItemUI> characterTraitItemUIs = new List<CharacterTraitItemUI>();

    protected override void RegisterMoreEvent()
    {
        base.RegisterMoreEvent();

        LeadershipManager.openCharacterCreationEvent += ShowCharacterCreation;

        nameInput.onEndEdit.AddListener(GetNameInput);
        selectCountryButton.onClick.AddListener(OpenCountryPicker);
    }

    protected override void UnregisterMoreEvent()
    {
        base.UnregisterMoreEvent();

        LeadershipManager.openCharacterCreationEvent -= ShowCharacterCreation;
    }

    private void GetNameInput(string name)
    {
        nameInputString = name;
    }

    private void CreateCharacter()
    {
        LeadershipCharacterData leadershipCharacterData = new LeadershipCharacterData();

        leadershipCharacterData.characterName = nameInputString;
    }

    private void OpenCountryPicker()
    {
        countryPicker.gameObject.SetActive(true);
        
        countryPicker.Initialize(onSelect: country =>
        {
            selectedCountry = country;
            selectedCountryText.text = $"{country}";

            flag.sprite = countryFlagDatabase.GetFlag(country);
        });
    }

    private void ShowCharacterCreation()
    {
        Show();

        countryPicker.gameObject.SetActive(false);
    }

    public void Init(LeadershipCharacterData data)
    {
        primaryHex = DynamicThemeManager.Ins.GetHex(ThemeableUI.ElementRole.Primary);

        this.leadershipCharacterData = data;

        nameText.text = data.characterName;

        System.DateTime birthDay = System.DateTime.ParseExact(
            data.birthDayString,
            "dd/MM/yyyy",
            System.Globalization.CultureInfo.InvariantCulture
        );
        int age = CommonUtil.GetAge(birthDay, IdleTimeManager.Ins.CurrentDateTime);

        ageText.text = $"{age} years old";
        proceduralHumanAvatarUI.age = age / 100f;
        proceduralHumanAvatarUI.GenerateNewRandomCharacterWithSeed(data.faceSeed);
        flag.sprite = countryFlagDatabase.GetFlag(data.country);
        chanceGainElectionPointText.text = $"Chance gain election point: {(int)(data.chanceToGainElectionPoint * 100)}%";

        int rankValue = (int)data.rank;
        float exp = data.currentExp;
        float expRequiredForNextLevel = CurveManager.Ins.GetBuyAugmentCost(rankValue);
        float expProgress = exp / expRequiredForNextLevel;

        expFill.fillAmount = expProgress;
        expText.text = $"{(int)(expRequiredForNextLevel - exp)}exp required to Level {rankValue + 2}";

        float currentEfficiency = CurveManager.Ins.GetCharacterEfficiencyPercent(rankValue);
        float nextEfficiency = CurveManager.Ins.GetCharacterEfficiencyPercent(rankValue + 1);
        efficiencyText.text = $"Base Efficiency: {currentEfficiency}% --> <color={primaryHex}>{nextEfficiency}%";

        if (data.leadershipPosition == LeadershipPosition.None)
        {
            leadershipPositionText.text = "No position taken";
        }
        else
        {
            leadershipPositionText.text = $"{data.leadershipPosition}";
        }

        CommonUtil.ReturnListToPool(characterTraitItemUIs, item => item.gameObject);

        List<CharacterTrait> characterTraits = LeadershipManager.Ins.GetCharacterTraits(data.traitIndexes);

        for (int i = 0; i < characterTraits.Count; i++)
        {
            CharacterTraitItemUI item = ObjectPoolingEverything.GetFromPool<CharacterTraitItemUI>(GameConstants.characterTraitItemUI);

            CommonUtil.SetParentAndResetScale(item.transform, traitContainer);
            item.Init(characterTraits[i]);

            characterTraitItemUIs.Add(item);
        }
    }

    private void RemoveFromLeaderPosition()
    {
        LeadershipManager.Ins.RemoveFromLeaderPosition(leadershipCharacterData.leadershipPosition);

        leadershipCharacterData.leadershipPosition = LeadershipPosition.None;

        LeadershipManager.Ins.SaveLeadershipData();
        LeadershipManager.Ins.Refresh();

        Hide();
    }
}
