using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class WashingMachineSlot : MonoBehaviour
{
    public SpriteRenderer lockSprite;

    public Button unlockButton;

    public WashingMachineItem washingMachineItem;
    public bool isLocked;

    void Awake()
    {
        unlockButton.onClick.AddListener(Unlock);

        lockSprite.gameObject.SetActive(isLocked);
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
}
