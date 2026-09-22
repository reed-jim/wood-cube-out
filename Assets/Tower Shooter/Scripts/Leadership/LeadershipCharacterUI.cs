using System.Collections.Generic;
using System.Text;
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

    public StatColorConfigSO config;
    public CountryFlagDatabase countryFlagDatabase;

    void Awake()
    {
        selectButton.onClick.AddListener(Select);
    }

    public void Init(string name, Country country, int age, List<CharacterTrait> characterTraits)
    {
        nameText.text = name;
        ageText.text = $"{age} years old";
        flag.sprite = countryFlagDatabase.GetFlag(country);

        for (int i = 0; i < characterTraits.Count; i++)
        {
            CharacterTrait characterTrait = characterTraits[i];
            CharacterTraitItemUI characterTraitItemUI = ObjectPoolingEverything.GetFromPool<CharacterTraitItemUI>(GameConstants.characterTraitItemUI);

            CommonUtil.SetParentAndResetScale(characterTraitItemUI.transform, traitContainer);
            characterTraitItemUI.Init(characterTrait);
        }
    }

    private void Select()
    {
        LeadershipManager.Ins.AssignLeadershipPosition(this);
    }
}
