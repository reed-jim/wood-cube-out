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

    protected override void RegisterMoreEvent()
    {
        base.RegisterMoreEvent(); 

        ElectionPopup.showConfirmPopupEvent += Init;
        LeadershipCharacterCreationUI.showConfirmPopupEvent += Init;  

        acceptButton.onClick.AddListener(Accept);
        denyButton.onClick.AddListener(Deny);
    }

    protected override void UnregisterMoreEvent()
    {
        base.UnregisterMoreEvent();

        ElectionPopup.showConfirmPopupEvent -= Init;
        LeadershipCharacterCreationUI.showConfirmPopupEvent -= Init;
    }

    private void Init(string description, Action acceptAction)
    {
        Show();

        descriptionText.text = description;

        this.acceptAction = acceptAction;
    }

    private void Accept()
    {
        acceptAction?.Invoke();

        Hide();
    }

    private void Deny()
    {
        Hide();
    }
}
