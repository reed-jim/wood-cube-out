using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseTile : MonoBehaviour
{
    [SerializeField] private Button selectButton;
    [SerializeField] private Image icon;

    [SerializeField] private Sprite[] iconPool;



    [SerializeField] private ItemCategory itemCategory;
    [SerializeField] private bool isDone;

    public ItemCategory ItemCategory
    {
        get => itemCategory;
        set => itemCategory = value;
    }

    public bool IsDone
    {
        get => isDone;
        set => isDone = value;
    }

    void Awake()
    {
        selectButton.onClick.AddListener(Select);
    }

    public void Init()
    {
        itemCategory = CommonUtil.GetRandomEnumValue<ItemCategory>();
        icon.sprite = iconPool[(int)itemCategory];
    }

    private void Select()
    {
        TileManager.Ins.MoveTileToSlot(this);
    }
}
