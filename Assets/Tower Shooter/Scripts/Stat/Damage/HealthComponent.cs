using System;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class HealthComponent : MonoBehaviour, IDamageable
{
    public float CurrentHealth { get; private set; }

    public TMP_Text healthText;
    public LazyHealthBar lazyHealthBar;

    public event Action<DamageResult> OnDamageTaken;
    public event Action OnDeath;

    public CharacterServiceLocator characterServiceLocator;
    private CharacterStats stats;

    public float HealthPercent => CurrentHealth / stats.GetValue(StatType.MaxHealth);

    public struct DamageResult
    {
        public float FinalDamage;
        public bool IsCritical;
        public GameObject Attacker;
    }

    private void Awake()
    {
        stats = characterServiceLocator.stats;
    }

    private void Start()
    {
        // Set health to MaxHealth on start
        CurrentHealth = stats.GetValue(StatType.MaxHealth);

        SetHealthText();

        // Keep current health capped if Max Health decreases at runtime
        stats.GetStat(StatType.MaxHealth).OnStatChanged += HandleMaxHealthChanged;
    }

    private void HandleMaxHealthChanged()
    {
        float maxHp = stats.GetValue(StatType.MaxHealth);
        CurrentHealth = Mathf.Min(CurrentHealth, maxHp);
    }

    public void TakeDamage(DamageData damageData)
    {
        if (CurrentHealth <= 0) return;

        // Fetch Armor from stats
        float armor = stats.GetValue(StatType.Armor);

        // LoL Damage Mitigation Formula: 100 / (100 + Armor)
        float mitigationMultiplier = armor >= 0
            ? 100f / (100f + armor)
            : 2f - (100f / (100f - armor));

        float finalDamage = Mathf.Max(1f, damageData.RawDamage * mitigationMultiplier);

        CurrentHealth = Mathf.Clamp(CurrentHealth - finalDamage, 0, stats.GetValue(StatType.MaxHealth));

        SetHealthText();
        ShowDamageText(finalDamage, transform.position);

        // Notify UI, Damage Numbers, or Life Steal listeners
        OnDamageTaken?.Invoke(new DamageResult
        {
            FinalDamage = finalDamage,
            IsCritical = damageData.IsCritical,
            Attacker = damageData.Attacker
        });

        if (CurrentHealth <= 0)
        {
            OnDeath?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        if (CurrentHealth <= 0) return;

        float maxHp = stats.GetValue(StatType.MaxHealth);
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, maxHp);
    }

    public void SetHealthText()
    {
        healthText.text = $"{CurrentHealth:F1}/{stats.GetValue(StatType.MaxHealth)}";
    }

    public void ShowDamageText(float damage, Vector3 position)
    {
        DamageText damageText = ObjectPoolingEverything.GetFromPool<DamageText>(GameConstants.damageText);

        damageText.ShowDamage((int)damage, position.MinusZALittle());
    }
}
