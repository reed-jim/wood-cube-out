using UnityEngine;
using UnityEngine.UI;

public class BaseTile : MonoBehaviour
{
    [SerializeField] private Button selectButton;



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

    private void Select()
    {
        TileManager.Ins.MoveTileToSlot(this);
    }
}
