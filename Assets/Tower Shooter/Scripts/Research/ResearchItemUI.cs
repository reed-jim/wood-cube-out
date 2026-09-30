using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ResearchItemUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Image progressBar;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Button clickButton;

    public ResearchNodeSO BoundNode { get; private set; }
    public static event Action<ResearchNodeSO> showResearchDetailEvent;

    private void Awake()
    {
        clickButton.onClick.AddListener(OnItemClicked);
    }

    public void Bind(ResearchNodeSO config)
    {
        BoundNode = config;
        if (iconImage) iconImage.sprite = config.icon;
        if (titleText) titleText.text = config.researchName;

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (BoundNode == null) return;

        int currentLevel = ResearchManager.Ins.GetResearchLevel(BoundNode.researchID);
        if (levelText) levelText.text = $"Lv. {currentLevel} / {BoundNode.maxLevel}";

        ActiveResearch active = ResearchManager.Ins.CurrentActiveResearch;

        if (active != null && active.Config.researchID == BoundNode.researchID)
        {
            float progress = 1f - (active.RemainingTime / active.TotalDuration);
            if (progressBar) progressBar.fillAmount = Mathf.Clamp01(progress);
            if (timerText) timerText.text = $"{Mathf.CeilToInt(active.RemainingTime)}s";
        }
        else
        {
            if (progressBar) progressBar.fillAmount = 0f;
            if (timerText) timerText.text = string.Empty;
        }
    }

    private void OnItemClicked()
    {
        showResearchDetailEvent?.Invoke(BoundNode);
    }
}
