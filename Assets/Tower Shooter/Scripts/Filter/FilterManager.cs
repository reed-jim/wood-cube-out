using UnityEngine;

public enum GameFilterMode
{
    Normal,
    Augment
}

public class FilterManager : Singleton<FilterManager>
{
    public static GameFilterMode filterMode;

    private bool isInTransition;

    public void EnableAugmentFilter()
    {
        if (isInTransition)
        {
            return;
        }

        for (int i = 0; i < NodeManager.Ins.nodes.Count; i++)
        {
            BaseCharacter node = NodeManager.Ins.nodes[i];

            if (node.nodeData.isUnlocked && node.ownedAugments.Count > 0)
            {
                node.serviceLocator.nodeUI.EnableAugmentFilter(onCompleteAction: () =>
                {
                    isInTransition = false;
                });
            }
        }

        isInTransition = true;
    }

    public void DisableAugmentFilter()
    {
        if (isInTransition)
        {
            return;
        }

        for (int i = 0; i < NodeManager.Ins.nodes.Count; i++)
        {
            BaseCharacter node = NodeManager.Ins.nodes[i];

            if (node.nodeData.isUnlocked)
            {
                node.serviceLocator.nodeUI.DisableAugmentFilter(onCompleteAction: () =>
                {
                    isInTransition = false;
                });
            }
        }

        isInTransition = true;
    }
}
