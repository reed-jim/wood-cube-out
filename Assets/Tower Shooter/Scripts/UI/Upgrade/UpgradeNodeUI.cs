using System.Collections.Generic;
using UnityEngine;

public class UpgradeNodeUI : Singleton<UpgradeNodeUI>
{
    public List<UpgradeItemUI> upgradeItemUIs;

    public BaseCharacter selectedNode;

    public void Init(BaseCharacter node)
    {
        selectedNode = node;

        for (int i = 0; i < upgradeItemUIs.Count; i++)
        {
            UpgradeItemUI upgradeItemUI = upgradeItemUIs[i];

            upgradeItemUI.Init(selectedNode);
        }
    }

    public void SelectNode(BaseCharacter node)
    {
        Init(node);
    }
}
