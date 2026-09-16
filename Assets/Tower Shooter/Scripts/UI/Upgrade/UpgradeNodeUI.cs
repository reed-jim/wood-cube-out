using System.Collections.Generic;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeNodeUI : Singleton<UpgradeNodeUI>
{
    [Header("UI")]
    public CanvasGroup lockState;
    public Button unlockNodeButton;
    public TMP_Text unlockCostText;

    public List<UpgradeItemUI> upgradeItemUIs;

    public BaseCharacter selectedNode;

    private float nodeUnlockCost;

    private Tween lockStateTween;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        unlockNodeButton.onClick.AddListener(UnlockNode);
    }

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

            nodeUnlockCost = CurveManager.Ins.GetBuyAugmentCost(NodeManager.Ins.GetNumUnlockedNode());

            unlockCostText.text = $"{nodeUnlockCost}";
        }
    }

    public void SelectNode(BaseCharacter node)
    {
        Init(node);
    }

    public void UnlockNode()
    {
        if (ResourceManager.Ins.GetResourceValue(ResourceType.Coin) >= nodeUnlockCost)
        {
            ResourceManager.Ins.ChangeResource(ResourceType.Coin, -nodeUnlockCost);

            selectedNode.Unlock();

            SelectNode(selectedNode);
        }
        else
        {
            IngameNotificationManager.Ins.ShowNotification(GameConstants.WARNING, "Not enough power to unlock this node!");
        }        
    }
}
