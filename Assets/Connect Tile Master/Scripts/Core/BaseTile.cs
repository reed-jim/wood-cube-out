using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseTile : MonoBehaviour
{
    [SerializeField] private RectTransform container;
    [SerializeField] private Button selectButton;
    [SerializeField] private Image containerImage;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text category;

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

    public float LeftBound => container.localPosition.x - 0.5f * container.sizeDelta.x;
    public float RightBound => container.localPosition.x + 0.5f * container.sizeDelta.x;
    public float TopBound => container.localPosition.y + 0.5f * container.sizeDelta.y;
    public float BottomBound => container.localPosition.y - 0.5f * container.sizeDelta.y;

    void Awake()
    {
        selectButton.onClick.AddListener(Select);
    }

    public void Init(ItemCategory itemCategory, Sprite iconSprite)
    {
        this.itemCategory = itemCategory;
        icon.sprite = iconSprite;
        category.text = itemCategory.ToString();

        containerImage.color = containerImage.color.ChangeAlpha(1);

        UIUtil.SetSizeKeepRatioY(icon, 0.8f * container.sizeDelta.x);

        isDone = false;
    }

    private void Select()
    {
        Tween.Alpha(containerImage, 0, duration: 0.3f);

        transform.SetAsLastSibling();

        TileManager.Ins.MoveTileToSlot(this);
    }
}
