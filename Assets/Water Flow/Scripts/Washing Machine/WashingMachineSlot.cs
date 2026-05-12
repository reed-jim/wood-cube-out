using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class WashingMachineSlot : MonoBehaviour
{
    public SpriteRenderer lockSprite;

    public Button unlockButton;

    public WashingMachineItem washingMachineItem;
    public bool isLocked;

    public static event Action<BoosterType> showBuyBoosterPopupEvent;

    void Awake()
    {
        BuyBoosterPopup.unlockScrewBoxEvent += OnUnlockBought;

        unlockButton.onClick.AddListener(ClickButtonUnlockByAds);

        lockSprite.gameObject.SetActive(isLocked);
    }

    void OnDestroy()
    {
        BuyBoosterPopup.unlockScrewBoxEvent -= OnUnlockBought;
    }

    private void ClickButtonUnlockByAds()
    {
        if (isLocked)
        {
            GamePersistentVariable.currentWashingMachineSlotToUnlock = this;

            showBuyBoosterPopupEvent?.Invoke(BoosterType.UnlockScrewBox);
        }
    }

    public void Unlock()
    {
        Tween.Alpha(lockSprite, 0, duration: 0.3f)
        .OnComplete(() =>
        {
            lockSprite.gameObject.SetActive(false);

            isLocked = false;
        });
    }

    public void OnUnlockBought()
    {
        if (this == GamePersistentVariable.currentWashingMachineSlotToUnlock)
        {
            Unlock();
        }
    }
}
