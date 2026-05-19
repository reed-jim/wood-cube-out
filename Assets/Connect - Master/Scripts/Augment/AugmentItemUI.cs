using Coffee.UIEffects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AugmentItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text description;
    [SerializeField] private Button selectButton;
    [SerializeField] private UIEffect effectUI;

    private AugmentItem _augmentItem;

    void Awake()
    {
        selectButton.onClick.AddListener(Select);
    }

    public void Init(AugmentItem augmentItem)
    {
        nameText.text = $"{augmentItem.name}";
        description.text = augmentItem.GetDescription();
        effectUI.edgeColor = InventoryDataContainer.GetTierColor((InventoryItemTier)augmentItem.level);

        _augmentItem = augmentItem;
    }

    private void Select()
    {

    }
}
