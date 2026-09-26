using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngameEventItemUI : MonoBehaviour
{
    [SerializeField] private RectTransform container;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text description;
    [SerializeField] private Button selectButton;
    [SerializeField] private Button removeButton;

    private IngameEventDataItem ingameEventDataItem;

    public static event Action<IngameEventItemUI> removeIngameEventItemUIEvent;

    void Awake()
    {
        selectButton.onClick.AddListener(Select);
        removeButton.onClick.AddListener(Remove);
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

    private void Remove()
    {
        IngameEventManager.Ins.RemoveEvent(ingameEventDataItem);

        ObjectPoolingEverything.ReturnToPool(GameConstants.ingameEventItemUI, gameObject);

        removeIngameEventItemUIEvent?.Invoke(this);
    }
}
