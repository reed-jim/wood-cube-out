using UnityEngine;

public abstract class AugmentEffect : ScriptableObject
{
    public abstract void ApplyEffect(GameObject target, object source);
    public abstract void RemoveEffect(GameObject target, object source);
}
