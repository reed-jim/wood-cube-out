using PrimeTween;
using UnityEngine;

public enum SwitchableScreenType
{
    Voyage,
    Leadership,
    IngameEvent
}

public class SwitchableScreen : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] protected CanvasGroup canvasGroup;
    [SerializeField] protected RectTransform container;

    [Header("Customize")]
    public SwitchableScreenType switchableScreenType;
    public bool isDisableGameObjectOnHiding = true;

    void Awake()
    {
        SwitchableScreenButton.switchScreenEvent += OnScreenSwitch;
    }

    void OnDestroy()
    {
        SwitchableScreenButton.switchScreenEvent -= OnScreenSwitch;
    }

    private void Show()
    {
        if (isDisableGameObjectOnHiding)
        {
            container.gameObject.SetActive(true);
        }

        canvasGroup.alpha = 1;

        float initialPositionX = container.localPosition.x;

        container.localPosition = new Vector3(-container.sizeDelta.x, 0);

        Tween.LocalPositionX(container, initialPositionX, duration: 0.3f);
    }

    private void Hide()
    {
        if (!canvasGroup.gameObject.activeSelf)
        {
            return;
        }

        Tween.Alpha(canvasGroup, 0, duration: 0.3f)
        .OnComplete(() =>
        {
            if (isDisableGameObjectOnHiding)
            {
                container.gameObject.SetActive(false);
            }
        });
    }

    private void OnScreenSwitch(SwitchableScreenType switchableScreenType)
    {
        if (switchableScreenType == this.switchableScreenType)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }
}
