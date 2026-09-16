using TMPro;
using UnityEngine;

public class StarDetailPopup : BasePopup
{
    public TMP_Text title;
    public TMP_Text description;

    public RectTransform augmentContainer;
    public RectTransform augmentSetBuffContainer;

    private StarPathConfig starPathConfig;
    private AugmentSetBuffConfig augmentSetBuffConfig;

    public void Init(StarPathConfig starPathConfig)
    {
        title.text = starPathConfig.starName;

        for (int i = 0; i < starPathConfig.unlockedAugments.Count; i++)
        {
            Augment augment = starPathConfig.unlockedAugments[i];

            NodeDetailAugmentItemUI nodeDetailAugmentItemUI =
                ObjectPoolingEverything.GetFromPool<NodeDetailAugmentItemUI>(GameConstants.nodeDetailAugmentItemUI);
        }
    }
}
