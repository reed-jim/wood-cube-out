using UnityEngine;

[CreateAssetMenu(fileName = "ResourceAugmentEffect", menuName = "Stats/Effects/ResourceAugmentEffect")]
public class ResourceAugmentEffect : AugmentEffect
{
    public ResourceDataItem resourceDataItem;

    public override void ApplyEffect(CharacterStats target, object source)
    {
        ResourceManager.Ins.ChangeResource(resourceDataItem.resourceType, resourceDataItem.value);
    }

    public override void RemoveEffect(CharacterStats target, object source)
    {
        ResourceManager.Ins.ChangeResource(resourceDataItem.resourceType, -resourceDataItem.value);
    }
}
