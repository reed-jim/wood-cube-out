using TMPro;
using UnityEngine;

public class NodeGroupDetailUI : BasePopup
{
    public TMP_Text nameText;
    public TMP_Text totalPowerOutputText;

    public RectTransform augmentContainer;

    private NodeGroup nodeGroup;

    protected override void RegisterMoreEvent()
    {
        base.RegisterMoreEvent();

        NodeGroupUI.showNodeGroupDetailEvent += ShowNodeGroupDetail;
    }

    protected override void UnregisterMoreEvent()
    {
        base.UnregisterMoreEvent();

        NodeGroupUI.showNodeGroupDetailEvent -= ShowNodeGroupDetail;
    }

    public void Init(NodeGroup nodeGroup)
    {
        nameText.text = nodeGroup.name;

        float totalPower = 0;

        for (int i = 0; i < nodeGroup.nodes.Count; i++)
        {
            CharacterStats nodeStats = nodeGroup.nodes[i].serviceLocator.stats;

            totalPower += nodeStats.GetStat(StatType.Damage).Value;
        }

        totalPowerOutputText.text = $"Total Power Output: {totalPower}";

        for (int i = 0; i < nodeGroup.activeAugments.Count; i++)
        {
            Augment augment = nodeGroup.activeAugments[i];

            NodeDetailAugmentItemUI nodeDetailAugmentItemUI = ObjectPoolingEverything.GetFromPool<NodeDetailAugmentItemUI>(GameConstants.nodeDetailAugmentItemUI);

            CommonUtil.SetParentAndResetScale(nodeDetailAugmentItemUI.transform, augmentContainer);

            nodeDetailAugmentItemUI.Init(augment);
        }
    }

    public void ShowNodeGroupDetail(NodeGroup nodeGroup)
    {
        Show();

        Init(nodeGroup);

        this.nodeGroup = nodeGroup;
    }
}
