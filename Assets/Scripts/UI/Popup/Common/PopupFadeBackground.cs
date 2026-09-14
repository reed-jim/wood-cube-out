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
        BasePopup.popupShowEvent += ShowPopup;
        BasePopup.popupHideEvent += TryClosePopup;

        fadeBackground.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        BasePopup.popupShowEvent -= ShowPopup;
        BasePopup.popupHideEvent -= TryClosePopup;

        CommonUtil.StopTween(_transitionTween);
    }

    private void ShowPopup()
    {
        fadeBackground.gameObject.SetActive(true);

        PlaceBeforeDeepestActivePopup(gameObject);

        CommonUtil.StopTween(_transitionTween);

        _transitionTween = Tween.Alpha(fadeBackground, 0.95f, duration: transitionDuration);
    }

    private void TryClosePopup()
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

        ClosePopup();
    }

    public void ClosePopup()
    {
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
        {
            newIndex--;
        }

        target.transform.SetSiblingIndex(newIndex);
    }
}
