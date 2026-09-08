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
    #endregion
}
