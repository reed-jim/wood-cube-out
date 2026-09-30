using System;
using System.Collections;
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

    [Header("Ability")]
    public CanvasGroup abilityCanvasGroup;
    public Image rankImage;
    public TMP_Text rankText;
    public RectTransform buffContainer;
    public RectTransform traitContainer;
    public Button boostAbilityPotentialButton;
    public TMP_Text boostAbilityCostText;
    public Button confirmAbilityButton;

    [Header("UI")]
    public TMP_Text abilityPotentialText;
    public ProceduralHumanAvatarUI proceduralHumanAvatarUI;

    [Header("Fx")]
    public Image revealAbilityFx;

    [Header("SO")]
    public CharacterTraitContainer characterTraitContainer;
    public CountryFlagDatabase countryFlagDatabase;
    public CharacterRankAssetContainer characterRankAssetContainer;

    private string primaryHex;

    private string nameInputString;
    private Country selectedCountry;
    private float abilityPotential;
    private int abilityPotentialUpgradeTime;
    private float abilityPotentialUpgradeCost;
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
        abilityPotentialUpgradeTime = 0;

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

    private bool ValidateCharacter()
    {
        bool isValid = true;

        if (string.IsNullOrWhiteSpace(nameInputString))
        {
            isValid = false;

            IngameNotificationManager.Ins.ShowNotification("Character name is not valid!");
        }

        return isValid;
    }

    private void ConfirmCreateCharacter()
    {
        if (!ValidateCharacter())
        {
            return;
        }

        float cost = CurveManager.Ins.GetHireCharactersCost(StarManager.Ins.currentStarPathConfig.id);

        if (ResourceManager.Ins.GetResourceValue(ResourceType.Coin) >= cost)
        {
            showConfirmPopupEvent?.Invoke($"Are you sure to spend {cost} to create this character", () => CreateCharacter());
        }
        else
        {
            IngameNotificationManager.Ins.ShowNotEnoughPower();
        }
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

        StartCoroutine(ShowCharacterAbility());
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
        if (ResourceManager.Ins.GetResourceValue(ResourceType.Coin) < abilityPotentialUpgradeCost)
        {
            IngameNotificationManager.Ins.ShowNotEnoughPower();

            return;
        }

        if (abilityPotential < 1)
        {
            abilityPotential += 0.05f;
            abilityPotentialUpgradeTime++;

            UpdateAbilityPotentialText();
        }
        else
        {
            IngameNotificationManager.Ins.ShowNotification(GameConstants.WARNING, "Max boosted!");
        }
    }

    private void UpdateAbilityPotentialText()
    {
        abilityPotentialUpgradeCost = CurveManager.Ins.GetBuyAugmentCost(abilityPotentialUpgradeTime);

        abilityPotentialText.text = $"Change to get good traits <color={primaryHex}>{(int)(abilityPotential * 100)}%";
        boostAbilityCostText.text = $"{abilityPotentialUpgradeCost}";
    }

    private void RevealCharacterAbility(LeadershipCharacterData leadershipCharacterData)
    {
        rank = CharacterRank.FlightCadet;

        buffs = new List<StatModifier>();

        StatModifier powerBuff = StatModifierGenerator.GenerateFlatModifier(StatType.Damage, abilityPotential);
        StatModifier cycleSpeedBuff = StatModifierGenerator.GenerateFlatModifier(StatType.AttackSpeed, abilityPotential);
        StatModifier critChanceBuff = StatModifierGenerator.GenerateFlatModifier(StatType.CritChance, abilityPotential);
        StatModifier critMultiplierBuff = StatModifierGenerator.GenerateFlatModifier(StatType.CritMultiplier, abilityPotential);

        buffs.Add(powerBuff);
        buffs.Add(cycleSpeedBuff);
        buffs.Add(critChanceBuff);
        buffs.Add(critMultiplierBuff);

        characterTraits = characterTraitContainer.GetRandomTraits(abilityPotential);
        List<int> characterTraitIds = new List<int>();

        for (int j = 0; j < characterTraits.Count; j++)
        {
            characterTraitIds.Add(characterTraits[j].id);
        }

        leadershipCharacterData.rank = rank;
        leadershipCharacterData.buffs = buffs;
        leadershipCharacterData.traitIds = characterTraitIds;
    }

    private IEnumerator ShowCharacterAbility()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1 / 30f);

        int step = 0;
        int maxStep = 2;
        int subStep = 0;
        bool isWaitForNextStep = false;

        rankImage.gameObject.SetActive(false);

        CommonUtil.ReturnListToPool(statItemUIs, item => item.gameObject);
        CommonUtil.ReturnListToPool(characterTraitItemUIs, item => item.gameObject);

        abilityCanvasGroup.gameObject.SetActive(true);

        abilityCanvasGroup.interactable = false;

        Tween.Alpha(abilityCanvasGroup, 1, duration: 0.3f);

        yield return new WaitForSeconds(0.3f);

        while (step <= maxStep)
        {
            if (step == 0 && !isWaitForNextStep)
            {
                PlayRevealAbilityFxAt
                (
                    rankImage.rectTransform,
                    onFxShown: () =>
                    {
                        rankImage.gameObject.SetActive(true);

                        rankImage.sprite = characterRankAssetContainer.GetRankSprite(rank);
                        rankText.text = $"Level {(int)rank + 1}";
                    },
                    onComplete: NextStep
                );

                isWaitForNextStep = true;
            }
            else if (step == 1 && !isWaitForNextStep)
            {
                if (subStep < buffs.Count)
                {
                    StatItemUI statItemUI = ObjectPoolingEverything.GetFromPool<StatItemUI>(GameConstants.statItemUI);

                    CommonUtil.SetParentAndResetScale(statItemUI.transform, buffContainer);

                    PlayRevealAbilityFxAt
                    (
                        statItemUI.container,
                        onFxShown: () =>
                        {
                            StatModifier buff = buffs[subStep];

                            statItemUI.Init(buff.Type, buff.ModType, buff.Value);

                            statItemUIs.Add(statItemUI);
                        },
                        onComplete: NextSubStep
                    );

                    isWaitForNextStep = true;
                }
                else
                {
                    subStep = 0;

                    NextStep();
                }
            }
            else if (step == 2 && !isWaitForNextStep)
            {
                if (subStep < characterTraits.Count)
                {
                    CharacterTraitItemUI item = ObjectPoolingEverything.GetFromPool<CharacterTraitItemUI>(GameConstants.characterTraitItemUI);

                    CommonUtil.SetParentAndResetScale(item.transform, traitContainer);

                    PlayRevealAbilityFxAt
                    (
                        item.container,
                        onFxShown: () =>
                        {
                            item.Init(characterTraits[subStep]);

                            characterTraitItemUIs.Add(item);
                        },
                        onComplete: NextSubStep
                    );

                    isWaitForNextStep = true;
                }
                else
                {
                    NextStep();
                }
            }

            yield return waitForSeconds;
        }

        revealAbilityFx.gameObject.SetActive(false);

        abilityCanvasGroup.interactable = true;

        void NextStep()
        {
            step++;
            isWaitForNextStep = false;
        }

        void NextSubStep()
        {
            subStep++;
            isWaitForNextStep = false;
        }
    }

    private void ConfirmAbility()
    {
        Hide();

        PowerManager.Ins.ShowTotalPowerOutputPerSecond();
    }

    private void PlayRevealAbilityFxAt(RectTransform target, Action onFxShown, Action onComplete)
    {
        revealAbilityFx.transform.SetParent(target);
        revealAbilityFx.transform.localScale = Vector3.zero;
        revealAbilityFx.rectTransform.sizeDelta = target.sizeDelta;
        revealAbilityFx.rectTransform.localPosition = Vector3.zero;
        revealAbilityFx.color = ColorUtil.WithAlpha(revealAbilityFx.color, 0);

        Sequence sequence = Sequence.Create(Tween.Scale(revealAbilityFx.transform, 1.5f, duration: 0.3f));
        sequence.Group(Tween.Alpha(revealAbilityFx, 1, duration: 0.3f).OnComplete(() =>
        {
            onFxShown?.Invoke();
        }));
        sequence.Chain(Tween.Scale(revealAbilityFx.transform, 0, duration: 0.3f));
        sequence.Group(Tween.Alpha(revealAbilityFx, 0, duration: 0.3f));
        sequence.OnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }
    #endregion
}
