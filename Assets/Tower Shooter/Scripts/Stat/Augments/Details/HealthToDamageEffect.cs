using UnityEngine;

[CreateAssetMenu(fileName = "HealthToDamageEffect", menuName = "Stats/Effects/Health To Damage")]
public class HealthToDamageEffect : AugmentEffect
{
    public float damagePerHundredHealth = 1f;

    public override void ApplyEffect(CharacterStats characterStats, object source)
    {
        // Custom component to monitor updates continuously
        var runtimeEffect = characterStats.gameObject.AddComponent<HealthToDamageRuntime>();
        runtimeEffect.Initialize(characterStats, source, damagePerHundredHealth);
    }

    public override void RemoveEffect(CharacterStats characterStats, object source)
    {
        var runtime = characterStats.gameObject.GetComponent<HealthToDamageRuntime>();
        if (runtime != null)
        {
            Destroy(runtime);
        }
    }
}

// Runtime component that converts Max Health to Bonus Damage dynamically
public class HealthToDamageRuntime : MonoBehaviour
{
    private CharacterStats stats;
    private object source;
    private float ratio;
    private StatModifier currentMod;

    public void Initialize(CharacterStats stats, object source, float ratio)
    {
        this.stats = stats;
        this.source = source;
        this.ratio = ratio;

        stats.GetStat(StatType.MaxHealth).OnStatChanged += RecalculateBonus;
        RecalculateBonus();
    }

    private void RecalculateBonus()
    {
        if (currentMod != null)
        {
            stats.RemoveModifier(currentMod);
        }

        double maxHp = stats.GetValue(StatType.MaxHealth);
        double bonusDamage = (maxHp / 100f) * ratio;

        // currentMod = new StatModifier(StatType.Damage, ModifierType.Flat, bonusDamage, 0, source);
        currentMod = new StatModifier(StatType.Damage, ModifierType.Flat, bonusDamage, 0);
        stats.AddModifier(currentMod);
    }

    private void OnDestroy()
    {
        if (stats != null)
        {
            stats.GetStat(StatType.MaxHealth).OnStatChanged -= RecalculateBonus;
            if (currentMod != null) stats.RemoveModifier(currentMod);
        }
    }
}
