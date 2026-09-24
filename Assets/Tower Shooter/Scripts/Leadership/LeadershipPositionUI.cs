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
            efficiencyText.color = DynamicThemeManager.Ins.CurrentTheme.primary;
            efficiencyText.text = $"Efficiency: {(int)(baseEfficiency * 100)}%";

            borderThemeable.ChangeRole(ThemeableUI.ElementRole.Primary);
            titleThemeable.ChangeRole(ThemeableUI.ElementRole.Primary);
        }
    }

    private void Select()
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

    public void AssignCharacter(LeadershipCharacterUI leadershipCharacterUI)
    {
        CommonUtil.SetParentAndResetScale(leadershipCharacterUI.transform, characterUIContainer, Vector3.zero);

        efficiency = baseEfficiency;

        List<CharacterTrait> characterTraits = leadershipCharacterUI.GetCharacterTraits();

        for (int i = 0; i < characterTraits.Count; i++)
        {
            CharacterTrait characterTrait = characterTraits[i];

            for (int j = 0; j < characterTrait.traitEfficiencyModifiers.Count; j++)
            {
                TraitEfficiencyModifier traitEfficiencyModifier = characterTrait.traitEfficiencyModifiers[j];

                if (traitEfficiencyModifier.leadershipPosition == leadershipPosition)
                {
                    efficiency += traitEfficiencyModifier.efficiencyModifier;
                }
            }
        }

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
}
