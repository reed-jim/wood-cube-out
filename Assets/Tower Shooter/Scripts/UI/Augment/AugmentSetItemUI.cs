using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AugmentSetItemUI : MonoBehaviour
{
    public TMP_Text setNameText;
    public TMP_Text setCountText;
    public RectTransform buffContainer;

    public SpaceObjectType spaceObjectType;

    private List<AugmentSetBuffItemUI> augmentSetBuffItemUIs = new List<AugmentSetBuffItemUI>();

    public AugmentSetBuffConfigContainer augmentSetBuffConfigContainer;

    public void Init(SpaceObjectType spaceObjectType)
    {
        int totalSetItem = AugmentManager.Ins.numAugmentOfSpaceObjectType[spaceObjectType];
        int augmentActive = AugmentManager.Ins.augmentOfSameSpaceObjectType[spaceObjectType].Count;

        setNameText.text = $"{spaceObjectType}";
        // setCountText.text = $"{augmentActive}/{totalSetItem}";

        this.spaceObjectType = spaceObjectType;

        for (int i = 0; i < augmentSetBuffItemUIs.Count; i++)
        {
            ObjectPoolingEverything.ReturnToPool(GameConstants.augmentSetBuffItemUI, augmentSetBuffItemUIs[i].gameObject);
        }

        for (int i = 0; i < augmentSetBuffConfigContainer.items.Count; i++)
        {
            AugmentSetBuffConfig augmentSetBuffConfig = augmentSetBuffConfigContainer.items[i];

            if (augmentSetBuffConfig.spaceObjectType != spaceObjectType)
            {
                continue;
            }

            AugmentSetBuffItemUI augmentSetBuffItemUI = ObjectPoolingEverything.GetFromPool<AugmentSetBuffItemUI>(GameConstants.augmentSetBuffItemUI);

            augmentSetBuffItemUI.transform.SetParent(buffContainer);
            augmentSetBuffItemUI.transform.localScale = Vector3.one;

            augmentSetBuffItemUI.Init(augmentSetBuffConfig, augmentActive);

            augmentSetBuffItemUIs.Add(augmentSetBuffItemUI);
        }
    }
}
