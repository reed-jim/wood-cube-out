using NamPhuThuy.Common;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class PopupFadeBackground : MonoBehaviour
{
    [SerializeField] private Image fadeBackground;

    [Header("CUSTOMIZE")]
    [SerializeField] private float transitionDuration;

    private Tween _transitionTween;

    void Awake()
    {
        BasePopup.popupShowEvent += OnPopupShow;
        BasePopup.popupHideEvent += OnPopupClose;
        RevivePopup.OnPeekPicture += OnPopupClose;
        RevivePopup.OnStopPeekPicture += OnPopupShow;

        fadeBackground.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        BasePopup.popupShowEvent -= OnPopupShow;
        BasePopup.popupHideEvent -= OnPopupClose;
        RevivePopup.OnPeekPicture -= OnPopupClose;
        RevivePopup.OnStopPeekPicture -= OnPopupShow;

        CommonUtil.StopTween(_transitionTween);
    }

    private void OnPopupShow()
    {
        fadeBackground.gameObject.SetActive(true);

        // Set position in hierarchy to near last
        /*var parent = transform.parent;
        if (parent != null && parent.childCount >= 2)
        {
            int nearLastIndex = parent.childCount - 2;
            transform.SetSiblingIndex(nearLastIndex);
        }*/
        
        CommonUtil.StopTween(_transitionTween);

        _transitionTween = Tween.Alpha(fadeBackground, 0.95f, duration: transitionDuration);
    }

    private void OnPopupClose()
    {
        DebugLogger.Log();
        Transform popupContainer = transform.parent;

        int numActivePopup = 0;

        for (int i = 1; i < popupContainer.childCount; i++)
        {
            if (popupContainer.GetChild(i).gameObject.activeSelf)
            {
                numActivePopup++;
            }
        }

        if (numActivePopup > 1)
        {
            DebugLogger.Log(message:$"numActivePopup: {numActivePopup}, skip disable");
            return;
        }

        CommonUtil.StopTween(_transitionTween);

        _transitionTween = Tween.Alpha(fadeBackground, 0, duration: transitionDuration).OnComplete(() =>
        {
            DebugLogger.Log(message:$"Disable");
            fadeBackground.gameObject.SetActive(false);
        });
    }
}
