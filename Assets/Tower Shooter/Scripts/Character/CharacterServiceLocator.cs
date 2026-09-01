using UnityEngine;

public class CharacterServiceLocator : MonoBehaviour
{
    public BaseCharacter character;
    public CharacterStats stats;
    public HealthComponent healthComponent;
    public DamageOverTimeReceiver damageOverTimeReceiver;
    public LazyHealthBar lazyHealthBar;
    public StackingUI stackingUI;
}
