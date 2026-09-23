using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class LeadershipPeriodUI : MonoBehaviour
{
    public RectTransform container;
    public RectTransform itemContainer;
    [SerializeField] private Button closeButton;

    private List<LeadershipPeriodItemUI> leadershipPeriodItemUIs = new List<LeadershipPeriodItemUI>();

    void Awake()
    {
        LeadershipManager.openLeadershipPeriodScreenEvent += Show;

        closeButton.onClick.AddListener(Hide);
    }

    void OnDestroy()
    {
        LeadershipManager.openLeadershipPeriodScreenEvent -= Show;
    }

    public void Init()
    {
        CommonUtil.ReturnListToPool(leadershipPeriodItemUIs, item => item.gameObject);

        List<LeadershipPeriodData> leadershipPeriodDataItems = LeadershipManager.Ins.leadershipData.leadershipPeriodDataItems;

        for (int i = 0; i < leadershipPeriodDataItems.Count; i++)
        {
            LeadershipPeriodData leadershipPeriodData = leadershipPeriodDataItems[i];

            LeadershipPeriodItemUI leadershipPeriodItemUI =
                ObjectPoolingEverything.GetFromPool<LeadershipPeriodItemUI>(GameConstants.leadershipPeriodItemUI);

            leadershipPeriodItemUI.Init(leadershipPeriodData);

            CommonUtil.SetParentAndResetScale(leadershipPeriodItemUI.transform, itemContainer);
            leadershipPeriodItemUI.transform.localScale = 0.5f * Vector3.one;

            leadershipPeriodItemUIs.Add(leadershipPeriodItemUI);
        }
    }

    private void Show()
    {
        container.gameObject.SetActive(true);
        container.localPosition = new Vector3(0, -1.5f * container.sizeDelta.y);

        Tween.LocalPositionY(container, -70, duration: 0.3f);

        Init();
    }

    private void Hide()
    {
        Tween.LocalPositionY(container, -1.5f * container.sizeDelta.y, duration: 0.3f)
        .OnComplete(() =>
        {
            container.gameObject.SetActive(false);
        });
    }
}
