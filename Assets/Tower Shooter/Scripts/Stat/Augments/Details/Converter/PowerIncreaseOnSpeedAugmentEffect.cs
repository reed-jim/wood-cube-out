using UnityEngine;

[CreateAssetMenu(fileName = "PowerIncreaseOnSpeedAugmentEffect", menuName = "Stats/Effects/PowerIncreaseOnSpeedAugmentEffect")]
public class PowerIncreaseOnSpeedAugmentEffect : AugmentEffect
{
    public float powerPerSpeed;

    public override void ApplyEffect(CharacterStats stats, object source)
    {
        if (stats.serviceLocator.powerIncreaseOnSpeedAugmentEffectRuntime == null)
        {
            stats.serviceLocator.powerIncreaseOnSpeedAugmentEffectRuntime = stats.gameObject.AddComponent<PowerIncreaseOnSpeedAugmentEffectRuntime>();
        }

        stats.serviceLocator.powerIncreaseOnSpeedAugmentEffectRuntime.Apply(stats, powerPerSpeed);
    }

    public override void RemoveEffect(CharacterStats target, object source)
    {

    }
}

public class PowerIncreaseOnSpeedAugmentEffectRuntime : MonoBehaviour
{
    private StatModifier lastStatModifier;

    public void Apply(CharacterStats stats, float powerPerSpeed)
    {
        float speed = stats.GetStat(StatType.AttackSpeed).Value;

        float bonusPower = powerPerSpeed * speed;

        StatModifier statModifier = new StatModifier(StatType.Damage, ModifierType.Flat, bonusPower);

        if (lastStatModifier != null)
        {
            stats.RemoveModifier(lastStatModifier);
        }

        stats.AddModifier(statModifier);

        lastStatModifier = statModifier;
    }
}
