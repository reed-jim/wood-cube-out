using System;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class GrillUI : MonoBehaviour
{
    [SerializeField] private BaseShelf grill;

    [SerializeField] private Button unlockButton;

    public static event Action<BoosterType> showBuyBoosterPopupEvent;

    void Awake()
    {
        BuyBoosterPopup.unlockScrewBoxEvent += Unlock;

        unlockButton.onClick.AddListener(ClickButtonUnlockByAds);

        unlockButton.gameObject.SetActive(grill.isLocked);
    }

    void OnDestroy()
    {
        BuyBoosterPopup.unlockScrewBoxEvent -= Unlock;
    }

    private void ClickButtonUnlockByAds()
    {
        GamePersistentVariable.currentToUnlockGrillInstanceId = gameObject.GetInstanceID();

        showBuyBoosterPopupEvent?.Invoke(BoosterType.UnlockScrewBox);
    }

    private void Unlock()
    {
        if (gameObject.GetInstanceID() != GamePersistentVariable.currentToUnlockGrillInstanceId)
        {
            return;
        }

        SaferioTween.ScaleDownWithPunch(unlockButton.transform, onCompleteAction: () =>
        {
            unlockButton.gameObject.SetActive(false);

            grill.isLocked = false;
        });
    }
}
