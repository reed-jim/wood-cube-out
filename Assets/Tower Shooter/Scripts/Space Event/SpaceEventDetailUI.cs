using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SpaceEventDetailUI : BasePopup
{
    [Header("UI Components")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text remainingTimeText;
    [SerializeField] private RectTransform buffContainer;

    private List<StatItemUI> statItemUIs = new List<StatItemUI>();
    private ActiveSpaceEvent currentEvent;
    private bool isExpired;

    protected override void RegisterMoreEvent()
    {
        base.RegisterMoreEvent();

        SpaceEventItemUI.showSpaceEventDetailEvent += ShowDetail;
    }

    protected override void UnregisterMoreEvent()
    {
        base.UnregisterMoreEvent();

        SpaceEventItemUI.showSpaceEventDetailEvent -= ShowDetail;
    }

    private void Update()
    {
        if (currentEvent != null)
        {
            UpdateDynamicUI();
        }
    }

    public void ShowDetail(ActiveSpaceEvent activeEvent)
    {
        Show();

        currentEvent = activeEvent;
        isExpired = false;

        iconImage.sprite = activeEvent.Config.eventIcon;
        titleText.text = activeEvent.Config.eventTitle;
        descriptionText.text = activeEvent.Config.eventDescription;

        CommonUtil.ReturnListToPool(statItemUIs, item => item.gameObject);

        for (int i = 0; i < activeEvent.Config.buffs.Count; i++)
        {
            StatModifierData statModifierData = activeEvent.Config.buffs[i];
            StatItemUI statItemUI = ObjectPoolingEverything.GetFromPool<StatItemUI>(GameConstants.statItemUI);

            statItemUI.Init(statModifierData.statType, statModifierData.modType, statModifierData.value);

            CommonUtil.SetParentAndResetScale(statItemUI.transform, buffContainer);

            statItemUIs.Add(statItemUI);
        }

        UpdateDynamicUI();
    }

    private void UpdateDynamicUI()
    {
        if (currentEvent == null) return;

        float remaining = Mathf.Max(0, currentEvent.RemainingTime);

        if (isExpired)
        {
            remainingTimeText.text = $"Expired!";
        }
        else
        {
            int mins = Mathf.FloorToInt(remaining / 60f);
            int secs = Mathf.FloorToInt(remaining % 60f);
            remainingTimeText.text = $"Expires in: {mins:D2}:{secs:D2}";
        }
    }
}
