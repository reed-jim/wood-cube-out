using System.Collections.Generic;
using System.Linq;
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
    public Button supportButton;
    public TMP_Text supportCostText;
    public TMP_Text electionPointText;

    public int electionPoint;
    public float supportCost;

    public StatColorConfigSO config;
    public CountryFlagDatabase countryFlagDatabase;

    private List<CharacterTrait> characterTraits;

    void Awake()
    {
        selectButton.onClick.AddListener(Select);
        supportButton.onClick.AddListener(Support);
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

        supportButton.gameObject.SetActive(false);

        this.characterTraits = characterTraits;
    }

    private void Select()
    {
        LeadershipManager.Ins.AssignLeadershipPosition(this);
    }

    public List<CharacterTrait> GetCharacterTraits()
    {
        return characterTraits;
    }

    public void EnableSupportOption()
    {
        supportButton.gameObject.SetActive(true);

        supportCost = 10 * characterTraits.Count(item => item.isGoodTrait);

        electionPointText.text = $"Election Point: {electionPoint}";
        supportCostText.text = $"{supportCost}";
    }

    private void Support()
    {
        if (ResourceManager.Ins.GetResourceValue(ResourceType.Coin) >= supportCost)
        {

        }
        else
        {
            IngameNotificationManager.Ins.ShowNotification(GameConstants.WARNING, "Not enough power to support this character!");
        }

        electionPoint += 10;

        electionPointText.text = $"Election Point: {electionPoint}";
    }
}
