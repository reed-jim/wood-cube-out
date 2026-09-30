using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ResearchDetailUI : BasePopup
{
    [Header("UI Components")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text durationText;
    [SerializeField] private Button startResearchButton;
    [SerializeField] private RectTransform buffContainer;

    private List<StatItemUI> statItemUIs = new List<StatItemUI>();
    private ResearchNodeSO currentConfig;

    protected override void RegisterMoreEvent()
    {
        base.RegisterMoreEvent();
        ResearchItemUI.showResearchDetailEvent += ShowDetail;
        startResearchButton.onClick.AddListener(OnStartResearchClicked);
    }

    protected override void MoreActionOnDestroy()
    {
        base.MoreActionOnDestroy();
        ResearchItemUI.showResearchDetailEvent -= ShowDetail;
        startResearchButton.onClick.RemoveListener(OnStartResearchClicked);
    }

    public void ShowDetail(ResearchNodeSO config)
    {
        Show();
        currentConfig = config;

        iconImage.sprite = config.icon;
        titleText.text = config.researchName;
        descriptionText.text = config.description;

        int currentLevel = ResearchManager.Ins.GetResearchLevel(config.researchID);
        int targetLevel = currentLevel + 1;

        if (currentLevel >= config.maxLevel)
        {
            costText.text = "MAXED";
            durationText.text = "--";
            startResearchButton.interactable = false;
        }
        else
        {
            double cost = config.GetCostForLevel(targetLevel);
            float duration = config.GetDurationForLevel(targetLevel);

            costText.text = $"{cost:N0}";
            durationText.text = $"{Mathf.CeilToInt(duration)}s";

            startResearchButton.interactable = ResearchManager.Ins.CanStartResearch(config, out _);
        }

        // Render Stats
        CommonUtil.ReturnListToPool(statItemUIs, item => item.gameObject);

        for (int i = 0; i < config.baseStatModifiers.Count; i++)
        {
            StatModifierData statData = config.baseStatModifiers[i];
            StatItemUI statItemUI = ObjectPoolingEverything.GetFromPool<StatItemUI>(GameConstants.statItemUI);

            statItemUI.Init(statData.statType, statData.modType, statData.value);
            CommonUtil.SetParentAndResetScale(statItemUI.transform, buffContainer);
            statItemUIs.Add(statItemUI);
        }
    }

    private void OnStartResearchClicked()
    {
        if (currentConfig == null) return;

        if (ResearchManager.Ins.TryStartResearch(currentConfig))
        {
            Hide();
        }
    }
}
