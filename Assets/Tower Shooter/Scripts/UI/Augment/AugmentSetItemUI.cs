using TMPro;
using UnityEngine;

public class AugmentSetItemUI : MonoBehaviour
{
    public TMP_Text setNameText;
    public TMP_Text setCountText;

    public SpaceObjectType spaceObjectType;

    public void Init(SpaceObjectType spaceObjectType)
    {
        int totalSetItem = AugmentManager.Ins.numAugmentOfSpaceObjectType[spaceObjectType];
        int augmentActive = AugmentManager.Ins.augmentOfSameSpaceObjectType[spaceObjectType].Count;

        setNameText.text = $"{spaceObjectType}";
        setCountText.text = $"{augmentActive}/{totalSetItem}";

        this.spaceObjectType = spaceObjectType;
    }
}
