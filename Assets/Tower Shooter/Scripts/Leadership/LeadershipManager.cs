using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
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
    public ScrollableLayoutGroup scrollableCharacterContainer;
    public RectTransform characterContainer;
    public Button closeButton;
    public Button hireCharacterButton;
    public TMP_Text hireCharacterCostText;
    public CanvasGroup overlayCanvasGroup;

    public Button leadershipPeriodButton;
    public Button electionButton;
    public TMP_Text candidatesCountText;
    public TMP_Text totalEffectText;

    [Header("Switchable Screen")]
    public SwitchableScreenType switchableScreenType;

    [Header("Appoint")]
    public CanvasGroup appointCharacterOverlayCanvasGroup;
    public Button cancelAppointCharacterButton;

    [Header("Remove")]
    public CanvasGroup removeCharacterOverlayCanvasGroup;
    public Button confirmRemoveCharacterButton;
    public Button enterRemoveCharacterModeButton;
    public Button exitRemoveCharacterButton;

    [Header("Customize")]
    public int maxCharacter;

    public CharacterTrailDetail characterTrailDetail;
    public ElectionPopup electionPopup;

    public LeadershipPositionUI selectedLeadershipPosition;

    public CharacterTraitContainer characterTraitContainer;

    public LeadershipData leadershipData;

    [Header("Leadership Positions")]
    public List<LeadershipPositionUI> leadershipPositionUIs;
    private float hireNewCharactersCost;
    public float forceElectionCost;

    [Header("Bonus Stat")]
    public List<StatModifier> statModifiers = new List<StatModifier>();

    [Header("SO")]
    public StatColorConfigSO statColorConfigSO;

    private List<LeadershipCharacterUI> leadershipCharacterUIs = new List<LeadershipCharacterUI>();
    private LeadershipCharacterUI currentCaptain;

    public static event Action openLeadershipPeriodScreenEvent;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        MultiverseIdleGameplayScreen.openLeadershipUIEvent += Show;
        SwitchableScreenButton.switchScreenEvent += OnScreenSwitch;
        IdleTimeManager.OnElectionStarted += ShowElection; // Trigger election event automatically

        closeButton.onClick.AddListener(Hide);
        hireCharacterButton.onClick.AddListener(HireNewCharacters);
        cancelAppointCharacterButton.onClick.AddListener(CancelSelectLeadershipPosition);

        confirmRemoveCharacterButton.onClick.AddListener(ConfirmRemoveCharacters);
        enterRemoveCharacterModeButton.onClick.AddListener(EnterRemoveCharacterMode);
        exitRemoveCharacterButton.onClick.AddListener(ExitRemoveCharacterMode);

        leadershipPeriodButton.onClick.AddListener(OpenLeadershipPeriodScreen);
        electionButton.onClick.AddListener(ShowElection);

        container.gameObject.SetActive(false);
        appointCharacterOverlayCanvasGroup.gameObject.SetActive(false);
        removeCharacterOverlayCanvasGroup.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        MultiverseIdleGameplayScreen.openLeadershipUIEvent -= Show;
        SwitchableScreenButton.switchScreenEvent -= OnScreenSwitch;
        IdleTimeManager.OnElectionStarted -= ShowElection;
    }

    public void LoadData()
    {
        LoadLeadershipData();
        CalculateBonusStat();
    }

    public void Init()
    {
        LoadCurrentCharacters();

        hireNewCharactersCost = CurveManager.Ins.GetHireCharactersCost(StarManager.Ins.currentStarPathConfig.id);

        hireCharacterCostText.text = $"{hireNewCharactersCost}";

        forceElectionCost = CurveManager.Ins.GetHireCharactersCost(DataUtility.Load(SaveSlotManager.Ins.GetSaveKey("forceElectionTime"), 0));
    }

    private void LoadCurrentCharacters()
    {
        CommonUtil.ReturnListToPool(leadershipCharacterUIs, item => item.gameObject);

        scrollableCharacterContainer.ClearAllItems();

        for (int i = 0; i < leadershipData.leadershipCharacterDataItems.Count; i++)
        {
            LeadershipCharacterData leadershipCharacterData = leadershipData.leadershipCharacterDataItems[i];

            LoadLeadershipCharacter(leadershipCharacterData);
        }

        UpdateNumCandidates();
    }

    private void LoadLeadershipCharacter(LeadershipCharacterData leadershipCharacterData)
    {
        LeadershipCharacterUI leadershipCharacterUI =
                ObjectPoolingEverything.GetFromPool<LeadershipCharacterUI>(GameConstants.leadershipCharacterUI);

        leadershipCharacterUI.Init(leadershipCharacterData);

        // Candidate
        if (leadershipCharacterData.leadershipPosition == LeadershipPosition.None)
        {
            // CommonUtil.SetParentAndResetScale(leadershipCharacterUI.transform, characterContainer, Vector3.zero);
            scrollableCharacterContainer.AddItem(leadershipCharacterUI.gameObject, targetScale: 0.48f * Vector3.one);
            leadershipCharacterUI.transform.localScale *= 0.48f;
        }

        // Assign Position
        if (leadershipCharacterData.leadershipPosition != LeadershipPosition.None)
        {
            for (int i = 0; i < leadershipPositionUIs.Count; i++)
            {
                LeadershipPositionUI leadershipPositionUI = leadershipPositionUIs[i];

                if (leadershipPositionUI.leadershipPosition == leadershipCharacterData.leadershipPosition)
                {
                    leadershipPositionUI.AssignCharacter(leadershipCharacterUI);

                    if (leadershipPositionUI.leadershipPosition == LeadershipPosition.Captain)
                    {
                        currentCaptain = leadershipCharacterUI;
                    }

                    break;
                }
            }
        }

        leadershipCharacterUIs.Add(leadershipCharacterUI);
    }

    public void HireNewCharacters()
    {
        if (ResourceManager.Ins.GetResourceValue(ResourceType.Coin) < hireNewCharactersCost)
        {
            IngameNotificationManager.Ins.ShowNotification(GameConstants.WARNING, "Not enough power to hire new characters!");

            return;
        }

        ResourceManager.Ins.DeductResource(ResourceType.Coin, hireNewCharactersCost);

        int numCharacterToHire = 3;

        int currentCharacter = GetNumCandidates();

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

        UpdateNumCandidates();

        SaveLeadershipData();
    }

    public void SelectLeadershipPosition(LeadershipPositionUI leadershipPositionUI)
    {
        appointCharacterOverlayCanvasGroup.interactable = false;

        SaferioTween.AlphaShow(appointCharacterOverlayCanvasGroup, onCompleteAction: () =>
        {
            appointCharacterOverlayCanvasGroup.interactable = true;
        });

        selectedLeadershipPosition = leadershipPositionUI;
    }

    public void CancelSelectLeadershipPosition()
    {
        appointCharacterOverlayCanvasGroup.interactable = false;

        SaferioTween.AlphaHide(appointCharacterOverlayCanvasGroup);

        selectedLeadershipPosition = null;
    }

    public void AssignLeadershipPosition(LeadershipCharacterUI leadershipCharacterUI)
    {
        selectedLeadershipPosition.AssignCharacter(leadershipCharacterUI);

        CalculateBonusStat();

        CancelSelectLeadershipPosition();
    }

    public void UpdateNumCandidates()
    {
        candidatesCountText.text = $"{GetNumCandidates()}/{maxCharacter}";
    }

    private void OnScreenSwitch(SwitchableScreenType switchableScreenType)
    {
        if (switchableScreenType == this.switchableScreenType)
        {
            Show();
        }
        else
        {
            Hide();
        }
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
        else
        {
            for (int i = 0; i < statModifiers.Count; i++)
            {
                StatModifier statModifier = statModifiers[i];

                statModifier.Value = 0;
            }
        }

        Dictionary<LeadershipPosition, LeadershipPositionUI> leadershipPositionDictionary = new Dictionary<LeadershipPosition, LeadershipPositionUI>();

        for (int i = 0; i < leadershipPositionUIs.Count; i++)
        {
            LeadershipPositionUI leadershipPositionUI = leadershipPositionUIs[i];

            leadershipPositionDictionary.Add(leadershipPositionUI.leadershipPosition, leadershipPositionUI);
        }

        for (int i = 0; i < leadershipData.leadershipCharacterDataItems.Count; i++)
        {
            LeadershipCharacterData leadershipCharacterData = leadershipData.leadershipCharacterDataItems[i];

            if (leadershipCharacterData.leadershipPosition != LeadershipPosition.None)
            {
                LeadershipPositionUI leadershipPositionUI = leadershipPositionDictionary[leadershipCharacterData.leadershipPosition];

                float efficiency = leadershipPositionUI.CalculateEfficiency(leadershipCharacterData);

                for (int j = 0; j < leadershipPositionUI.statModifierDataItems.Count; j++)
                {
                    StatModifierData statModifierData = leadershipPositionUI.statModifierDataItems[j];

                    for (int k = 0; k < statModifiers.Count; k++)
                    {
                        if (statModifiers[k].Type == statModifierData.statType)
                        {
                            statModifiers[k].statModifierSource = StatModifierSource.LeadershipPosition;
                            statModifiers[k].Value += efficiency * statModifierData.value;

                            break;
                        }
                    }
                }
            }
        }

        // Apply Stats to Nodes
        for (int i = 0; i < NodeManager.Ins.nodes.Count; i++)
        {
            BaseCharacter node = NodeManager.Ins.nodes[i];

            node.serviceLocator.stats.RemoveAllModifiersFromSource(StatModifierSource.LeadershipPosition);

            for (int j = 0; j < statModifiers.Count; j++)
            {
                statModifiers[j].statModifierSource = StatModifierSource.LeadershipPosition;

                node.serviceLocator.stats.AddModifier(statModifiers[j]);
            }
        }

        UpdateTotalBonusText();
    }

    public void UpdateTotalBonusText()
    {
        StringBuilder statDescription = new StringBuilder();

        foreach (var mod in statModifiers)
        {
            if (mod.Value == 0)
            {
                continue;
            }

            string hex = statColorConfigSO.GetHex(mod.Type);

            bool isPositive = mod.Value >= 0;

            string sign = isPositive ? "+" : "";

            string valStr = mod.ModType == ModifierType.Flat
                ? $"{sign}{mod.Value:F2}"
                : $"{sign}{mod.Value * 100f:F2}%";

            statDescription.AppendLine($"<color=#{hex}>{valStr} {mod.Type}</color>");
        }

        totalEffectText.text = statDescription.ToString();
    }
    #endregion

    #region Character Trait
    public void ShowTraitDetail(CharacterTraitItemUI characterTraitItemUI)
    {
        characterTrailDetail.transform.position = characterTraitItemUI.transform.position.AddX(400);

        characterTrailDetail.ShowAt(characterTraitItemUI);
    }

    public void HideTraitDetail()
    {
        characterTrailDetail.Hide();
    }
    #endregion

    #region Election
    public void ShowElection()
    {
        electionPopup.Show();
    }

    public void ForceStartElection()
    {
        if (currentCaptain == null)
        {
            IdleTimeManager.Ins.StartElectionImmediately();
        }
        else
        {
            if (ResourceManager.Ins.GetResourceValue(ResourceType.Coin) >= forceElectionCost)
            {
                IdleTimeManager.Ins.StartElectionImmediately();
            }
            else
            {
                IngameNotificationManager.Ins.ShowNotification(
                    GameConstants.WARNING, $"Can't start election now. {currentCaptain.leadershipCharacterData.characterName} is the current Captain");
            }
        }
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

    public void SaveLeadershipData()
    {
        DataUtility.Save(SaveSlotManager.Ins.GetSaveKey("leadershipData"), leadershipData);
    }

    private LeadershipData GetDefaultLeadershipData()
    {
        LeadershipData defaultLeadershipData = new LeadershipData();

        defaultLeadershipData.isScoutAnyCharacter = false;
        defaultLeadershipData.leadershipCharacterDataItems = new List<LeadershipCharacterData>();
        defaultLeadershipData.leadershipPeriodDataItems = new List<LeadershipPeriodData>();

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
        Country country = TopCountries.GetRandomTop50Country();

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
        leadershipCharacterData.birthDayString = birthday.ToString("dd/MM/yyyy");
        leadershipCharacterData.traitIndexes = characterTraitIndexes;
        leadershipCharacterData.faceSeed = UnityEngine.Random.Range(1000, 999999);
        leadershipCharacterData.electionPoint = UnityEngine.Random.Range(10, 100);

        return leadershipCharacterData;
    }
    #endregion

    #region Remove
    private void EnterRemoveCharacterMode()
    {
        removeCharacterOverlayCanvasGroup.interactable = false;

        SaferioTween.AlphaShow(removeCharacterOverlayCanvasGroup, onCompleteAction: () =>
        {
            removeCharacterOverlayCanvasGroup.interactable = true;
        });

        IdleGameInputManager.inputMode = IdleGameInputMode.RemoveLeadershipCharacter;
    }

    private void ExitRemoveCharacterMode()
    {
        removeCharacterOverlayCanvasGroup.interactable = false;

        SaferioTween.AlphaHide(removeCharacterOverlayCanvasGroup);

        IdleGameInputManager.inputMode = IdleGameInputMode.Normal;
    }

    private void ConfirmRemoveCharacters()
    {
        List<LeadershipCharacterUI> toRemoveList = new List<LeadershipCharacterUI>();

        for (int i = 0; i < leadershipCharacterUIs.Count; i++)
        {
            LeadershipCharacterUI leadershipCharacterUI = leadershipCharacterUIs[i];

            if (leadershipCharacterUI.isMarkForRemove)
            {
                toRemoveList.Add(leadershipCharacterUI);
            }
        }

        for (int i = 0; i < toRemoveList.Count; i++)
        {
            leadershipData.leadershipCharacterDataItems.Remove(toRemoveList[i].leadershipCharacterData);
        }

        CommonUtil.ReturnListToPool(toRemoveList, item => item.gameObject);

        ExitRemoveCharacterMode();

        SaveLeadershipData();
    }

    public void RemoveSingleCharacter(LeadershipCharacterUI leadershipCharacterUI)
    {
        leadershipData.leadershipCharacterDataItems.Remove(leadershipCharacterUI.leadershipCharacterData);

        ObjectPoolingEverything.ReturnToPool(GameConstants.leadershipCharacterUI, leadershipCharacterUI.gameObject);

        leadershipCharacterUIs.Remove(leadershipCharacterUI);

        SaveLeadershipData();
    }
    #endregion

    #region Leadership Period
    public void OpenLeadershipPeriodScreen()
    {
        openLeadershipPeriodScreenEvent?.Invoke();
    }

    public void AddLeadershipPeriod(LeadershipCharacterData leadershipCharacterData, DateTime startDateTime)
    {
        TryEndCurrentLeadershipPeriod();

        LeadershipPeriodData leadershipPeriodData = new LeadershipPeriodData();

        leadershipPeriodData.leader = leadershipCharacterData;
        leadershipPeriodData.startDateTimeString = startDateTime.ToString("dd/MM/yyyy");
        leadershipPeriodData.starDiscoveryEventDataItems = new List<StarDiscoveryEventDataItem>();

        leadershipData.leadershipPeriodDataItems.Add(leadershipPeriodData);

        SaveLeadershipData();
    }

    public void TryEndCurrentLeadershipPeriod()
    {
        for (int i = 0; i < leadershipData.leadershipPeriodDataItems.Count; i++)
        {
            LeadershipPeriodData leadershipPeriodData = leadershipData.leadershipPeriodDataItems[i];

            if (leadershipPeriodData.leader == currentCaptain.leadershipCharacterData)
            {
                leadershipPeriodData.endDateTimeString = IdleTimeManager.Ins.CurrentDateTime.ToString("dd/MM/yyyy");
            }
        }

        SaveLeadershipData();
    }

    public void AddStarDiscoveryEventLeadershipPeriod(StarDiscoveryEventDataItem starDiscoveryEventDataItem)
    {
        if (leadershipData.leadershipCharacterDataItems.Count == 0)
        {
            return;
        }

        LeadershipPeriodData currentLeadershipPeriodData = leadershipData.leadershipPeriodDataItems.Last();

        currentLeadershipPeriodData.starDiscoveryEventDataItems.Add(starDiscoveryEventDataItem);

        SaveLeadershipData();
    }
    #endregion

    #region Util
    public int GetNumCandidates()
    {
        int currentCharacter = 0;

        for (int i = 0; i < leadershipData.leadershipCharacterDataItems.Count; i++)
        {
            LeadershipCharacterData leadershipCharacterData = leadershipData.leadershipCharacterDataItems[i];

            if (leadershipCharacterData.leadershipPosition == LeadershipPosition.None)
            {
                currentCharacter++;
            }
        }

        return currentCharacter;
    }
    #endregion
}

[Serializable]
public class LeadershipData
{
    public bool isScoutAnyCharacter;
    public List<LeadershipCharacterData> leadershipCharacterDataItems;
    public List<LeadershipPeriodData> leadershipPeriodDataItems;
}

[Serializable]
public class LeadershipCharacterData
{
    public string characterName;
    public Country country;
    public string birthDayString;
    public List<int> traitIndexes;
    public int faceSeed;
    public LeadershipPosition leadershipPosition;
    public int electionPoint;
}

[Serializable]
public class LeadershipPeriodData
{
    public LeadershipCharacterData leader;
    public string startDateTimeString;
    public string endDateTimeString;
    public List<StarDiscoveryEventDataItem> starDiscoveryEventDataItems;
}
