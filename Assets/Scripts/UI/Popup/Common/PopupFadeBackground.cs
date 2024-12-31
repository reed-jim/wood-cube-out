using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class PopupFadeBackground : MonoBehaviour
{
    [SerializeField] private Image fadeBackground;

    [Header("CUSTOMIZE")]
    [SerializeField] private float transitionDuration;

    void Awake()
    {
        BasePopup.popupShowEvent += OnPopupShow;
        BasePopup.popupHideEvent += OnPopupClose;

        fadeBackground.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        BasePopup.popupShowEvent -= OnPopupShow;
        BasePopup.popupHideEvent -= OnPopupClose;
    }

    private void OnPopupShow()
    {
        fadeBackground.gameObject.SetActive(true);

        Tween.Alpha(fadeBackground, 0.9f, duration: transitionDuration);
    }

    private void OnPopupClose()
    {
        Tween.Alpha(fadeBackground, 0, duration: transitionDuration).OnComplete(() =>
        {
            fadeBackground.gameObject.SetActive(false);
        });
    }
}
