using UnityEngine;

[CreateAssetMenu(fileName = "PredictionAugmentEffect", menuName = "Stats/Effects/PredictionAugmentEffect")]
public class PredictionAugmentEffect : AugmentEffect
{
    public float minBonusPercent;
    public float maxBonusPercent;

    public override void ApplyEffect(CharacterStats target, object source)
    {
        InitRuntime(target);
    }

    public override void RemoveEffect(CharacterStats target, object source)
    {

    }

    public override void ApplyEffectOnCycleCompleted(CharacterStats target)
    {
        Apply(target);
    }

    protected override (string Key, object Value)[] GetDescriptionTokens()
    {
        var tokens = new (string Key, object Value)[2];

        tokens[0] = ($"value0", minBonusPercent);
        tokens[1] = ($"value1", maxBonusPercent);

        return tokens;
    }

    private void InitRuntime(CharacterStats stats)
    {
        if (stats.serviceLocator.predictionAugmentEffectRuntime == null)
        {
            stats.serviceLocator.predictionAugmentEffectRuntime = stats.gameObject.AddComponent<PredictionAugmentEffectRuntime>();
        }

        stats.serviceLocator.predictionAugmentEffectRuntime.Init(minBonusPercent, maxBonusPercent);
    }

    private void Apply(CharacterStats stats)
    {
        if (stats.serviceLocator.predictionAugmentEffectRuntime == null)
        {
            stats.serviceLocator.predictionAugmentEffectRuntime = stats.gameObject.AddComponent<PredictionAugmentEffectRuntime>();
        }

        stats.serviceLocator.predictionAugmentEffectRuntime.Apply(stats);
    }
}

public class PredictionAugmentEffectRuntime : MonoBehaviour
{
    public float minBonusPercent;
    public float maxBonusPercent;

    private StatModifier statModifier;

    public void Init(float minBonusPercent, float maxBonusPercent)
    {
        this.minBonusPercent = minBonusPercent;
        this.maxBonusPercent = maxBonusPercent;
    }

    public void Apply(CharacterStats stats)
    {
        BaseCharacter node = stats.serviceLocator.character;

        if (node.isJustCrit)
        {
            float randomBonusPercent = Random.RandomRange(minBonusPercent, maxBonusPercent);

            if (statModifier == null)
            {
                statModifier = new StatModifier(StatType.Damage, ModifierType.PercentAdd, randomBonusPercent);
            }
            else
            {
                double oldRandomBonusPercent = statModifier.Value;

                stats.RemoveModifier(statModifier);

                statModifier = new StatModifier(StatType.Damage, ModifierType.PercentAdd, oldRandomBonusPercent + randomBonusPercent);
            }

            stats.AddModifier(statModifier, isApplyAugmentEffect: false);
        }
    }
}
