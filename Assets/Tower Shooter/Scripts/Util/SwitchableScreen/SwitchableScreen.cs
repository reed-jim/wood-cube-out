using PrimeTween;
using UnityEngine;

public enum SwitchableScreenType
{
    Voyage,
    Leadership,
    IngameEvent,
    Research,
    TutorialSelection
}

// 1. Non-generic base class handling core UI and switching behavior
public class SwitchableScreen : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] protected CanvasGroup canvasGroup;
    [SerializeField] protected RectTransform container;

    [Header("Customize")]
    public SwitchableScreenType switchableScreenType;
    public bool isDisableGameObjectOnHiding = true;

    protected virtual void Awake()
    {
        SwitchableScreenButton.switchScreenEvent += OnScreenSwitch;
    }

    protected virtual void OnDestroy()
    {
        SwitchableScreenButton.switchScreenEvent -= OnScreenSwitch;
    }

    protected virtual void Show()
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

    protected virtual void Hide()
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

    private void OnScreenSwitch(SwitchableScreenType type)
    {
        if (type == switchableScreenType)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }
}

// 2. Generic intermediate class that adds Singleton behavior to the screen
public class SwitchableScreen<T> : SwitchableScreen where T : SwitchableScreen<T>
{
    public static T Ins { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        if (Ins != null && Ins != this)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
        else
        {
            Ins = this as T;
        }

        MoreActionOnAwake();
    }

    protected virtual void MoreActionOnAwake()
    {
    }
}