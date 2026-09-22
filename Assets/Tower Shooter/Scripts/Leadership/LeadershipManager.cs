using System;
using System.Collections.Generic;
using System.Globalization;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public enum LeadershipPosition
{
    None,

    // Command
    Captain,
    FirstOfficer,

    // Navigation & Operations
    Navigator,
    OperationsOfficer,
    CommunicationsOfficer,

    // Engineering
    ChiefEngineer,
    SystemsEngineer,

    // Science
    ScienceOfficer,
    ResearchOfficer,

    // Ship Support
    MedicalOfficer,
    Quartermaster,
    MaintenanceOfficer,
}

public class LeadershipManager : Singleton<LeadershipManager>
{
    public RectTransform container;
    public RectTransform characterContainer;
    public Button closeButton;
    public Button hireCharacterButton;
    public Button electionButton;

    [Header("Customize")]
    public int maxCharacter;

    public CharacterTrailDetail characterTrailDetail;
    public ElectionPopup electionPopup;

    public LeadershipPositionUI selectedLeadershipPosition;

    public CharacterTraitContainer characterTraitContainer;

    public LeadershipData leadershipData;

    [Header("Leadership Positions")]
    public List<LeadershipPositionUI> leadershipPositionUIs;

    [Header("Bonus Stat")]
    public List<StatModifier> statModifiers = new List<StatModifier>();

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        MultiverseIdleGameplayScreen.openLeadershipUIEvent += Show;
        IdleTimeManager.OnElectionStarted += ShowElection; // Trigger election event automatically

        closeButton.onClick.AddListener(Hide);
        hireCharacterButton.onClick.AddListener(HireNewCharacters);
        electionButton.onClick.AddListener(ShowElection);

        container.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        MultiverseIdleGameplayScreen.openLeadershipUIEvent -= Show;
        IdleTimeManager.OnElectionStarted -= ShowElection;
    }

    void Start()
    {
        LoadLeadershipData();
    }

    public void Init()
    {
        LoadCurrentCharacters();
    }

    private void LoadCurrentCharacters()
    {
        for (int i = 0; i < leadershipData.leadershipCharacterDataItems.Count; i++)
        {
            LeadershipCharacterData leadershipCharacterData = leadershipData.leadershipCharacterDataItems[i];

            LoadLeadershipCharacter(leadershipCharacterData);
        }
    }

    private void LoadLeadershipCharacter(LeadershipCharacterData leadershipCharacterData)
    {
        LeadershipCharacterUI leadershipCharacterUI =
                ObjectPoolingEverything.GetFromPool<LeadershipCharacterUI>(GameConstants.leadershipCharacterUI);

        Country country = leadershipCharacterData.country;
        string name = leadershipCharacterData.characterName;
        DateTime birthDay = DateTime.ParseExact(
            leadershipCharacterData.birthDayString,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture
        );
        int age = CommonUtil.GetAge(birthDay, DateTime.Today);

        List<CharacterTrait> characterTraits = new List<CharacterTrait>();

        for (int j = 0; j < characterTraitContainer.characterTraits.Count; j++)
        {
            CharacterTrait characterTrait = characterTraitContainer.characterTraits[j];

            if (leadershipCharacterData.traitIndexes.Contains(characterTrait.id))
            {
                characterTraits.Add(characterTrait);
            }
        }

        leadershipCharacterUI.Init(leadershipCharacterData, characterTraits);

        CommonUtil.SetParentAndResetScale(leadershipCharacterUI.transform, characterContainer, Vector3.zero);
        leadershipCharacterUI.transform.localScale *= 0.48f;
    }

    public void HireNewCharacters()
    {
        int numCharacterToHire = 3;

        int currentCharacter = leadershipData.leadershipCharacterDataItems.Count;

        numCharacterToHire = Mathf.Min(numCharacterToHire, maxCharacter - currentCharacter);

        if (numCharacterToHire == 0)
        {
            IngameNotificationManager.Ins.ShowNotification(GameConstants.WARNING, "You have reached max character!");

            return;
        }

        for (int i = 0; i < numCharacterToHire; i++)
        {
            LeadershipCharacterData leadershipCharacterData = GetRandomLeadershipCharacterData();

            leadershipData.leadershipCharacterDataItems.Add(leadershipCharacterData);

            LoadLeadershipCharacter(leadershipCharacterData);
        }
    }

    public void SelectLeadershipPosition(LeadershipPositionUI leadershipPositionUI)
    {
        selectedLeadershipPosition = leadershipPositionUI;
    }

    public void AssignLeadershipPosition(LeadershipCharacterUI leadershipCharacterUI)
    {
        selectedLeadershipPosition.AssignCharacter(leadershipCharacterUI);

        CalculateBonusStat();
    }

    private void Show()
    {
        container.gameObject.SetActive(true);
        container.localPosition = new Vector3(0, -1.5f * container.sizeDelta.y);

        Tween.LocalPositionY(container, -70, duration: 0.3f);

        Init();
    }

    private void Hide()
    {
        Tween.LocalPositionY(container, -1.5f * container.sizeDelta.y, duration: 0.3f)
        .OnComplete(() =>
        {
            container.gameObject.SetActive(false);
        });
    }

    #region Bonus Stat
    public void CalculateBonusStat()
    {
        StatModifier damageStatModifier = new StatModifier(StatType.Damage, ModifierType.PercentMult, 0);
        StatModifier speedStatModifier = new StatModifier(StatType.AttackSpeed, ModifierType.PercentMult, 0);
        StatModifier critChanceStatModifier = new StatModifier(StatType.CritChance, ModifierType.PercentMult, 0);
        StatModifier critMultiplierStatModifier = new StatModifier(StatType.CritMultiplier, ModifierType.PercentMult, 0);

        if (statModifiers.Count == 0)
        {
            statModifiers.Add(damageStatModifier);
            statModifiers.Add(speedStatModifier);
            statModifiers.Add(critChanceStatModifier);
            statModifiers.Add(critMultiplierStatModifier);
        }

        for (int i = 0; i < leadershipPositionUIs.Count; i++)
        {
            LeadershipPositionUI leadershipPositionUI = leadershipPositionUIs[i];

            float efficiency = leadershipPositionUI.efficiency;

            if (leadershipPositionUI.characterUIContainer != null)
            {
                for (int j = 0; j < leadershipPositionUI.statModifierDataItems.Count; j++)
                {
                    StatModifierData statModifierData = leadershipPositionUI.statModifierDataItems[j];

                    for (int k = 0; k < statModifiers.Count; k++)
                    {
                        if (statModifiers[k].Type == statModifierData.statType)
                        {
                            statModifiers[k].Value += efficiency * statModifierData.value;
                        }
                    }
                }
            }
        }

        // Apply Stats to Nodes
        for (int i = 0; i < NodeManager.Ins.nodes.Count; i++)
        {
            BaseCharacter node = NodeManager.Ins.nodes[i];

            for (int j = 0; j < statModifiers.Count; j++)
            {
                node.serviceLocator.stats.AddModifier(statModifiers[j]);
            }
        }
    }
    #endregion

    #region Character Trait
    public void ShowTraitDetail(CharacterTraitItemUI characterTraitItemUI)
    {
        CommonUtil.SetParentAndResetScale(characterTrailDetail.transform, characterTraitItemUI.transform);

        characterTrailDetail.ShowAt(characterTraitItemUI);
    }
    #endregion

    #region Election
    private void ShowElection()
    {
        electionPopup.Show();
    }
    #endregion

    #region Save/Load
    private void LoadLeadershipData()
    {
        leadershipData = DataUtility.Load(SaveSlotManager.Ins.GetSaveKey("leadershipData"), GetDefaultLeadershipData());

        if (!leadershipData.isScoutAnyCharacter)
        {
            leadershipData.isScoutAnyCharacter = true;

            SaveLeadershipData();
        }
    }

    private void SaveLeadershipData()
    {
        DataUtility.Save(SaveSlotManager.Ins.GetSaveKey("leadershipData"), leadershipData);
    }

    private LeadershipData GetDefaultLeadershipData()
    {
        LeadershipData defaultLeadershipData = new LeadershipData();

        defaultLeadershipData.isScoutAnyCharacter = false;
        defaultLeadershipData.leadershipCharacterDataItems = new List<LeadershipCharacterData>();

        int numStartCharacter = 3;

        for (int i = 0; i < numStartCharacter; i++)
        {
            LeadershipCharacterData leadershipCharacterData = GetRandomLeadershipCharacterData();

            defaultLeadershipData.leadershipCharacterDataItems.Add(leadershipCharacterData);
        }

        return defaultLeadershipData;
    }

    private LeadershipCharacterData GetRandomLeadershipCharacterData()
    {
        Country country = CommonUtil.GetRandomEnumValue<Country>();

        string name = CountryNameGenerator.Ins.GetRandomName(country);

        DateTime birthday = CommonUtil.RandomBirthday(2026 - 55, 2026 - 16);

        List<CharacterTrait> characterTraits = characterTraitContainer.GetRandomTraits();
        List<int> characterTraitIndexes = new List<int>();

        for (int j = 0; j < characterTraits.Count; j++)
        {
            characterTraitIndexes.Add(characterTraits[j].id);
        }

        LeadershipCharacterData leadershipCharacterData = new LeadershipCharacterData();

        leadershipCharacterData.characterName = name;
        leadershipCharacterData.country = country;
        leadershipCharacterData.birthDayString = birthday.ToString("yyyy-MM-dd");
        leadershipCharacterData.traitIndexes = characterTraitIndexes;

        return leadershipCharacterData;
    }
    #endregion
}

[Serializable]
public class LeadershipData
{
    public bool isScoutAnyCharacter;
    public List<LeadershipCharacterData> leadershipCharacterDataItems;
}

[Serializable]
public class LeadershipCharacterData
{
    public string characterName;
    public Country country;
    public string birthDayString;
    public List<int> traitIndexes;
    public LeadershipPosition leadershipPosition;
}
