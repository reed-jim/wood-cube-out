using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseTile : MonoBehaviour
{
    [SerializeField] private Button selectButton;
    [SerializeField] private TMP_Text category;



    [SerializeField] private ItemCategory itemCategory;

    public ItemCategory ItemCategory
    {
        get => itemCategory;
        set => itemCategory = value;
    }

    void Awake()
    {
        selectButton.onClick.AddListener(Select);
    }

    public void Init()
    {
        itemCategory = CommonUtil.GetRandomEnumValue<ItemCategory>();
        category.text = $"{itemCategory.ToString()[0]}";
    }

    private void Select()
    {
        TileManager.Ins.MoveTileToSlot(this);
    }
}
