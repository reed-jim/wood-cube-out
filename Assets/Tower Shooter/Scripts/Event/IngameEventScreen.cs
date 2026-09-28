using System.Collections.Generic;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngameEventScreen : MonoBehaviour
{
    public RectTransform container;
    public ScrollableLayoutGroup scrollableLayoutGroup;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Button removeAllButton;
    [SerializeField] private Button closeButton;

    [Header("Switchable Screen")]
    public SwitchableScreenType switchableScreenType;

    private List<IngameEventItemUI> ingameEventItemUIs = new List<IngameEventItemUI>();

    void Awake()
    {
        SwitchableScreenButton.switchScreenEvent += OnScreenSwitch;
        IngameEventItemUI.removeIngameEventItemUIEvent += OnItemRemoved;

        removeAllButton.onClick.AddListener(RemoveAll);
        closeButton.onClick.AddListener(Hide);
    }

    void OnDestroy()
    {
        SwitchableScreenButton.switchScreenEvent -= OnScreenSwitch;
        IngameEventItemUI.removeIngameEventItemUIEvent -= OnItemRemoved;
    }

    private void Init(IngameEventData ingameEventData)
    {
        CommonUtil.ReturnListToPool(ingameEventItemUIs, item => item.gameObject);

        scrollableLayoutGroup.ClearAllItems();

        for (int i = 0; i < ingameEventData.ingameEventDataItems.Count; i++)
        {
            IngameEventDataItem ingameEventDataItem = ingameEventData.ingameEventDataItems[i];

            IngameEventItemUI ingameEventItemUI = ObjectPoolingEverything.GetFromPool<IngameEventItemUI>(GameConstants.ingameEventItemUI);

            scrollableLayoutGroup.AddItem(ingameEventItemUI.gameObject);

            ingameEventItemUI.Init(ingameEventDataItem);

            ingameEventItemUIs.Add(ingameEventItemUI);
        }

        int count = ingameEventData.ingameEventDataItems.Count;

        if (count > 1)
        {
            countText.text = $"There are {count} events";
        }
        else if (count == 1)
        {
            countText.text = $"There is {count} event";
        }
        else
        {
            countText.text = $"There is no event";
        }
    }

    public void RemoveItem(IngameEventItemUI ingameEventItemUI)
    {
        scrollableLayoutGroup.RemoveItemAt(ingameEventItemUIs.IndexOf(ingameEventItemUI));
    }

    public void RemoveAll()
    {
        CommonUtil.ReturnListToPool(ingameEventItemUIs, item => item.gameObject);

        scrollableLayoutGroup.ClearAllItems();
    }

    private void OnItemRemoved(IngameEventItemUI ingameEventItemUI)
    {
        RemoveItem(ingameEventItemUI);
    }

    private void OnScreenSwitch(SwitchableScreenType switchableScreenType)
    {
        if (switchableScreenType == this.switchableScreenType)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    public void Show()
    {
        container.gameObject.SetActive(true);
        container.localPosition = new Vector3(0, -1.5f * container.sizeDelta.y);

        Tween.LocalPositionY(container, -70, duration: 0.3f);

        Init(IngameEventManager.Ins.ingameEventData);
    }

    public void Hide()
    {
        Tween.LocalPositionY(container, -1.5f * container.sizeDelta.y, duration: 0.3f)
        .OnComplete(() =>
        {
            container.gameObject.SetActive(false);
        });
    }
}
