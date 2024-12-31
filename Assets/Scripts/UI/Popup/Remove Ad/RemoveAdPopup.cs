using System;
using UnityEngine;
using UnityEngine.UI;

public class RemoveAdPopup : BasePopup
{
    [SerializeField] private Button buyButton;

    public static event Action<string> buyRemoveAdEvent;

    protected override void RegisterMoreEvent()
    {
        buyButton.onClick.AddListener(BuyRemoveAd);
    }

    private void BuyRemoveAd()
    {
        buyRemoveAdEvent?.Invoke(GameConstants.REMOVE_AD_ID);
    }
}
