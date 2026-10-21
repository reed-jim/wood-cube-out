using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

public class UnlockEffectController : Singleton<UnlockEffectController>
{
    [Header("UI References")]
    [SerializeField] private GameObject effectRootCanvasGroup;
    [SerializeField] private CanvasGroup backgroundDim;

    [Header("State Containers (Separate)")]
    [SerializeField] private RectTransform lockStateContainer;
    [SerializeField] private CanvasGroup lockCanvasGroup;
    [SerializeField] private RectTransform unlockStateContainer;
    [SerializeField] private CanvasGroup unlockCanvasGroup;
    [SerializeField] private Image unlockStateImage;
    [SerializeField] private TextMeshProUGUI unlockedFeatureText;

    [Header("Action & Shader")]
    [SerializeField] private Button continueButton;

    [Header("Cinematic Slow Pacing Settings")]
    [SerializeField] private float moveToCenterDuration = 1.4f;
    [SerializeField] private float unlockTransformDuration = 1.0f;
    [SerializeField] private Ease moveEase = Ease.OutQuart;

    [Header("Test")]
    [SerializeField] private RectTransform testElementToUnlock;

    private Action onContinueCallback;
    private RectTransform canvasRect;
    private Material effectMaterial;
    private Vector2 targetOriginalAnchoredPos;
    private bool isBusy;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        effectRootCanvasGroup.SetActive(false);
        continueButton.gameObject.SetActive(false);
        continueButton.onClick.AddListener(OnContinueClicked);

        if (unlockStateImage != null)
        {
            effectMaterial = new Material(unlockStateImage.material);
            unlockStateImage.material = effectMaterial;
        }
    }

    public void PlayUnlockEffect(RectTransform elementToUnlock, string featureName, Action onComplete = null)
    {
        if (isBusy)
        {
            return;
        }
        else
        {
            isBusy = true;
        }

        onContinueCallback = onComplete;
        effectRootCanvasGroup.SetActive(true);
        continueButton.gameObject.SetActive(false);

        // 1. Setup initial state: Lock State ON, Unlock State OFF
        lockStateContainer.gameObject.SetActive(true);
        lockCanvasGroup.alpha = 1f;
        lockStateContainer.localScale = Vector3.one;

        unlockStateContainer.gameObject.SetActive(false);
        unlockCanvasGroup.alpha = 0f;
        unlockStateContainer.localScale = Vector3.one * 0.9f;
        unlockedFeatureText.text = featureName;

        if (effectMaterial != null)
        {
            effectMaterial.SetFloat("_SweepPosition", -0.5f);
            effectMaterial.SetFloat("_UnlockProgress", 0.0f); // Initialize locked
            effectMaterial.SetFloat("_DockingEffect", 0.0f);   // Initialize docking effect off
        }

        // 2. Convert coordinates and place at origin target element position
        targetOriginalAnchoredPos = GetCanvasPosition(elementToUnlock);
        lockStateContainer.anchoredPosition = targetOriginalAnchoredPos;
        unlockStateContainer.anchoredPosition = Vector2.zero;
        lockStateContainer.localScale = Vector3.one;
        backgroundDim.alpha = 0f;

        // --- CINEMATIC TWEEN SEQUENCE ---
        Sequence unlockSeq = DOTween.Sequence();

        unlockSeq.Join(backgroundDim.DOFade(0.85f, moveToCenterDuration));
        unlockSeq.AppendInterval(0.5f);

        unlockSeq.Join(lockStateContainer.DOAnchorPos(Vector2.zero, moveToCenterDuration).SetEase(moveEase));
        unlockSeq.Join(lockStateContainer.DOScale(1.25f, moveToCenterDuration).SetEase(moveEase));
        unlockSeq.AppendInterval(0.35f);

        unlockSeq.AppendCallback(() =>
        {
            lockCanvasGroup.DOFade(0f, 0.4f);
            lockStateContainer.DOScale(0.8f, 0.4f).SetEase(Ease.InBack)
                .OnComplete(() => lockStateContainer.gameObject.SetActive(false));

            unlockStateContainer.gameObject.SetActive(true);
            unlockCanvasGroup.DOFade(1f, unlockTransformDuration);
            unlockStateContainer.DOScale(1f, unlockTransformDuration).SetEase(Ease.OutBack);

            unlockStateContainer.DOScale(1.4f, 0.35f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad);
        });

        if (effectMaterial != null)
        {
            unlockSeq.Join(effectMaterial.DOFloat(1.0f, "_UnlockProgress", unlockTransformDuration).SetEase(Ease.OutQuad));
            unlockSeq.Join(effectMaterial.DOFloat(1.5f, "_SweepPosition", 1.1f).SetEase(Ease.InOutSine));
            unlockSeq.Join(effectMaterial.DOFloat(1.2f, "_UnlockFlash", 0.55f).SetLoops(2, LoopType.Yoyo));
        }

        unlockSeq.AppendInterval(0.5f);

        unlockSeq.AppendCallback(() =>
        {
            continueButton.gameObject.SetActive(true);
            continueButton.transform.localScale = Vector3.zero;
            continueButton.transform.DOScale(1f, 0.45f).SetEase(Ease.OutBack);
        });
    }

    private void OnContinueClicked()
    {
        continueButton.interactable = false;

        float moveDuration = 0.65f;
        float fadeDur = 0.25f;

        Sequence dismissSeq = DOTween.Sequence();

        // Hide Continue button instantly
        dismissSeq.Append(continueButton.transform.DOScale(0f, fadeDur).SetEase(Ease.InBack));
        dismissSeq.Join(backgroundDim.DOFade(0f, moveDuration));

        // Keep progress at 1.0 (fully unlocked) while moving back to destination[cite: 1, 2]
        if (effectMaterial != null)
        {
            effectMaterial.SetFloat("_UnlockProgress", 1.0f);
            effectMaterial.SetFloat("_DockingEffect", 0.0f);
        }

        // Glide the fully unlocked state back to the original slot[cite: 1, 2]
        dismissSeq.Join(unlockStateContainer.DOAnchorPos(targetOriginalAnchoredPos, moveDuration).SetEase(Ease.InOutQuad));

        // When it arrives at destination, trigger the shader's docking absorption effect
        dismissSeq.AppendCallback(() =>
        {
            if (effectMaterial != null)
            {
                effectMaterial.DOFloat(1.0f, "_DockingEffect", 0.3f).SetEase(Ease.InQuad).OnComplete(() =>
                {
                    effectRootCanvasGroup.SetActive(false);
                    continueButton.interactable = true;
                    onContinueCallback?.Invoke();

                    isBusy = false;
                });
            }
            else
            {
                effectRootCanvasGroup.SetActive(false);
                continueButton.interactable = true;
                onContinueCallback?.Invoke();
            }
        });
    }

    private Vector2 GetCanvasPosition(RectTransform target)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, target.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out Vector2 localPoint);
        return localPoint;
    }

    #region Test
    [ContextMenu("TestUnlockFeature")]
    private void TestUnlockFeature()
    {
        PlayUnlockEffect(testElementToUnlock, "Test Unlock Feature", null);
    }
    #endregion
}