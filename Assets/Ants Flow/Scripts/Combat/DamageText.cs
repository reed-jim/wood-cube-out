using TMPro;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.UI;
using Unity.Mathematics;

public class DamageText : MonoBehaviour
{
    [SerializeField] private TMP_Text damageText;

    [Header("Theme")]
    [SerializeField] private Material lightTextBorderMaterial;
    [SerializeField] private Material darkTextBorderMaterial;

    [Header("Animation Settings")]
    [SerializeField] private float lifetime = 0.85f;
    [SerializeField] private float floatDistance = 1.2f;

    [Header("Juice")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color critColor = new Color(1f, 0.85f, 0.2f); // Punchy yellow-gold

    [Header("Augment Visual Palette")]
    [SerializeField] private Color damageStatColor = new Color(1f, 0.35f, 0.2f);       // Red / Orange
    [SerializeField] private Color attackSpeedStatColor = new Color(0.2f, 0.85f, 1f);  // Cyan / Blue
    [SerializeField] private Color critChanceStatColor = new Color(0.9f, 0.2f, 0.9f);   // Magenta
    [SerializeField] private Color critMultiStatColor = new Color(1f, 0.8f, 0.1f);    // Yellow / Gold
    [SerializeField] private Color swapEffectColor = new Color(0.3f, 1f, 0.5f);        // Vibrant Green

    // Aggregation tracking
    public Transform CurrentTarget { get; private set; }
    public double CurrentDamage { get; private set; }
    public bool IsActive { get; private set; }

    private Sequence currentSequence;
    private Action<DamageText> onCompleteCallback;

    void Awake()
    {
        DynamicThemeManager.OnThemeChanged += OnThemeChanged;
    }

    void OnDestroy()
    {
        DynamicThemeManager.OnThemeChanged -= OnThemeChanged;
    }

    void Start()
    {
        normalColor = DynamicThemeManager.Ins.CurrentTheme.primary;

        if (DynamicThemeManager.Ins.CurrentTheme.isDarkTheme)
        {
            damageText.fontMaterial = darkTextBorderMaterial;
        }
        else
        {
            damageText.fontMaterial = lightTextBorderMaterial;
        }
    }

    public void ShowDamage(double damage, Vector3 spawnPosition, bool isCritical, Transform target, string prefix = "", string suffix = "", Action<DamageText> onComplete = null)
    {
        IsActive = true;
        CurrentTarget = target;
        CurrentDamage = damage;
        onCompleteCallback = onComplete;

        // Reset transforms & kill existing tweens
        KillTweens();

        damageText.text = $"{prefix}{CommonUtil.FormatNumber(CurrentDamage)}{suffix}";
        damageText.rectTransform.sizeDelta = new Vector2(1.5f * damageText.preferredWidth, damageText.preferredHeight);
        damageText.color = isCritical ? critColor : normalColor;
        damageText.alpha = 1f;

        if (isCritical)
        {
            string critColorHex = ColorUtil.ColorToHex(critColor);

            damageText.text = $"<sprite=3 color={critColorHex}>{prefix}{CommonUtil.FormatNumber(CurrentDamage)}{suffix}";
        }

        transform.position = spawnPosition;

        // Random directional trajectory (-1 or +1)
        float horizontalDir = UnityEngine.Random.value < 0.5f ? -1f : 1f;
        float xOffset = UnityEngine.Random.Range(0.4f, 0.7f) * horizontalDir;
        Vector3 targetPos = spawnPosition + new Vector3(xOffset, floatDistance, 0f);

        float targetScale = isCritical ? 1.4f : 1.0f;
        transform.localScale = Vector3.zero;

        currentSequence = DOTween.Sequence();

        // 1. Initial Explosive Pop (OutBack)
        currentSequence.Join(
            transform.DOScale(targetScale, 0.12f).SetEase(Ease.OutBack, 3.5f)
        );

        // 2. Smooth Physics Arc Trajectory
        currentSequence.Join(
            transform.DOMoveY(targetPos.y, lifetime).SetEase(Ease.OutQuad)
        );
        currentSequence.Join(
            transform.DOMoveX(targetPos.x, lifetime).SetEase(Ease.OutCubic)
        );

        // 3. Smooth Critical Hit Impact
        if (isCritical)
        {
            currentSequence.Insert(
                0.08f,
                transform.DOPunchScale(Vector3.one * 0.4f, 0.25f, vibrato: 5, elasticity: 0.5f)
            );
        }

        // 4. Fade Out
        currentSequence.Insert(
            lifetime * 0.5f,
            damageText.DOFade(0f, lifetime * 0.5f).SetEase(Ease.InQuad)
        );

        currentSequence.OnComplete(FinishAndReturn);
    }

    /// <summary>
    /// Displays a visual pop-up effect specifically when a Generic Augment executes on a node.
    /// </summary>
    public void ShowAugmentEffect(
        GenericAugmentData augmentConfig,
        double value,
        Vector3 spawnPosition,
        Transform target,
        bool isMoveHalfway = false,
        Color? customColorOverride = null,
        Action<DamageText> onComplete = null)
    {
        IsActive = true;
        CurrentTarget = target;
        CurrentDamage = value;
        onCompleteCallback = onComplete;

        KillTweens();

        // Hide damage crit icon for generic stat popups

        // 1. Determine color by target stat or operation override
        Color textColor = customColorOverride ?? GetAugmentStatColor(augmentConfig);
        damageText.color = textColor;
        damageText.alpha = 1f;

        // 2. Format text based on operation type
        string formattedText = FormatAugmentText(augmentConfig, value, textColor);
        damageText.text = formattedText;
        CommonUtil.TryClearTMPSubmesh(damageText);
        damageText.rectTransform.sizeDelta = new Vector2(damageText.preferredWidth, damageText.preferredHeight);

        transform.position = spawnPosition;

        // Slight upward bounce float trajectory
        float xOffset = UnityEngine.Random.Range(-0.25f, 0.25f);
        Vector3 targetPos = spawnPosition + new Vector3(xOffset, floatDistance * 1f, 0f);

        if (isMoveHalfway)
        {
            targetPos = spawnPosition + new Vector3(xOffset, floatDistance * 0.6f, 0f);
        }

        transform.localScale = Vector3.zero;
        currentSequence = DOTween.Sequence();

        // Explosive outward pop
        currentSequence.Join(
            transform.DOScale(1.15f, 0.15f).SetEase(Ease.OutBack, 4f)
        );

        // Vertical float motion
        currentSequence.Join(
            transform.DOMoveY(targetPos.y, lifetime).SetEase(Ease.OutSine)
        );
        currentSequence.Join(
            transform.DOMoveX(targetPos.x, lifetime).SetEase(Ease.OutCubic)
        );

        // Quick elasticity punch for bursts/swaps/stack max
        if (augmentConfig.durationType == EffectDuration.TemporaryBurst || augmentConfig.operation == EffectOperation.Swap)
        {
            currentSequence.Insert(
                0.1f,
                transform.DOPunchScale(Vector3.one * 0.35f, 0.2f, vibrato: 6, elasticity: 0.5f)
            );
        }

        // Fade Out
        currentSequence.Insert(
            lifetime * 0.45f,
            damageText.DOFade(0f, lifetime * 0.55f).SetEase(Ease.InQuad)
        );

        currentSequence.OnComplete(FinishAndReturn);
    }

    // Format display string based on Augment Operation & Target Stat
    private string FormatAugmentText(GenericAugmentData config, double value, Color color)
    {
        if (config == null) return $"+{CommonUtil.FormatNumber(value)}";

        string hex = ColorUtil.ColorToHex(color);

        string sourceStatSpriteTag = $"<sprite name=\"{config.sourceStat}\" color={hex}>";
        string targetStatSpriteTag = $"<sprite name=\"{config.targetStat}\" color={hex}>";

        switch (config.operation)
        {
            case EffectOperation.Swap:
                return $"SWAP [{sourceStatSpriteTag} ↔ {targetStatSpriteTag}]";

            case EffectOperation.Multiply:
                return $"x{value:F1} {targetStatSpriteTag}";

            case EffectOperation.Convert:
                return $"CONVERT! +{CommonUtil.FormatNumber(value)} {targetStatSpriteTag}";

            default:
                string sign = value >= 0 ? "+" : "";
                return $"{sign}{CommonUtil.FormatNumber(value)} {targetStatSpriteTag}";
        }
    }

    // Color mapper based on Target Stat
    private Color GetAugmentStatColor(GenericAugmentData config)
    {
        if (config == null) return normalColor;

        if (config.operation == EffectOperation.Swap)
        {
            return swapEffectColor;
        }

        switch (config.targetStat)
        {
            case StatType.Damage:
                return damageStatColor;
            case StatType.AttackSpeed:
                return attackSpeedStatColor;
            case StatType.CritChance:
                return critChanceStatColor;
            case StatType.CritMultiplier:
                return critMultiStatColor;
            default:
                return normalColor;
        }
    }

    // Call this to add damage to an existing floating number
    public void AddDamage(float additionalDamage, bool isCritical)
    {
        CurrentDamage += additionalDamage;
        damageText.text = $"{CurrentDamage:F0}";

        if (isCritical) damageText.color = critColor;

        // Punch/Pop animation when aggregated to show the number grew
        transform.DOKill(complete: false); // Kill active scale tweens, keep position sequence

        float punchMagnitude = isCritical ? 0.5f : 0.3f;
        transform.DOPunchScale(Vector3.one * punchMagnitude, 0.18f, vibrato: 6, elasticity: 0.4f);

        // Restart fade-out delay on hit refresh
        currentSequence?.Insert(
            lifetime * 0.5f,
            damageText.DOFade(0f, lifetime * 0.5f).SetEase(Ease.InQuad)
        );
    }

    private void KillTweens()
    {
        currentSequence?.Kill();
        transform.DOKill();
        damageText.DOKill();
    }

    private void FinishAndReturn()
    {
        IsActive = false;
        CurrentTarget = null;
        onCompleteCallback?.Invoke(this);
        ObjectPoolingEverything.ReturnToPool(GameConstants.damageText, gameObject);
    }

    private void OnThemeChanged(ThemeConfig themeConfig)
    {
        normalColor = DynamicThemeManager.Ins.CurrentTheme.primary;

        if (themeConfig.isDarkTheme)
        {
            damageText.fontMaterial = darkTextBorderMaterial;
        }
        else
        {
            damageText.fontMaterial = lightTextBorderMaterial;
        }
    }
}
