using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrestigeUI : BasePopup
{
    [SerializeField] private RectTransform buffContainer;
    [SerializeField] private TMP_Text prestigeCountText;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button denyButton;

    private List<StatItemUI> statItemUIs = new List<StatItemUI>();

    protected override void RegisterMoreEvent()
    {
        base.RegisterMoreEvent();

        acceptButton.onClick.AddListener(AcceptPrestige);
        denyButton.onClick.AddListener(DenyPrestige);
    }

    [ContextMenu("ShowPrestigePopup")]
    private void ShowPrestigePopup()
    {
        Show();

        Init();
    }

    private void Init()
    {
        List<StatModifier> permanentStatModifiers = PrestigeManager.Ins.GenerateRandomPermanentBuffs();

        CommonUtil.ReturnListToPool(statItemUIs, item => item.gameObject);

        for (int i = 0; i < permanentStatModifiers.Count; i++)
        {
            StatModifier statModifier = permanentStatModifiers[i];
            StatItemUI statItemUI = ObjectPoolingEverything.GetFromPool<StatItemUI>(GameConstants.statItemUI);

            CommonUtil.SetParentAndResetScale(statItemUI.transform, buffContainer);

            statItemUI.Init(statModifier);

            statItemUIs.Add(statItemUI);
        }

        prestigeCountText.text = $"Prestige Count: <color={DynamicThemeManager.Ins.PrimaryHex}>{PrestigeManager.Ins.GetPrestigeCount()}";
    }

    private void AcceptPrestige()
    {
        ShowConfirmPopup("This will reset the game progress, are you sure?", onAcceptAction: () =>
        {
            PrestigeManager.Ins.AcceptPrestige();

            Hide();
        });
    }

    private void DenyPrestige()
    {
        Hide();
    }
}
