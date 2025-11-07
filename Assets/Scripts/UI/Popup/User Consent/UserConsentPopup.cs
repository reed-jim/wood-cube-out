using UnityEngine;
using UnityEngine.UI;

public class UserConsentPopup : BasePopup
{
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button declineButton;

    protected override void RegisterMoreEvent()
    {
        acceptButton.onClick.AddListener(Accept);
        declineButton.onClick.AddListener(Decline);
    }

    private void Accept()
    {
        DataUtility.Save("User Consent", true);
        DataUtility.Save("Is User Customized Consent", true);

        Hide(onCompletedAction: () =>
        {
            ShowNotification("Successful", "Your change will apply next time you open the game");
        });
    }

    private void Decline()
    {
        DataUtility.Save("User Consent", false);
        DataUtility.Save("Is User Customized Consent", true);

        Hide(onCompletedAction: () =>
        {
            ShowNotification("Successful", "Your change will apply next time you open the game");
        });
    }
}
