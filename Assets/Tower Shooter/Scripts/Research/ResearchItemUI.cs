using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResearchItemUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Button purchaseButton;
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private GameObject completedOverlay;

    public ResearchNodeSO BoundConfig { get; private set; }

    public static event Action<ResearchNodeSO> showResearchDetailEvent;

    private void Awake()
    {
        if (purchaseButton != null)
        {
            purchaseButton.onClick.AddListener(OnItemClicked);
        }
    }

    public void Bind(ResearchNodeSO config)
    {
        BoundConfig = config;

        if (iconImage) iconImage.sprite = config.researchIcon;
        if (titleText) titleText.text = config.researchTitle;

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (BoundConfig == null) return;

        bool isUnlocked = ResearchManager.Ins.IsUnlocked(BoundConfig.researchID);
        bool canResearch = ResearchManager.Ins.CanResearch(BoundConfig);

        if (costText) costText.text = isUnlocked ? "Unlocked" : CommonUtil.FormatNumber(BoundConfig.cost);
        if (lockedOverlay) lockedOverlay.SetActive(!isUnlocked && !canResearch);
        if (completedOverlay) completedOverlay.SetActive(isUnlocked);
        if (purchaseButton) purchaseButton.interactable = !isUnlocked;
    }

    private void OnItemClicked()
    {
        showResearchDetailEvent?.Invoke(BoundConfig);
    }
}
