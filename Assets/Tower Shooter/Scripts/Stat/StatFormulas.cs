public static class StatFormulas
{
    // League of Legends Armor formula: Damage Multiplier = 100 / (100 + Armor)
    public static float CalculateMitigatedDamage(float rawDamage, float targetArmor)
    {
        if (targetArmor >= 0)
        {
            return rawDamage * (100f / (100f + targetArmor));
        }
        else
        {
            // Negative armor amplification formula
            return rawDamage * (2f - (100f / (100f - targetArmor)));
        }
    }

    // League of Legends Ability Haste formula: CDR = Haste / (Haste + 100)
    public static float CalculateCooldown(float baseCooldown, float abilityHaste)
    {
        float cooldownReduction = abilityHaste / (abilityHaste + 100f);
        return baseCooldown * (1f - cooldownReduction);
    }
}
