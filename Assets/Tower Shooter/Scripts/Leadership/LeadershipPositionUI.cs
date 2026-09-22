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

    [Header("Customize")]
    public LeadershipPosition leadershipPosition;
    public bool isBuffAllStats;
    public List<StatModifierData> statModifierDataItems;
    public float baseEfficiency;

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
        efficiencyText.text = $"Efficiency: {(int)(baseEfficiency * 100)}%";
    }

    private void Select()
    {
        LeadershipManager.Ins.SelectLeadershipPosition(this);
    }

    public void AssignCharacter(LeadershipCharacterUI leadershipCharacterUI)
    {
        CommonUtil.SetParentAndResetScale(leadershipCharacterUI.transform, characterUIContainer, Vector3.zero);
    }
}
