using UnityEngine;
using UnityEngine.UI;

public class BuyBoosterPopup : BasePopup
{
    [SerializeField] private Button buyWithDiamondButton;

    [SerializeField] private UserResourcesObserver userResourcesObserver;

    protected override void MoreActionInAwake()
    {
        if (userResourcesObserver.UserResources.DiamondQuantity == 0)
        {
            buyWithDiamondButton.interactable = false;
        }
        else
        {
            buyWithDiamondButton.interactable = true;
        }
    }
}
