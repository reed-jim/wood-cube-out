using System;
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
    public TMP_Text chanceGainElectionPointText;
    public Image flag;
    public Button removeFromLeaderPositionButton;
    public RectTransform traitContainer;
    public ProceduralHumanAvatarUI proceduralHumanAvatarUI;

    [Header("Exp")]
    public Image expFill;
    public TMP_Text expText;

    public CountryFlagDatabase countryFlagDatabase;

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

    private void InitAndShow(LeadershipCharacterData data)
    {
        Show();

        Init(data);
    }

    public void Init(LeadershipCharacterData data)
    {
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

        float exp = data.currentExp;
        float expRequiredForNextLevel = CurveManager.Ins.GetBuyAugmentCost((int)data.rank);
        float expProgress = exp / expRequiredForNextLevel;

        expFill.fillAmount = expProgress;
        expText.text = $"{(int)(expRequiredForNextLevel - exp)}exp required to Level {(int)data.rank + 2}";

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
