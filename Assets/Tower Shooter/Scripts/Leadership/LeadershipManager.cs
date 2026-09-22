using System;
using System.Collections.Generic;
using System.Globalization;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public enum LeadershipPosition
{
    Captain,
    Chieftain
}

public class LeadershipManager : Singleton<LeadershipManager>
{
    public RectTransform container;
    public RectTransform characterContainer;
    public Button closeButton;

    public CharacterTrailDetail characterTrailDetail;

    public LeadershipPositionUI selectedLeadershipPosition;

    public CharacterTraitContainer characterTraitContainer;

    private LeadershipData leadershipData;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        MultiverseIdleGameplayScreen.openLeadershipUIEvent += Show;

        closeButton.onClick.AddListener(Hide);

        container.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        MultiverseIdleGameplayScreen.openLeadershipUIEvent -= Show;
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

            leadershipCharacterUI.Init(name, country, age, characterTraits);

            CommonUtil.SetParentAndResetScale(leadershipCharacterUI.transform, characterContainer, Vector3.zero);
            leadershipCharacterUI.transform.localScale *= 0.48f;
        }
    }

    public void SelectLeadershipPosition(LeadershipPositionUI leadershipPositionUI)
    {
        selectedLeadershipPosition = leadershipPositionUI;
    }

    public void AssignLeadershipPosition(LeadershipCharacterUI leadershipCharacterUI)
    {
        selectedLeadershipPosition.AssignCharacter(leadershipCharacterUI);
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

    #region Character Trait
    public void ShowTraitDetail(CharacterTraitItemUI characterTraitItemUI)
    {
        CommonUtil.SetParentAndResetScale(characterTrailDetail.transform, characterTraitItemUI.transform);

        characterTrailDetail.ShowAt(characterTraitItemUI);
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
            Country country = CommonUtil.GetRandomEnumValue<Country>();

            string name = CountryNameGenerator.Ins.GetRandomName(country);

            DateTime birthday = CommonUtil.RandomBirthday(2026 - 18, 2026 - 18 + 50);

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

            defaultLeadershipData.leadershipCharacterDataItems.Add(leadershipCharacterData);
        }

        return defaultLeadershipData;
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
}
