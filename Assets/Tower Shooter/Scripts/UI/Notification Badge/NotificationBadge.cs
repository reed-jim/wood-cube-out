using System;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // Required for Image

public class NotificationBadge : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public TMP_Text descriptionText;
    public ScaleTransition scaleTransition;

    [Header("Shader Effect")]
    public Image effectImage; // Assign the separate Image containing your shader material here
    private Material effectMaterialInstance;
    private Tween progressTween;

    [Header("Customize")]
    public float existDuration;
    public float effectDuration = 0.5f; // Duration for the shockwave/ember progress

    private Tween hideTween;

    private void Awake()
    {
        if (effectImage != null)
        {
            // Instantiate material so it doesn't modify the shared asset globally
            effectMaterialInstance = new Material(effectImage.material);
            effectImage.material = effectMaterialInstance;

            effectImage.gameObject.SetActive(false);
        }
    }

    public void ShowNotification(string title, string description, Action onStartHidden = null)
    {
        // Standard show without progress effect
        ShowNotificationInternal(description, onStartHidden);
    }

    public void ShowNotification(string description, Action onStartHidden = null)
    {
        // Standard show without progress effect
        ShowNotificationInternal(description, onStartHidden);
    }

    // New variant that plays the shader effect using progress
    public void ShowNotification(string description, float customProgressDuration, Action onStartHidden = null)
    {
        ShowNotificationInternal(description, onStartHidden);
        PlayEffect(customProgressDuration);
    }

    public void ShowNotification(string description, Action onStartHidden, bool playEffect)
    {
        ShowNotificationInternal(description, onStartHidden, playEffect);
    }

    private void ShowNotificationInternal(string description, Action onStartHidden, bool playEffect = false)
    {
        descriptionText.text = description;

        if (hideTween.isAlive)
        {
            hideTween.Stop();
        }

        effectImage.gameObject.SetActive(false);

        scaleTransition.Show(onCompletedAction: () =>
        {
            if (playEffect)
            {
                PlayEffect(effectDuration);
            }
        });

        hideTween = Tween.Delay(existDuration).OnComplete(() =>
        {
            scaleTransition.Hide();
            onStartHidden?.Invoke();
        });
    }

    private void PlayEffect(float duration)
    {
        if (progressTween.isAlive)
        {
            progressTween.Stop();
        }

        effectImage.gameObject.SetActive(true);

        // Reset progress to 0
        effectImage.material.SetFloat("_Progress", 0f);

        // Tween progress from 0 to 1
        progressTween = Tween.Custom(0f, 1f, duration, (float val) =>
        {
            effectImage.material.SetFloat("_Progress", val);
        });
    }

    public void UpdateDescription(string description)
    {
        descriptionText.text = description;
    }
}