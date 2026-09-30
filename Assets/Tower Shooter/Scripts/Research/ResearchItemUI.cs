using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResearchItemUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Button selectButton;
    [SerializeField] private RectTransform lockedOverlay;
    [SerializeField] private RectTransform completedOverlay;

    public ResearchNodeSO BoundConfig { get; private set; }

    public static event Action<ResearchNodeSO> showResearchDetailEvent;

    private void Awake()
    {
        selectButton.onClick.AddListener(OnItemClicked);
    }

    public void Bind(ResearchNodeSO config)
    {
        BoundConfig = config;

        if (iconImage) iconImage.sprite = config.researchIcon;

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (BoundConfig == null) return;

        bool isUnlocked = ResearchManager.Ins.IsUnlocked(BoundConfig.researchID);
        bool canResearch = ResearchManager.Ins.CanResearch(BoundConfig);

        lockedOverlay.gameObject.SetActive(!isUnlocked && !canResearch);
        completedOverlay.gameObject.SetActive(isUnlocked);
    }

    private void OnItemClicked()
    {
        showResearchDetailEvent?.Invoke(BoundConfig);
    }
}
