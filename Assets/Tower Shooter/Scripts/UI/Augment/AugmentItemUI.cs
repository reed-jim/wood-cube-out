using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AugmentItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text title;
    public TMP_Text qualityText;
    public TagItemUI qualityTag;
    public PlanetTagItemUI planetTagItemUI;
    // public TMP_Text augmentLevelText;
    public AugmentIcon augmentIcon;
    public AugmentIconQualityEffect augmentIconEffect;
    public AugmentUIEffect augmentLevelEffect;
    public AugmentUIEffect nextAugmentLevelEffect;
    public GameObject augmentLevelUpgradeArrow;
    public TMP_Text efficiencyText;
    public TMP_Text description;
    public Image qualityBorder;
    public Button showAugmentDetailButton;
    public Image upgradeHighlight;

    private Augment augment;

    [Header("SO")]
    [SerializeField] private StatColorConfigSO colorConfig;

    [Header("Tooltip")]
    [SerializeField] private float hoverDelay = 0.5f;
    private Coroutine hoverCoroutine;

    public static event Action<Augment> showAugmentDetailEvent;
    private Action onAugmentSelected;

    void Awake()
    {
        showAugmentDetailButton.onClick.AddListener(PickAugment);
    }

    public void Init(Augment augment, Action onAugmentSelected)
    {
        gameObject.SetActive(true);

        this.augment = augment;

        SetupAugment(augment);

        this.onAugmentSelected = onAugmentSelected;
    }

    private void SetupAugment(Augment augment)
    {
        AugmentLevel augmentLevel = AugmentManager.Ins.GetAugmentLevel(augment);
        bool isUpLevelAugment = AugmentManager.Ins.IsAugmentActive(augment);

        title.text = augment.augmentName;
        efficiencyText.text = $"Efficiency: {CommonUtil.ToPercent(AugmentManager.Ins.GetAugmentEfficiency(augmentLevel))}";

        if (isUpLevelAugment)
        {
            description.text = augment.BuildFullDescriptionForUpLevelAugment(colorConfig, augmentLevel);
        }
        else
        {
            description.text = augment.BuildFullDescription(colorConfig);
        }

        qualityText.text = $"{augment.augmentQuality}";

        augmentLevelUpgradeArrow.gameObject.SetActive(false);
        nextAugmentLevelEffect.gameObject.SetActive(false);

        augmentLevelUpgradeArrow.transform.localScale = Vector3.one;

        augmentLevelEffect.SetLevel(augmentLevel);

        Color qualityColor = ThemeManager.GetRarityColor(augment.augmentQuality);

        qualityText.color = qualityColor;
        qualityBorder.color = qualityColor;

        augmentIcon.SetupIcon(augment);
        // augmentIconEffect.SetQuality(augment.augmentQuality);

        qualityTag.SetBorderColor(qualityColor);
        planetTagItemUI.Init(augment.spaceObjectType);

        upgradeHighlight.gameObject.SetActive(false);

        if (isUpLevelAugment)
        {
            ShowUpgradeAugmentLevelUI(augmentLevel);
        }
    }

    private void ShowAugmentDetail()
    {
        showAugmentDetailEvent?.Invoke(augment);
    }

    public void PickAugment()
    {
        if (augment.isNeedCharacterToApply)
        {
            AugmentManager.Ins.PickNodeForAugment(augment);
        }
        else
        {
            foreach (var augmentEffect in augment.dynamicEffects)
            {
                augmentEffect.ApplyGlobalEffect();
            }
        }

        onAugmentSelected?.Invoke();
    }

    #region Augment Level
    private void ShowUpgradeAugmentLevelUI(AugmentLevel currentAugmentLevel)
    {
        AugmentLevel nextAugmentLevel = currentAugmentLevel + 1;

        string currentEfficiency = AugmentManager.Ins.GetAugmentEfficiencyString(currentAugmentLevel);
        string nextEfficiency = AugmentManager.Ins.GetAugmentEfficiencyString(nextAugmentLevel);

        // augmentLevelText.text = $"{currentAugmentLevel} --> <color={DynamicThemeManager.Ins.PrimaryHex}>{nextAugmentLevel}";
        // augmentLevelText.text = $"{nextAugmentLevel}";
        efficiencyText.text = $"Efficiency: {currentEfficiency} > <color={DynamicThemeManager.Ins.PrimaryHex}>{nextEfficiency}";

        // augmentLevelUpgradeArrow.gameObject.SetActive(true);
        // nextAugmentLevelEffect.gameObject.SetActive(true);

        // augmentLevelEffect.transform.localScale = 0.5f * Vector3.one;
        // nextAugmentLevelEffect.transform.localScale = 0.5f * Vector3.one;

        // augmentLevelEffect.SetLevel(augmentLevel);
        // nextAugmentLevelEffect.SetLevel(nextAugmentLevel);
    }
    #endregion

    #region Hover to show tooltip
    private static AugmentItemUI currentlyHoveredItem;
    private static float hideTimer;
    private static bool isWaitingToHide;

    void Update()
    {
        // Handle the global hide grace period via Update to avoid Coroutine stop warnings
        if (isWaitingToHide)
        {
            hideTimer -= Time.unscaledDeltaTime;
            if (hideTimer <= 0f)
            {
                isWaitingToHide = false;
                if (currentlyHoveredItem == this)
                {
                    AugmentManager.Ins.HideAugmentTooltip();
                    currentlyHoveredItem = null;
                }
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Cancel any pending hide countdown because we entered an item
        isWaitingToHide = false;
        currentlyHoveredItem = this;

        // Cancel any local show delay if it was running
        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
            hoverCoroutine = null;
        }

        // SHOW IMMEDIATELY
        AugmentManager.Ins.ShowAugmentTooltip(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Cancel local show delay if exiting early
        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
            hoverCoroutine = null;
        }

        // Only start the hide countdown if this is still the active item
        if (currentlyHoveredItem == this)
        {
            hideTimer = 0.15f; // Grace period duration
            isWaitingToHide = true;
        }
    }
    #endregion

    #region Util
    public Augment GetAugment()
    {
        return augment;
    }
    #endregion
}
