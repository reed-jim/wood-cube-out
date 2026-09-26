using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeadershipPositionUI : MonoBehaviour
{
    public RectTransform container;
    public RectTransform characterUIContainer;
    public TMP_Text positionNameText;
    public TMP_Text description;
    public TMP_Text efficiencyText;
    public Button selectButton;

    public ThemeableUI borderThemeable;
    public ThemeableUI titleThemeable;

    [Header("Customize")]
    public LeadershipPosition leadershipPosition;
    public bool isBuffAllStats;
    public List<StatModifierData> statModifierDataItems;
    public float baseEfficiency;

    public LeadershipCharacterUI leadershipCharacterUI;
    public float efficiency;

    [Header("SO")]
    public CharacterTraitContainer characterTraitContainer;
    public StatColorConfigSO config;

    void Awake()
    {
        selectButton.onClick.AddListener(Select);
    }

    void Start()
    {
        Init();
    }

    private void Init()
    {
        StringBuilder statDescription = new StringBuilder();

        foreach (var mod in statModifierDataItems)
        {
            string hex = config.GetHex(mod.statType);

            bool isPositive = mod.value >= 0;

            string sign = isPositive ? "+" : "";

            string valStr = mod.modType == ModifierType.Flat
                ? $"{sign}{mod.value:F2}"
                : $"{sign}{mod.value * 100f:F2}%";

            statDescription.AppendLine($"<color=#{hex}>{valStr} {mod.statType}</color>");
        }

        positionNameText.text = $"{leadershipPosition}";
        description.text = statDescription.ToString().TrimEnd();

        if (leadershipCharacterUI == null)
        {
            efficiencyText.color = DynamicThemeManager.Ins.CurrentTheme.onSurface;
            efficiencyText.text = $"Vacant";

            borderThemeable.ChangeRole(ThemeableUI.ElementRole.Outline);
            titleThemeable.ChangeRole(ThemeableUI.ElementRole.OnSurface);
        }
        else
        {
            efficiency = CalculateEfficiency(leadershipCharacterUI.leadershipCharacterData);

            efficiencyText.color = DynamicThemeManager.Ins.CurrentTheme.primary;
            efficiencyText.text = $"Efficiency: {(int)(efficiency * 100)}%";

            borderThemeable.ChangeRole(ThemeableUI.ElementRole.Primary);
            titleThemeable.ChangeRole(ThemeableUI.ElementRole.Primary);
        }
    }

    private void Select()
    {
        if (leadershipCharacterUI == null)
        {
            if (leadershipPosition == LeadershipPosition.Captain)
            {
                LeadershipManager.Ins.ShowElection();
            }
            else
            {
                LeadershipManager.Ins.SelectLeadershipPosition(this);
            }
        }
        else
        {
            leadershipCharacterUI.ShowLeadershipCharacterDetail();
        }
    }

    public void AssignCharacter(LeadershipCharacterUI leadershipCharacterUI)
    {
        LeadershipManager.Ins.RemoveItemFromScrollableLayoutGroup(leadershipCharacterUI);

        CommonUtil.SetParentAndResetScale(leadershipCharacterUI.transform, characterUIContainer, Vector3.zero);
        CommonUtil.ResetRectTransform(leadershipCharacterUI.GetComponent<RectTransform>());

        efficiency = CalculateEfficiency(leadershipCharacterUI.leadershipCharacterData);

        efficiencyText.color = DynamicThemeManager.Ins.CurrentTheme.primary;
        efficiencyText.text = $"Efficiency: {(int)(efficiency * 100)}%";

        borderThemeable.ChangeRole(ThemeableUI.ElementRole.Primary);
        titleThemeable.ChangeRole(ThemeableUI.ElementRole.Primary);

        if (leadershipCharacterUI.leadershipCharacterData.leadershipPosition != leadershipPosition)
        {
            leadershipCharacterUI.leadershipCharacterData.leadershipPosition = leadershipPosition;
            LeadershipManager.Ins.SaveLeadershipData();
        }

        this.leadershipCharacterUI = leadershipCharacterUI;
    }

    public void RemoveCharacter()
    {
        leadershipCharacterUI = null;

        Init();
    }

    public float CalculateEfficiency(LeadershipCharacterData leadershipCharacterData)
    {
        float efficiency = baseEfficiency;

        List<CharacterTrait> characterTraits = new List<CharacterTrait>();

        for (int i = 0; i < leadershipCharacterData.traitIndexes.Count; i++)
        {
            int traitIndex = leadershipCharacterData.traitIndexes[i];

            characterTraits.Add(characterTraitContainer.characterTraits[traitIndex]);
        }

        for (int i = 0; i < characterTraits.Count; i++)
        {
            CharacterTrait characterTrait = characterTraits[i];

            for (int j = 0; j < characterTrait.traitEfficiencyModifiers.Count; j++)
            {
                TraitEfficiencyModifier traitEfficiencyModifier = characterTrait.traitEfficiencyModifiers[j];

                // Debug.Log($"CalculateEfficiency2 {characterTrait.traitName} {traitEfficiencyModifier.leadershipPosition} {traitEfficiencyModifier.efficiencyModifier}");

                if (traitEfficiencyModifier.leadershipPosition == leadershipPosition)
                {
                    efficiency += traitEfficiencyModifier.efficiencyModifier;
                }
            }
        }

        return efficiency;
    }
}
