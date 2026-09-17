using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardItemUI : MonoBehaviour
{
    public RectTransform container;
    public Image icon;
    public TMP_Text valueText;

    public SerializableDictionary<ResourceType, Sprite> iconOfResources;

    public void Init(ResourceDataItem rewardData)
    {
        icon.sprite = iconOfResources[rewardData.resourceType];
        valueText.text = $"{rewardData.value}";
    }
}
