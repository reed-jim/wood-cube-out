using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeadershipCharacterUI : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text ageText;
    public Image flag;
    public Button selectButton;
    public RectTransform traitContainer;
    public Button supportButton;
    public TMP_Text supportCostText;
    public TMP_Text electionPointText;
    public ProceduralHumanAvatarUI proceduralHumanAvatarUI;
    public TMP_Text leadershipPositionText;
    public TMP_Text levelText;

    [Header("Theme")]
    public ThemeableUI borderThemable;

    [Header("State")]
    public int electionPoint;
    public float supportCost;
    public bool isMarkForRemove;

    [Header("SO")]
    public CharacterTraitContainer characterTraitContainer;
    public CountryFlagDatabase countryFlagDatabase;

    private DateTime birthDay;
    private List<CharacterTrait> characterTraits;
    public LeadershipCharacterData leadershipCharacterData;

    public static event Action<LeadershipCharacterData, List<CharacterTrait>> showLeadershipCharacterDetailEvent;

    void Awake()
    {
        IdleTimeManager.OnYearPassed += OnYearPassed;

        selectButton.onClick.AddListener(Select);
        supportButton.onClick.AddListener(Support);
    }

    void OnDestroy()
    {
        IdleTimeManager.OnYearPassed -= OnYearPassed;
    }

    public void Init(LeadershipCharacterData data)
    {
        leadershipCharacterData = data;

        nameText.text = data.characterName;

        System.DateTime birthDay = System.DateTime.ParseExact(
            data.birthDayString,
            "dd/MM/yyyy",
            System.Globalization.CultureInfo.InvariantCulture
        );
        int age = CommonUtil.GetAge(birthDay, IdleTimeManager.Ins.CurrentDateTime);

        List<CharacterTrait> characterTraits = new List<CharacterTrait>();

        for (int j = 0; j < characterTraitContainer.characterTraits.Count; j++)
        {
            CharacterTrait characterTrait = characterTraitContainer.characterTraits[j];

            if (leadershipCharacterData.traitIndexes.Contains(characterTrait.id))
            {
                characterTraits.Add(characterTrait);
            }
        }

        ageText.text = $"{age} years old";
        proceduralHumanAvatarUI.age = age / 60f;
        flag.sprite = countryFlagDatabase.GetFlag(data.country);

        if (leadershipCharacterData.leadershipPosition == LeadershipPosition.None)
        {
            leadershipPositionText.text = "No position taken";
        }
        else
        {
            leadershipPositionText.text = $"{leadershipCharacterData.leadershipPosition}";
        }

        proceduralHumanAvatarUI.GenerateNewRandomCharacterWithSeed(data.faceSeed);

        for (int i = 0; i < characterTraits.Count; i++)
        {
            CharacterTraitItemUI item = ObjectPoolingEverything.GetFromPool<CharacterTraitItemUI>(GameConstants.characterTraitItemUI);
            CommonUtil.SetParentAndResetScale(item.transform, traitContainer);
            item.Init(characterTraits[i]);
        }

        supportButton.gameObject.SetActive(false);
        electionPointText.gameObject.SetActive(false);

        this.birthDay = birthDay;
        this.characterTraits = characterTraits;
    }

    private void Select()
    {
        if (IdleGameInputManager.inputMode == IdleGameInputMode.RemoveLeadershipCharacter)
        {
            if (isMarkForRemove)
            {
                borderThemable.ChangeRole(ThemeableUI.ElementRole.Outline);
            }
            else
            {
                borderThemable.ChangeRole(ThemeableUI.ElementRole.Error);
            }

            isMarkForRemove = !isMarkForRemove;
        }
        else if (LeadershipManager.Ins.selectedLeadershipPosition == null)
        {
            showLeadershipCharacterDetailEvent?.Invoke(leadershipCharacterData, characterTraits);
        }
        else
        {
            LeadershipManager.Ins.AssignLeadershipPosition(this);
        }
    }

    public List<CharacterTrait> GetCharacterTraits() => characterTraits;

    public void EnableSupportOption()
    {
        supportButton.gameObject.SetActive(true);
        electionPointText.gameObject.SetActive(true);

        supportCost =
            CurveManager.Ins.GetHireCharactersCost(StarManager.Ins.currentStarPathConfig.id) * (1 + 0.1f * characterTraits.Count(item => item.isGoodTrait));

        electionPointText.text = $"Election Points: {electionPoint}";
        supportCostText.text = $"{supportCost}";
    }

    public void DisableSupportOption()
    {
        supportButton.gameObject.SetActive(false);
        electionPointText.gameObject.SetActive(false);
    }

    private void Support()
    {
        if (ResourceManager.Ins.GetResourceValue(ResourceType.Coin) >= supportCost)
        {
            ResourceManager.Ins.DeductResource(ResourceType.Coin, supportCost);
            electionPoint += 10;
            electionPointText.text = $"Election Points: {electionPoint}";
        }
        else
        {
            IngameNotificationManager.Ins.ShowNotification(GameConstants.WARNING, "Not enough resources to support this character!");
        }
    }

    private void OnYearPassed()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

        int age = CommonUtil.GetAge(birthDay, IdleTimeManager.Ins.CurrentDateTime);

        ageText.text = $"{age} years old";
        proceduralHumanAvatarUI.age = age / 60f;

        if (age > 60)
        {
            float deathChance = age / 500f;

            if (UnityEngine.Random.value < deathChance)
            {
                Die();
            }
        }
    }

    #region Old Age
    private void Die()
    {
        IngameEventManager.Ins.CreateCharacterDeathEvent(leadershipCharacterData);
        LeadershipManager.Ins.RemoveSingleCharacter(this);
    }
    #endregion
}
