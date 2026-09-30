using UnityEngine;

public static class StatModifierGenerator
{
    // Lower weight = less likely to be selected.
    private static readonly (StatType stat, float weight)[] StatWeights =
    {
        (StatType.Damage,          35f),
        (StatType.AttackSpeed,     30f),
        (StatType.CritMultiplier, 25f),
        (StatType.CritChance,     10f), // intentionally low
    };

    public static StatModifier Generate(StatType statType, float abilityPotential)
    {
        abilityPotential = Mathf.Max(0f, abilityPotential);

        StatType stat = GetRandomStat();

        // Randomly choose Flat / Percent.
        ModifierType modType = Random.value < 0.5f
            ? ModifierType.Flat
            : ModifierType.PercentMult;

        float value = GetModifierValue(stat, modType, abilityPotential);

        return new StatModifier(statType, modType, value);
    }

    public static StatModifier GenerateFlatModifier(StatType statType, float abilityPotential)
    {
        abilityPotential = Mathf.Max(0f, abilityPotential);

        StatType stat = GetRandomStat();

        ModifierType modType = ModifierType.Flat;

        float value = GetModifierValue(stat, modType, abilityPotential);

        return new StatModifier(statType, modType, value);
    }

    private static StatType GetRandomStat()
    {
        float totalWeight = 0f;

        foreach (var entry in StatWeights)
            totalWeight += entry.weight;

        float roll = Random.Range(0f, totalWeight);

        foreach (var entry in StatWeights)
        {
            roll -= entry.weight;

            if (roll <= 0f)
                return entry.stat;
        }

        return StatType.Damage;
    }

    private static float GetModifierValue(
        StatType stat,
        ModifierType mode,
        float abilityPotential)
    {
        // Base scaling.
        float value = abilityPotential;

        // Different stats have different power budgets.
        float multiplier = stat switch
        {
            StatType.Damage => 1.00f,
            StatType.AttackSpeed => 0.80f,
            StatType.CritMultiplier => 0.70f,
            StatType.CritChance => 0.20f, // heavily reduced
            _ => 1f
        };

        value *= multiplier;

        // Percent modifiers are generally smaller because
        // they can scale very strongly with high base stats.
        if (mode == ModifierType.PercentMult)
            value *= 0.01f;

        return value;
    }
}
