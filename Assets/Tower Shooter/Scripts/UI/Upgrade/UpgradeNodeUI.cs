using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class UpgradeNodeUI : Singleton<UpgradeNodeUI>
{
    [Header("UI")]
    public CanvasGroup lockState;

    public List<UpgradeItemUI> upgradeItemUIs;

    public BaseCharacter selectedNode;

    private Tween lockStateTween;

    public void Init(BaseCharacter node)
    {
        selectedNode = node;

        for (int i = 0; i < upgradeItemUIs.Count; i++)
        {
            UpgradeItemUI upgradeItemUI = upgradeItemUIs[i];

            upgradeItemUI.Init(selectedNode);
        }

        if (lockStateTween.isAlive)
        {
            lockStateTween.Stop();
        }

        if (node.nodeData.isUnlocked)
        {
            lockStateTween = Tween.Alpha(lockState, 0, duration: 0.3f).OnComplete(() =>
            {
                lockState.gameObject.SetActive(false);
            });
        }
        else
        {
            lockState.gameObject.SetActive(true);

            lockStateTween = Tween.Alpha(lockState, 1, duration: 0.3f);
        }
    }

    public void SelectNode(BaseCharacter node)
    {
        Init(node);
    }
}
