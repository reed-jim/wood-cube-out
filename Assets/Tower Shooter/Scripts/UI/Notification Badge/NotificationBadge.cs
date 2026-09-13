using PrimeTween;
using TMPro;
using UnityEngine;

public class NotificationBadge : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    // public TMP_Text titleText;
    public TMP_Text descriptionText;

    public ScaleTransition scaleTransition;

    [Header("Customize")]
    public float existDuration;

    private Tween hideTween;

    public void ShowNotification(string title, string description)
    {
        // titleText.text = title;
        descriptionText.text = description;

        if (hideTween.isAlive)
        {
            hideTween.Stop();
        }

        scaleTransition.Show(onCompletedAction: null);

        hideTween = Tween.Delay(existDuration).OnComplete(() =>
        {
            scaleTransition.Hide();
        });
    }
}
