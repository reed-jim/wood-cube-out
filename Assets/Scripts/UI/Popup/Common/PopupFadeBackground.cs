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

        PlaceBeforeDeepestActivePopup(gameObject);

        CommonUtil.StopTween(_transitionTween);

        _transitionTween = Tween.Alpha(fadeBackground, 0.95f, duration: transitionDuration);
    }

    private void OnPopupClose()
    {
        PlaceBeforeDeepestActivePopup(gameObject);

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
            return;
        }

        CommonUtil.StopTween(_transitionTween);

        _transitionTween = Tween.Alpha(fadeBackground, 0, duration: transitionDuration).OnComplete(() =>
        {
            fadeBackground.gameObject.SetActive(false);
        });
    }

    public void PlaceBeforeDeepestActivePopup(GameObject target)
    {
        if (target == null) return;
        Transform parent = target.transform.parent;
        if (parent == null) return;

        int deepestIndex = -1;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            if (child.gameObject.activeInHierarchy &&
                child.GetComponent<BasePopup>() != null)
            {
                deepestIndex = i;
            }
        }

        if (deepestIndex < 0)
        {
            return;
        }

        int newIndex = Mathf.Max(0, deepestIndex);

        if (target.transform.GetSiblingIndex() <= deepestIndex)
            newIndex--;

        target.transform.SetSiblingIndex(newIndex);
    }
}
