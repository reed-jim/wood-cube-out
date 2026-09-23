using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngameEventItemUI : MonoBehaviour
{
    [SerializeField] private RectTransform container;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text description;
    [SerializeField] private Button selectButton;

    private IngameEventDataItem ingameEventDataItem;

    void Awake()
    {
        selectButton.onClick.AddListener(Select);
    }

    public void Init(IngameEventDataItem ingameEventDataItem)
    {
        titleText.text = ingameEventDataItem.title;
        description.text = ingameEventDataItem.description;

        this.ingameEventDataItem = ingameEventDataItem;

        if (ingameEventDataItem.eventType == IngameEventType.ElectionComplete)
        {
            this.ingameEventDataItem = (ElectionCompleteEventDataItem)this.ingameEventDataItem;
        }
    }

    private void Select()
    {
        IngameEventManager.Ins.ShowIngameEventDetail(ingameEventDataItem.eventType, ingameEventDataItem.GetParams());
    }
}
