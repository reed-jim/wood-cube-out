using UnityEngine;

public abstract class AugmentEffect : ScriptableObject
{
    public abstract void ApplyEffect(CharacterStats target, object source);
    public abstract void RemoveEffect(CharacterStats target, object source);
}
