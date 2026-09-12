using System.Collections.Generic;
using Dreamteck.Utilities;
using UnityEngine;

[CreateAssetMenu(fileName = "TimeCapsuleAugmentEffect", menuName = "Stats/Effects/TimeCapsuleAugmentEffect")]
public class TimeCapsuleAugmentEffect : AugmentEffect
{
    public float capsuleCapacity;
    public float burstPowerMultiplier;

    public override void ApplyEffect(CharacterStats target, object source)
    {


    }

    public override void RemoveEffect(CharacterStats target, object source)
    {

    }

    public override void ApplyEffectOnCycleCompleted(CharacterStats target)
    {
        StorePower(target);
    }

    protected override (string Key, object Value)[] GetDescriptionTokens()
    {
        var tokens = new (string Key, object Value)[2];

        tokens[0] = ($"value0", capsuleCapacity);
        tokens[1] = ($"value1", burstPowerMultiplier);

        return tokens;
    }

    private void StorePower(CharacterStats stats)
    {
        if (stats.serviceLocator.timeCapsuleAugmentEffectRuntime == null)
        {
            stats.serviceLocator.timeCapsuleAugmentEffectRuntime = stats.gameObject.AddComponent<TimeCapsuleAugmentEffectRuntime>();
        }

        stats.serviceLocator.timeCapsuleAugmentEffectRuntime.Apply(stats, capsuleCapacity, burstPowerMultiplier);
    }
}

public class TimeCapsuleAugmentEffectRuntime : MonoBehaviour
{
    public float capsuleCapacity;
    public float burstPowerMultiplier;
    private float storedPower;

    private StoredPowerCapsuleUI storedPowerCapsuleUI;

    public void Apply(CharacterStats stats, float capsuleCapacity, float burstPowerMultiplier)
    {
        this.capsuleCapacity = capsuleCapacity;
        this.burstPowerMultiplier = burstPowerMultiplier;

        BaseCharacter node = stats.serviceLocator.character;

        if (storedPowerCapsuleUI == null)
        {
            storedPowerCapsuleUI = ObjectPoolingEverything.GetFromPool<StoredPowerCapsuleUI>(GameConstants.storedPowerCapsuleUI);

            storedPowerCapsuleUI.Init(node);
        }

        storedPower += node.lastPowerGenerated;

        if (storedPower > capsuleCapacity)
        {
            node.GenerateInstantPower(burstPowerMultiplier * capsuleCapacity);

            storedPower = 0;
        }

        storedPowerCapsuleUI.UpdateProgress(storedPower / capsuleCapacity);
    }
}
