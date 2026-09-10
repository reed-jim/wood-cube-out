using TMPro;
using UnityEngine;

public class AugmentSetItemUI : MonoBehaviour
{
    public TMP_Text setNameText;
    public TMP_Text setCountText;

    public SpaceObjectType spaceObjectType;

    public AugmentSetContainer augmentSetContainer;

    public void Init(Augment augment)
    {
        spaceObjectType = augment.spaceObjectType;

        int totalSetItem = augmentSetContainer.items[spaceObjectType].Count;
        int augmentActive = AugmentManager.Ins.augmentOfSameSpaceObjectType[spaceObjectType].Count;

        setNameText.text = $"{augment.spaceObjectType}";
        setCountText.text = $"{augmentActive}/{totalSetItem}";

        spaceObjectType = augment.spaceObjectType;
    }
}
