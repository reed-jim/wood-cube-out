using UnityEngine;
using UnityEngine.UI;

public class NoAdsLoadedPopup : BasePopup
{
    [SerializeField] private Button okButton;

    protected override void RegisterMoreEvent()
    {
        showAdNotLoadedPopupEvent += Show;

        okButton.onClick.AddListener(Hide);
    }

    protected override void UnregisterMoreEvent()
    {
        showAdNotLoadedPopupEvent -= Show;
    }
}
