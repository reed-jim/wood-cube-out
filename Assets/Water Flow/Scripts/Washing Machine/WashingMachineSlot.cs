using System;
using NamPhuThuy.GamePlay;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class WashingMachineSlot : MonoBehaviour
{
    public SpriteRenderer lockSprite;

    public Button unlockButton;
    public Image unlockButtonIcon;

    public WashingMachineItem washingMachineItem;
    public bool isLocked;

    public static event Action<BoosterType> showBuyBoosterPopupEvent;

    void Awake()
    {
        BuyBoosterPopup.unlockScrewBoxEvent += OnUnlockBought;
        LevelLoader.startLevelEvent += OnLevelLoaded;

        unlockButton.onClick.AddListener(ClickButtonUnlockByAds);

        lockSprite.gameObject.SetActive(isLocked);
        unlockButton.gameObject.SetActive(isLocked);
    }

    void OnDestroy()
    {
        BuyBoosterPopup.unlockScrewBoxEvent -= OnUnlockBought;
        LevelLoader.startLevelEvent -= OnLevelLoaded;
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
        SaferioTween.AlphaHide(unlockButtonIcon);
        Tween.Alpha(lockSprite, 0, duration: 0.3f)
        .OnComplete(() =>
        {
            lockSprite.gameObject.SetActive(false);
            unlockButton.gameObject.SetActive(false);

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

    private void OnLevelLoaded()
    {
        unlockButtonIcon.color = Color.white;

        washingMachineItem = null;
    }
}
