using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class IngameEventScreen : MonoBehaviour
{
    public RectTransform container;
    public RectTransform itemContainer;

    private List<IngameEventItemUI> ingameEventItemUIs = new List<IngameEventItemUI>();

    private void Init(IngameEventData ingameEventData)
    {
        CommonUtil.ReturnListToPool(ingameEventItemUIs, item => item.gameObject);

        for (int i = 0; i < ingameEventData.ingameEventDataItems.Count; i++)
        {
            IngameEventDataItem ingameEventDataItem = ingameEventData.ingameEventDataItems[i];

            IngameEventItemUI ingameEventItemUI = ObjectPoolingEverything.GetFromPool<IngameEventItemUI>(GameConstants.ingameEventItemUI);

            CommonUtil.SetParentAndResetScale(ingameEventItemUI.transform, itemContainer);

            ingameEventItemUI.Init(ingameEventDataItem);

            ingameEventItemUIs.Add(ingameEventItemUI);
        }
    }

    public void Show(IngameEventData ingameEventData)
    {
        container.gameObject.SetActive(true);
        container.localPosition = new Vector3(0, -1.5f * container.sizeDelta.y);

        Tween.LocalPositionY(container, -70, duration: 0.3f);

        Init(ingameEventData);
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
