using System.Collections.Generic;
using UnityEngine;

public static class AugmentScoringUtility
{
    /// <summary>
    /// Evaluates a list of GenericAugmentEffects and returns the one with the highest overpower score.
    /// Returns null if the list is empty or null.
    /// </summary>
    public static GenericAugmentEffect GetStrongestAugment(List<GenericAugmentEffect> effects)
    {
        if (effects == null || effects.Count == 0) return null;

        GenericAugmentEffect strongestEffect = null;
        float maxScore = float.MinValue;

        foreach (var effect in effects)
        {
            if (effect == null) continue;

            // Calculate the score using its underlying config
            float score = AugmentScoringUtility.CalculateOverpoweredScore(effect.config);

            if (score > maxScore)
            {
                maxScore = score;
                strongestEffect = effect;
            }
        }

        return strongestEffect;
    }

    /// <summary>
    /// Calculates an estimated "Overpowered Score" for a given augment configuration.
    /// Higher scores indicate a more impactful or potentially overpowered effect.
    /// </summary>
    public static float CalculateOverpoweredScore(GenericAugmentData config)
    {
        if (config == null) return 0f;

        // 1. Base magnitude contribution
        float score = Mathf.Abs(config.baseValue);

        // 2. Estimate scaling contribution (assuming a baseline source stat value of 100 for estimation)
        float estimatedSourceStat = 100f;
        float scalingContribution = ScalingMath.Evaluate(config.baseValue, estimatedSourceStat, config.scalingFunction, config.scalingFactor);
        score += Mathf.Abs(scalingContribution);

        // 3. Target Stat Weighting (adjust multipliers based on how critical the stat is)
        float statWeight = config.targetStat switch
        {
            StatType.Damage => 1.0f,
            StatType.AttackSpeed => 1.2f,
            StatType.CritChance => 1.5f,
            StatType.CritMultiplier => 1.8f,
            _ => 1.0f
        };
        score *= statWeight;

        // 4. Scope Multiplier (Global and Adjacent effects affect multiple targets, compounding power)[cite: 2]
        float scopeMultiplier = config.scope switch
        {
            EffectScope.Global => 5.0f,       // Affects all unlocked nodes
            EffectScope.AdjacentNodes => 2.5f, // Affects neighboring nodes
            EffectScope.Self => 1.0f,
            _ => 1.0f
        };
        score *= scopeMultiplier;

        // 5. Operation Type Weighting
        if (config.operation == EffectOperation.Multiply)
        {
            score *= 1.5f; // Multipliers scale exponentially better into late-game
        }
        else if (config.operation == EffectOperation.Convert || config.operation == EffectOperation.Swap)
        {
            score *= 1.2f; // Utility/conversion operations have high synergy potential
        }

        // 6. Duration & Lifetime Multiplier[cite: 2]
        float durationMultiplier = config.durationType switch
        {
            EffectDuration.Permanent => 1.2f,
            EffectDuration.Stacking => 1.0f + (0.1f * Mathf.Min(config.maxStacks, 10f)),
            EffectDuration.TemporaryBurst => 0.8f,
            EffectDuration.Decaying => 0.9f,
            _ => 1.0f
        };
        score *= durationMultiplier;

        return score;
    }

    /// <summary>
    /// Compares two augment configurations and returns a positive value if A is stronger, 
    /// negative if B is stronger, or 0 if they are roughly balanced.
    /// </summary>
    public static int CompareAugmentPower(GenericAugmentData configA, GenericAugmentData configB)
    {
        float scoreA = CalculateOverpoweredScore(configA);
        float scoreB = CalculateOverpoweredScore(configB);

        return scoreA.CompareTo(scoreB);
    }
}