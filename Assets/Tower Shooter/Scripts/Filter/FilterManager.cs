using UnityEngine;

public enum GameFilterMode
{
    Normal,
    Augment
}

public class FilterManager : Singleton<FilterManager>
{
    public static GameFilterMode filterMode;

    public void EnableAugmentFilter()
    {
        for (int i = 0; i < NodeManager.Ins.nodes.Count; i++)
        {
            BaseCharacter node = NodeManager.Ins.nodes[i];

            if (node.nodeData.isUnlocked)
            {
                node.serviceLocator.nodeUI.EnableAugmentFilter();
            }
        }
    }

    public void DisableAugmentFilter()
    {
        for (int i = 0; i < NodeManager.Ins.nodes.Count; i++)
        {
            BaseCharacter node = NodeManager.Ins.nodes[i];

            if (node.nodeData.isUnlocked)
            {
                node.serviceLocator.nodeUI.DisableAugmentFilter();
            }
        }
    }
}
