using TMPro;
using UnityEngine;
using DG.Tweening;
using System;

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

    // Aggregation tracking
    public Transform CurrentTarget { get; private set; }
    public float CurrentDamage { get; private set; }
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

    public void ShowDamage(float damage, Vector3 spawnPosition, bool isCritical, Transform target, string prefix = "", string suffix = "", Action<DamageText> onComplete = null)
    {
        IsActive = true;
        CurrentTarget = target;
        CurrentDamage = damage;
        onCompleteCallback = onComplete;

        // Reset transforms & kill existing tweens
        KillTweens();

        damageText.text = $"{prefix}{TextUtil.FormatFloat(CurrentDamage)}{suffix}";
        damageText.color = isCritical ? critColor : normalColor;
        damageText.alpha = 1f;

        transform.position = spawnPosition;

        // Random directional trajectory (-1 or +1)
        float horizontalDir = UnityEngine.Random.value < 0.5f ? -1f : 1f;
        float xOffset = UnityEngine.Random.Range(0.4f, 0.7f) * horizontalDir;
        Vector3 targetPos = spawnPosition + new Vector3(xOffset, floatDistance, 0f);

        // Subtle random rotation (-8 to 8 deg)
        // transform.rotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(-8f, 8f));

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

        // 3. Smooth Critical Hit Impact (Punch Scale instead of position shake)
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
