using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;
using UnityEngine.UI;

public class FilterUI : MonoBehaviour
{
    public CanvasGroup contentCanvasGroup;
    public RectTransform background;
    public Image borderImage;

    public Button changeVisibilityButton;
    public Image changeVisibilityButtonIcon;

    public Sprite showFilterIcon;
    public Sprite hideFilterIcon;

    public bool isClosed;

    private Vector2 initialSize;
    private Vector2 initialPosition;

    private Tween contentCanvasGroupTween;
    private Tween borderImageTween;
    private Tween sizeTween;

    void Awake()
    {
        initialSize = background.sizeDelta;
        initialPosition = background.anchoredPosition;

        changeVisibilityButton.onClick.AddListener(ChangeVisibility);
    }

    private void ChangeVisibility()
    {
        if (isClosed)
        {
            Show();
        }
        else
        {
            Close();
        }
    }

    private void Show()
    {
        StopTween(contentCanvasGroupTween);
        StopTween(borderImageTween);
        StopTween(sizeTween);

        contentCanvasGroupTween = Tween.Alpha(contentCanvasGroup, 1, duration: 0.3f);
        borderImageTween = Tween.Alpha(borderImage, 1, duration: 0.3f);
        sizeTween = Tween.Custom(0, initialSize.x, duration: 0.3f, onValueChange: newVal =>
        {
            float ratio = newVal / initialSize.x;

            background.sizeDelta = ratio * initialSize;
            background.anchoredPosition = ratio * initialPosition;

            changeVisibilityButton.interactable = true;
            contentCanvasGroup.interactable = true;
        });

        changeVisibilityButtonIcon.sprite = hideFilterIcon;

        changeVisibilityButton.interactable = false;

        isClosed = false;
    }

    private void Close()
    {
        StopTween(contentCanvasGroupTween);
        StopTween(borderImageTween);
        StopTween(sizeTween);
        
        contentCanvasGroupTween = Tween.Alpha(contentCanvasGroup, 0, duration: 0.15f);
        borderImageTween = Tween.Alpha(borderImage, 0, duration: 0.15f);
        sizeTween = Tween.Custom(initialSize.x, 0, duration: 0.3f, onValueChange: newVal =>
        {
            float ratio = newVal / initialSize.x;

            background.sizeDelta = ratio * initialSize;
            background.anchoredPosition = ratio * initialPosition;

            changeVisibilityButton.interactable = true;
        });

        changeVisibilityButtonIcon.sprite = showFilterIcon;

        changeVisibilityButton.interactable = false;
        contentCanvasGroup.interactable = false;

        isClosed = true;
    }

    private void StopTween(Tween tween)
    {
        if (tween.isAlive)
        {
            tween.Stop();
        }
    }
}
