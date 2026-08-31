using System;
using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class HealthComponent : MonoBehaviour, IDamageable
{
    public float CurrentHealth { get; private set; }

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

        characterServiceLocator.lazyHealthBar.SetProgress(HealthPercent);

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
}
