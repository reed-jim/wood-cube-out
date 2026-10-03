using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmPopup : BasePopup
{
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button denyButton;

    private Action acceptAction;
    private Action denyAction;

    protected override void RegisterMoreEvent()
    {
        base.RegisterMoreEvent();

        BasePopup.showConfirmPopupEvent += Init;
        ElectionPopup.showConfirmPopupEvent += Init;
        LeadershipCharacterCreationUI.showConfirmPopupEvent += Init;
        IdleGameSettingPopup.showConfirmPopupEvent += Init;

        acceptButton.onClick.AddListener(Accept);
        denyButton.onClick.AddListener(Deny);
    }

    protected override void UnregisterMoreEvent()
    {
        base.UnregisterMoreEvent();

        BasePopup.showConfirmPopupEvent -= Init;
        ElectionPopup.showConfirmPopupEvent -= Init;
        LeadershipCharacterCreationUI.showConfirmPopupEvent -= Init;
        IdleGameSettingPopup.showConfirmPopupEvent -= Init;
    }

    private void Init(string description, Action acceptAction)
    {
        Show();

        descriptionText.text = description;

        this.acceptAction = acceptAction;
    }

    private void Init(string description, Action acceptAction, Action denyAction)
    {
        Show();

        descriptionText.text = description;

        this.acceptAction = acceptAction;
        this.denyAction = denyAction;
    }

    private void Accept()
    {
        acceptAction?.Invoke();

        Hide();
    }

    private void Deny()
    {
        denyAction?.Invoke();

        Hide();
    }
}
