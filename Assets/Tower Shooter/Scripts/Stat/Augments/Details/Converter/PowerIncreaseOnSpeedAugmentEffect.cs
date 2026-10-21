using UnityEngine;

[CreateAssetMenu(fileName = "PowerIncreaseOnSpeedAugmentEffect", menuName = "Stats/Effects/PowerIncreaseOnSpeedAugmentEffect")]
public class PowerIncreaseOnSpeedAugmentEffect : AugmentEffect
{
    public float powerPerSpeed;

    public override void ApplyEffect(CharacterStats stats, object source)
    {
        Convert(stats);
    }

    public override void ApplyEffectOnStatChanged(CharacterStats stats)
    {
        Convert(stats);
    }

    public override void RemoveEffect(CharacterStats target, object source)
    {

    }

    protected override (string Key, object Value)[] GetDescriptionTokens()
    {
        var tokens = new (string Key, object Value)[1];

        tokens[0] = ($"value0", powerPerSpeed);

        return tokens;
    }

    private void Convert(CharacterStats stats)
    {
        if (stats.serviceLocator.powerIncreaseOnSpeedAugmentEffectRuntime == null)
        {
            stats.serviceLocator.powerIncreaseOnSpeedAugmentEffectRuntime = stats.gameObject.AddComponent<PowerIncreaseOnSpeedAugmentEffectRuntime>();
        }

        stats.serviceLocator.powerIncreaseOnSpeedAugmentEffectRuntime.Apply(stats, powerPerSpeed);
    }
}

public class PowerIncreaseOnSpeedAugmentEffectRuntime : MonoBehaviour
{
    private StatModifier lastStatModifier;

    public void Apply(CharacterStats stats, float powerPerSpeed)
    {
        double speed = stats.GetStat(StatType.AttackSpeed).Value;

        double bonusPower = powerPerSpeed * speed;

        StatModifier statModifier = new StatModifier(StatType.Damage, ModifierType.Flat, bonusPower);

        if (lastStatModifier != null)
        {
            stats.RemoveModifier(lastStatModifier);
        }

        stats.AddModifier(statModifier, isApplyAugmentEffect: false);

        lastStatModifier = statModifier;
    }
}
