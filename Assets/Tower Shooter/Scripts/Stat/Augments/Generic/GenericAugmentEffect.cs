using UnityEngine;

[CreateAssetMenu(fileName = "GenericAugmentEffect", menuName = "Stats/Effects/GenericAugmentEffect")]
public class GenericAugmentEffect : AugmentEffect
{
    public GenericAugmentData config;

    public override void ApplyEffect(CharacterStats target, object source)
    {
        EnsureRuntime(target).Init(config);
    }

    public override void RemoveEffect(CharacterStats target, object source)
    {
        var runtime = target.GetComponent<GenericAugmentEffectRuntime>();
        if (runtime != null)
        {
            Destroy(runtime);
        }
    }

    public override void ApplyEffectOnCycleCompleted(CharacterStats target)
    {
        var runtime = EnsureRuntime(target);
        runtime.OnTriggerRaised(EffectTrigger.OnCycleComplete);
    }

    // Pass the Augment instance down to the runtime so utility tracking knows which augment to accredit
    public override void SetAugment(CharacterStats target, Augment augment)
    {
        EnsureRuntime(target).SetAugment(augment);
    }

    public override void SetAugment(NodeGroup nodeGroup, Augment augment)
    {
        // For group-wide augments
        foreach (var node in nodeGroup.nodes)
        {
            if (node.serviceLocator != null && node.serviceLocator.stats != null)
            {
                EnsureRuntime(node.serviceLocator.stats).SetAugment(augment);
            }
        }
    }

    private GenericAugmentEffectRuntime EnsureRuntime(CharacterStats target)
    {
        var runtime = target.GetComponent<GenericAugmentEffectRuntime>();
        if (runtime == null)
        {
            runtime = target.gameObject.AddComponent<GenericAugmentEffectRuntime>();
            runtime.Init(config);
        }
        return runtime;
    }
}
