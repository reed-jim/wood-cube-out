using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class IngameEventScreen : MonoBehaviour
{
    public RectTransform container;
    public ScrollableLayoutGroup scrollableLayoutGroup;
    [SerializeField] private Button closeButton;

    [Header("Switchable Screen")]
    public SwitchableScreenType switchableScreenType;

    private List<IngameEventItemUI> ingameEventItemUIs = new List<IngameEventItemUI>();

    void Awake()
    {
        SwitchableScreenButton.switchScreenEvent += OnScreenSwitch;
        IngameEventItemUI.removeIngameEventItemUIEvent += OnItemRemoved;

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
    }

    public void RemoveItem(IngameEventItemUI ingameEventItemUI)
    {
        scrollableLayoutGroup.RemoveItemAt(ingameEventItemUIs.IndexOf(ingameEventItemUI));
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
