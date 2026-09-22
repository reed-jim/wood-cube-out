using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeadershipCharacterDetailUI : BasePopup
{
    public TMP_Text nameText;
    public TMP_Text ageText;
    public Image flag;
    public RectTransform traitContainer;
    public ProceduralHumanAvatarUI proceduralHumanAvatarUI;

    public CountryFlagDatabase countryFlagDatabase;

    private List<CharacterTrait> characterTraits;
    public LeadershipCharacterData CharacterData { get; private set; }

    private List<CharacterTraitItemUI> characterTraitItemUIs = new List<CharacterTraitItemUI>();

    protected override void RegisterMoreEvent()
    {
        base.RegisterMoreEvent();

        LeadershipCharacterUI.showLeadershipCharacterDetailEvent += InitAndShow;
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
        CharacterData = data;
        this.characterTraits = traits;

        nameText.text = data.characterName;

        System.DateTime birthDay = System.DateTime.ParseExact(
            data.birthDayString,
            "yyyy-MM-dd",
            System.Globalization.CultureInfo.InvariantCulture
        );
        int age = CommonUtil.GetAge(birthDay, IdleTimeManager.Ins.CurrentDateTime);

        ageText.text = $"{age} years old";
        proceduralHumanAvatarUI.age = age / 60f;
        flag.sprite = countryFlagDatabase.GetFlag(data.country);

        CommonUtil.ReturnListToPool(characterTraitItemUIs, item => item.gameObject);

        for (int i = 0; i < traits.Count; i++)
        {
            CharacterTraitItemUI item = ObjectPoolingEverything.GetFromPool<CharacterTraitItemUI>(GameConstants.characterTraitItemUI);

            CommonUtil.SetParentAndResetScale(item.transform, traitContainer);
            item.Init(traits[i]);

            characterTraitItemUIs.Add(item);
        }
    }
}
