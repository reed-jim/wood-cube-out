using UnityEngine;

[CreateAssetMenu(fileName = "NodeBehaviourAugment", menuName = "Stats/NodeBehaviourAugment")]
public class NodeBehaviourAugment : Augment
{
    public override void Pick()
    {
        AugmentManager.Ins.AddPendingAugment(this);
    }
}
