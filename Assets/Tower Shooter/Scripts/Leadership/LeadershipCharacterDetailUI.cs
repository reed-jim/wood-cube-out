using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeadershipCharacterDetailUI : BasePopup
{
    public TMP_Text nameText;
    public TMP_Text ageText;
    public TMP_Text leadershipPositionText;
    public Image flag;
    public Button removeFromLeaderPositionButton;
    public RectTransform traitContainer;
    public ProceduralHumanAvatarUI proceduralHumanAvatarUI;

    public CountryFlagDatabase countryFlagDatabase;

    private List<CharacterTrait> characterTraits;
    private LeadershipCharacterData leadershipCharacterData;

    private List<CharacterTraitItemUI> characterTraitItemUIs = new List<CharacterTraitItemUI>();

    protected override void RegisterMoreEvent()
    {
        base.RegisterMoreEvent();

        LeadershipCharacterUI.showLeadershipCharacterDetailEvent += InitAndShow;

        removeFromLeaderPositionButton.onClick.AddListener(RemoveFromLeaderPosition);
    }

    protected override void UnregisterMoreEvent()
    {
        base.UnregisterMoreEvent();

        LeadershipCharacterUI.showLeadershipCharacterDetailEvent -= InitAndShow;
    }

    private void InitAndShow(LeadershipCharacterData data, List<CharacterTrait> traits)
    {
        Show();

        Init(data, traits);
    }

    public void Init(LeadershipCharacterData data, List<CharacterTrait> traits)
    {
        this.characterTraits = traits;
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

        if (data.leadershipPosition == LeadershipPosition.None)
        {
            leadershipPositionText.text = "No position taken";
            removeFromLeaderPositionButton.gameObject.SetActive(false);
        }
        else
        {
            leadershipPositionText.text = $"{data.leadershipPosition}";
            removeFromLeaderPositionButton.gameObject.SetActive(true);
        }

        CommonUtil.ReturnListToPool(characterTraitItemUIs, item => item.gameObject);

        for (int i = 0; i < traits.Count; i++)
        {
            CharacterTraitItemUI item = ObjectPoolingEverything.GetFromPool<CharacterTraitItemUI>(GameConstants.characterTraitItemUI);

            CommonUtil.SetParentAndResetScale(item.transform, traitContainer);
            item.Init(traits[i]);

            characterTraitItemUIs.Add(item);
        }
    }

    private void RemoveFromLeaderPosition()
    {
        leadershipCharacterData.leadershipPosition = LeadershipPosition.None;

        Init(leadershipCharacterData, characterTraits);

        LeadershipManager.Ins.SaveLeadershipData();
        LeadershipManager.Ins.Refresh();

        Hide();
    }
}
