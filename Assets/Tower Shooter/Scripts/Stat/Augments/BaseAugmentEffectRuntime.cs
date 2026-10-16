using UnityEngine;

public class BaseAugmentEffectRuntime : MonoBehaviour
{
    public Augment augment;

    public virtual void SetAugment(Augment augment)
    {
        this.augment = augment;
    }
}
