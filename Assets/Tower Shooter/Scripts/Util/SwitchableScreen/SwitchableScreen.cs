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
        container.gameObject.SetActive(true);

        canvasGroup.alpha = 1;

        float initialPositionY = container.localPosition.y;

        container.localPosition = new Vector3(0, -container.sizeDelta.y);

        Tween.LocalPositionY(container, initialPositionY, duration: 0.3f);
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
            container.gameObject.SetActive(false);
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
