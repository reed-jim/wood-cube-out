using System;
using System.Collections.Generic;
using System.Globalization;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class ElectionPopup : MonoBehaviour
{
    public RectTransform container;
    public RectTransform characterContainer;
    public Button closeButton;

    public List<LeadershipCharacterUI> leadershipCharacterUIs;

    public CharacterTraitContainer characterTraitContainer;

    void Awake()
    {
        closeButton.onClick.AddListener(Hide);
    }

    private void Init()
    {
        ShowAllCharacters();
    }

    private void ShowAllCharacters()
    {
        for (int i = 0; i < LeadershipManager.Ins.leadershipData.leadershipCharacterDataItems.Count; i++)
        {
            LeadershipCharacterData leadershipCharacterData = LeadershipManager.Ins.leadershipData.leadershipCharacterDataItems[i];

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

        leadershipCharacterUI.Init(name, country, age, characterTraits);
        leadershipCharacterUI.EnableSupportOption();

        CommonUtil.SetParentAndResetScale(leadershipCharacterUI.transform, characterContainer, Vector3.zero);
        leadershipCharacterUI.transform.localScale *= 0.48f;
    }

    public void Show()
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
}
