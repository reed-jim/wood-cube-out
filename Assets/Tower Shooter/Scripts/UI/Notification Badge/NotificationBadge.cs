using System;
using PrimeTween;
using Saferio.Util.SaferioTween;
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

        TryClearTMPSubmesh();

        if (hideTween.isAlive)
        {
            hideTween.Stop();
        }

        effectImage.gameObject.SetActive(false);

        if (playEffect)
        {
            ResetEffect();
        }

        Show(onComplete: () =>
        {
            if (playEffect)
            {
                PlayEffect(effectDuration);
            }

            SaferioTween.Punch(descriptionText.transform, effectScale: 1.02f, duration: 0.15f);
        });

        hideTween = Tween.Delay(existDuration).OnComplete(() =>
        {
            Hide();
            onStartHidden?.Invoke();
        });
    }

    private void Show(Action onComplete)
    {
        canvasGroup.alpha = 0;
        descriptionText.color = ColorUtil.WithAlpha(descriptionText.color, 0);

        Tween.Alpha(canvasGroup, 1, duration: 0.3f)
        .OnComplete(() =>
        {
            descriptionText.color = ColorUtil.WithAlpha(descriptionText.color, 1);

            descriptionText.transform.localScale = 1.5f * Vector3.one;

            Tween.Scale(descriptionText.transform, 1, duration: 0.2f).OnComplete(onComplete);
        });
    }

    private void Hide()
    {
        Tween.Alpha(canvasGroup, 0, duration: 0.3f)
        .OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }

    private void ResetEffect()
    {
        effectImage.gameObject.SetActive(true);

        // Reset progress to 0
        effectImage.material.SetFloat("_Progress", 0f);
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

        TryClearTMPSubmesh();
    }

    private void TryClearTMPSubmesh()
    {
        TMP_SubMeshUI[] submeshes = descriptionText.GetComponentsInChildren<TMP_SubMeshUI>();

        foreach (var submesh in submeshes)
        {
            DestroyImmediate(submesh.gameObject);
        }
    }
}