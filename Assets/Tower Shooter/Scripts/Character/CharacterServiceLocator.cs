using UnityEngine;

public class CharacterServiceLocator : MonoBehaviour
{
    public BaseCharacter character;
    public CharacterStats stats;
    public NodeUI nodeUI;
    public HealthComponent healthComponent;
    public DamageOverTimeReceiver damageOverTimeReceiver;
    public LazyHealthBar lazyHealthBar;
    public StackingUI stackingUI;

    #region Augment Runtime
    [Header("Augment Runtime")]
    public PowerIncreaseOnSpeedAugmentEffectRuntime powerIncreaseOnSpeedAugmentEffectRuntime;
    public TimeCapsuleAugmentEffectRuntime timeCapsuleAugmentEffectRuntime;
    public OverheatAugmentEffectRuntime overheatAugmentEffectRuntime;
    public ChainCriticalAugmentEffectRuntime chainCriticalAugmentEffectRuntime;
    public PredictionAugmentEffectRuntime predictionAugmentEffectRuntime;
    public AlternativeAugmentEffectRuntime alternativeAugmentEffectRuntime;
    public CritBoostSpeedAugmentEffectRuntime critBoostSpeedAugmentEffectRuntime;
    public SumAugmentEffectRuntime sumAugmentEffectRuntime;
    public DebuffAdjacentBuffSelfAugmentEffectRuntime debuffAdjacentBuffSelfAugmentEffectRuntime;
    public HighRiskSurgeAugmentEffectRuntime highRiskSurgeAugmentEffectRuntime;
    #endregion
}
