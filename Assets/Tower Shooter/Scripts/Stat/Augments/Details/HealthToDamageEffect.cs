using UnityEngine;

[CreateAssetMenu(fileName = "HealthToDamageEffect", menuName = "Stats/Effects/Health To Damage")]
public class HealthToDamageEffect : AugmentEffect
{
    public float damagePerHundredHealth = 1f;

    public override void ApplyEffect(GameObject target, object source)
    {
        var stats = target.GetComponent<CharacterStats>();
        if (stats == null) return;

        // Custom component to monitor updates continuously
        var runtimeEffect = target.AddComponent<HealthToDamageRuntime>();
        runtimeEffect.Initialize(stats, source, damagePerHundredHealth);
    }

    public override void RemoveEffect(GameObject target, object source)
    {
        var runtime = target.GetComponent<HealthToDamageRuntime>();
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

        float maxHp = stats.GetValue(StatType.MaxHealth);
        float bonusDamage = (maxHp / 100f) * ratio;

        currentMod = new StatModifier(StatType.Damage, ModifierType.Flat, bonusDamage, source);
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
