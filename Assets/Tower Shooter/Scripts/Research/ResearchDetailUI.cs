using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResearchDetailUI : BasePopup
{
    [Header("UI Components")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Button unlockButton;
    [SerializeField] private RectTransform buffContainer;

    private List<StatItemUI> statItemUIs = new List<StatItemUI>();
    private ResearchNodeSO currentResearch;

    protected override void RegisterMoreEvent()
    {
        base.RegisterMoreEvent();

        ResearchItemUI.showResearchDetailEvent += ShowDetail;
        ResearchManager.OnResearchUpdated += UpdateDynamicUI;
    }

    protected override void UnregisterMoreEvent()
    {
        base.UnregisterMoreEvent();

        ResearchItemUI.showResearchDetailEvent -= ShowDetail;
        ResearchManager.OnResearchUpdated -= UpdateDynamicUI;
    }

    private void Awake()
    {
        unlockButton.onClick.AddListener(OnUnlockClicked);
    }

    public void ShowDetail(ResearchNodeSO researchSO)
    {
        Show();
        currentResearch = researchSO;

        iconImage.sprite = researchSO.researchIcon;
        titleText.text = researchSO.researchTitle;
        descriptionText.text = researchSO.researchDescription;

        CommonUtil.ReturnListToPool(statItemUIs, item => item.gameObject);

        for (int i = 0; i < researchSO.statModifiers.Count; i++)
        {
            StatModifierData statModifierData = researchSO.statModifiers[i];
            StatItemUI statItemUI = ObjectPoolingEverything.GetFromPool<StatItemUI>(GameConstants.statItemUI);

            statItemUI.Init(statModifierData.statType, statModifierData.modType, statModifierData.value);
            CommonUtil.SetParentAndResetScale(statItemUI.transform, buffContainer);

            statItemUIs.Add(statItemUI);
        }

        UpdateDynamicUI();
    }

    private void UpdateDynamicUI()
    {
        if (currentResearch == null) return;

        bool isUnlocked = ResearchManager.Ins.IsUnlocked(currentResearch.researchID);
        bool canResearch = ResearchManager.Ins.CanResearch(currentResearch);

        if (costText) costText.text = isUnlocked ? "Completed" : $"Cost: {CommonUtil.FormatNumber(currentResearch.cost)}";
        if (unlockButton) unlockButton.interactable = canResearch && !isUnlocked;
    }

    private void OnUnlockClicked()
    {
        if (currentResearch != null && ResearchManager.Ins.TryUnlockResearch(currentResearch))
        {
            UpdateDynamicUI();
        }
    }
}
