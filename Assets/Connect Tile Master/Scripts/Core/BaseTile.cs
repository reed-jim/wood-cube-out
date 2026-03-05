using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseTile : MonoBehaviour
{
    [SerializeField] private RectTransform container;
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

    public void Init(ItemCategory itemCategory, Sprite iconSprite)
    {
        this.itemCategory = itemCategory;
        icon.sprite = iconSprite;

        UIUtil.SetSizeKeepRatioY(icon, 0.7f * container.sizeDelta.x);
    }

    private void Select()
    {
        TileManager.Ins.MoveTileToSlot(this);
    }
}
