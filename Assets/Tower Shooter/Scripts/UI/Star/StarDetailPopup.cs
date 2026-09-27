using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StarDetailPopup : BasePopup
{
    public TMP_Text title;
    public Image descriptionImage;
    public TMP_Text description;

    public RectTransform augmentContainer;
    public RectTransform augmentSetBuffContainer;

    private StarPathConfig starPathConfig;
    private AugmentSetBuffConfig augmentSetBuffConfig;

    private List<NodeDetailAugmentItemUI> nodeDetailAugmentItemUIs = new List<NodeDetailAugmentItemUI>();
    private AugmentSetItemUI augmentSetItemUI;

    protected override void RegisterMoreEvent()
    {
        base.RegisterMoreEvent();

        VoyageMilestoneItemUI.openStarDetailEvent += ShowStarDetail;
    }

    protected override void UnregisterMoreEvent()
    {
        base.UnregisterMoreEvent();

        VoyageMilestoneItemUI.openStarDetailEvent -= ShowStarDetail;
    }

    public void Init(StarPathConfig starPathConfig)
    {
        title.text = starPathConfig.starName;

        CommonUtil.ReturnListToPool(nodeDetailAugmentItemUIs, item => item.gameObject);

        if (augmentSetItemUI != null)
        {
            ObjectPoolingEverything.ReturnToPool(GameConstants.nodeDetailAugmentItemUI, augmentSetItemUI.gameObject);
        }

        descriptionImage.sprite = starPathConfig.picture;
        descriptionImage.SetNativeSize();

        for (int i = 0; i < starPathConfig.unlockedAugments.Count; i++)
        {
            Augment augment = starPathConfig.unlockedAugments[i];

            NodeDetailAugmentItemUI nodeDetailAugmentItemUI =
                ObjectPoolingEverything.GetFromPool<NodeDetailAugmentItemUI>(GameConstants.nodeDetailAugmentItemUI);

            nodeDetailAugmentItemUI.transform.SetParent(augmentContainer);
            nodeDetailAugmentItemUI.transform.localScale = Vector3.one;

            nodeDetailAugmentItemUI.Init(augment);

            nodeDetailAugmentItemUIs.Add(nodeDetailAugmentItemUI);
        }

        augmentSetItemUI = ObjectPoolingEverything.GetFromPool<AugmentSetItemUI>(GameConstants.augmentSetItemUI);

        augmentSetItemUI.transform.SetParent(augmentSetBuffContainer);
        augmentSetItemUI.transform.localScale = Vector3.one;

        augmentSetItemUI.Init(starPathConfig.spaceObjectType);
    }

    public void ShowStarDetail(StarPathConfig starPathConfig)
    {
        Show();

        Init(starPathConfig);
    }
}
