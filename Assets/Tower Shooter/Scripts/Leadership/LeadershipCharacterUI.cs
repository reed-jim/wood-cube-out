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

    public int electionPoint;
    public float supportCost;

    public StatColorConfigSO config;
    public CountryFlagDatabase countryFlagDatabase;

    private List<CharacterTrait> characterTraits;
    public LeadershipCharacterData CharacterData { get; private set; }

    void Awake()
    {
        selectButton.onClick.AddListener(Select);
        supportButton.onClick.AddListener(Support);
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

        for (int i = 0; i < traits.Count; i++)
        {
            CharacterTraitItemUI item = ObjectPoolingEverything.GetFromPool<CharacterTraitItemUI>(GameConstants.characterTraitItemUI);
            CommonUtil.SetParentAndResetScale(item.transform, traitContainer);
            item.Init(traits[i]);
        }

        supportButton.gameObject.SetActive(false);
        electionPointText.gameObject.SetActive(false);
    }

    private void Select()
    {
        LeadershipManager.Ins.AssignLeadershipPosition(this);
    }

    public List<CharacterTrait> GetCharacterTraits() => characterTraits;

    public void EnableSupportOption()
    {
        supportButton.gameObject.SetActive(true);
        electionPointText.gameObject.SetActive(true);

        supportCost = Mathf.Max(10, 10 * characterTraits.Count(item => item.isGoodTrait));

        electionPointText.text = $"Election Points: {electionPoint}";
        supportCostText.text = $"{supportCost}";
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
}
