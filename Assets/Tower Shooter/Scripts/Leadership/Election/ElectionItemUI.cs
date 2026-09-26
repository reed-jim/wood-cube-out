using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElectionItemUI : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text ageText;
    public Image flag;
    public Button selectButton;
    public RectTransform traitContainer;
    public ProceduralHumanAvatarUI proceduralHumanAvatarUI;
    public TMP_Text leadershipPositionText;
    public TMP_Text levelText;

    [Header("Election")]
    public Button supportButton;
    public TMP_Text supportCostText;
    public TMP_Text electionPointText;
    public TMP_Text electionRank;
    public ThemeableUI electionRankThemeable;

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

    public static event Action<LeadershipCharacterData> showLeadershipCharacterDetailEvent;
    public static event Action selectLeadershipCharacterUIEvent;

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

        List<CharacterTrait> characterTraits = LeadershipManager.Ins.GetCharacterTraits(data.traitIndexes);
        ageText.text = $"{age} years old";
        proceduralHumanAvatarUI.age = age / 100f;
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
        electionRank.gameObject.SetActive(false);

        this.birthDay = birthDay;
        this.characterTraits = characterTraits;
        this.electionPoint = data.electionPoint;
    }

    private void Select()
    {
        ShowLeadershipCharacterDetail();
    }

    public void ShowLeadershipCharacterDetail()
    {
        showLeadershipCharacterDetailEvent?.Invoke(leadershipCharacterData);
    }

    #region Election
    public void EnableSupportOption()
    {
        supportButton.gameObject.SetActive(true);
        electionPointText.gameObject.SetActive(true);
        electionRank.gameObject.SetActive(true);

        supportCost =
            CurveManager.Ins.GetHireCharactersCost(StarManager.Ins.currentStarPathConfig.id) * (1 + 0.1f * characterTraits.Count(item => item.isGoodTrait));

        electionPointText.text = $"{electionPoint} votes";
        supportCostText.text = $"{supportCost}";
    }

    public void DisableSupportOption()
    {
        supportButton.gameObject.SetActive(false);
        electionPointText.gameObject.SetActive(false);
        electionRank.gameObject.SetActive(false);
    }

    public void SetElectionRank(int rank)
    {
        if (leadershipCharacterData.leadershipPosition == LeadershipPosition.Captain)
        {
            electionRank.text = $"{CommonUtil.GetOrdinal(rank)} - Reigning {leadershipCharacterData.leadershipPosition}";
        }
        else
        {
            electionRank.text = $"{CommonUtil.GetOrdinal(rank)}";
        }

        if (rank == 1)
        {
            electionRankThemeable.ChangeRole(ThemeableUI.ElementRole.Primary);
        }
        else
        {
            electionRankThemeable.ChangeRole(ThemeableUI.ElementRole.OnSurface);
        }
    }

    private void Support()
    {
        if (ResourceManager.Ins.GetResourceValue(ResourceType.Coin) >= supportCost)
        {
            ResourceManager.Ins.DeductResource(ResourceType.Coin, supportCost);
            electionPoint += 10;
            electionPointText.text = $"Election Points: {electionPoint}";

            leadershipCharacterData.electionPoint = electionPoint;

            LeadershipManager.Ins.SaveLeadershipData();
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
        proceduralHumanAvatarUI.age = age / 100f;

        if (age > 60)
        {
            float deathChance = age / 500f;

            if (UnityEngine.Random.value < deathChance)
            {
                Die();
            }
        }
    }
    #endregion

    #region Remove
    public void MarkForRemove(bool isMarkForRemove)
    {
        if (isMarkForRemove)
        {
            borderThemable.ChangeRole(ThemeableUI.ElementRole.Error);
        }
        else
        {
            borderThemable.ChangeRole(ThemeableUI.ElementRole.Outline);
        }

        this.isMarkForRemove = isMarkForRemove;
    }
    #endregion

    #region Old Age
    private void Die()
    {
        IngameEventManager.Ins.CreateCharacterDeathEvent(leadershipCharacterData);
        // LeadershipManager.Ins.RemoveSingleCharacter(this);
    }
    #endregion
}
