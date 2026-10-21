using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

public class UnlockEffectController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject effectRootCanvasGroup;
    [SerializeField] private CanvasGroup backgroundDim;
    [SerializeField] private RectTransform effectElement;

    [Header("State Containers (Separate)")]
    [SerializeField] private GameObject lockStateContainer;
    [SerializeField] private CanvasGroup lockCanvasGroup;
    [SerializeField] private GameObject unlockStateContainer;
    [SerializeField] private CanvasGroup unlockCanvasGroup;
    [SerializeField] private TextMeshProUGUI unlockedFeatureText;

    [Header("Action & Shader")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Image targetEffectImage;          // Image component using the procedural shader

    [Header("Cinematic Slow Pacing Settings")]
    [SerializeField] private float moveToCenterDuration = 1.0f;
    [SerializeField] private float unlockTransformDuration = 0.8f;
    [SerializeField] private Ease moveEase = Ease.OutQuart;

    [Header("Timing Control")]
    [Tooltip("Multiplies all animation durations. 1.0 = normal, 0.5 = twice as fast, 2.0 = twice as slow.")]
    [SerializeField] private float durationMultiplier = 1.0f;

    [Header("Test")]
    [SerializeField] private RectTransform testElementToUnlock;

    private Action onContinueCallback;
    private RectTransform canvasRect;
    private Material effectMaterial;

    private void Awake()
    {
        canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        effectRootCanvasGroup.SetActive(false);
        continueButton.gameObject.SetActive(false);
        continueButton.onClick.AddListener(OnContinueClicked);

        if (targetEffectImage != null)
        {
            effectMaterial = new Material(targetEffectImage.material);
            targetEffectImage.material = effectMaterial;
        }
    }

    // private void Update()
    // {
    //     // Dynamically update Rect Size property for the procedural shader
    //     if (effectMaterial != null && effectElement != null)
    //     {
    //         Vector2 rectSize = effectElement.rect;
    //         effectMaterial.SetVector("_RectSize", new Vector4(rectSize.x, rectSize.y, 0, 0));
    //     }
    // }

    public void PlayUnlockEffect(RectTransform elementToUnlock, string featureName, Action onComplete = null)
    {
        onContinueCallback = onComplete;
        effectRootCanvasGroup.SetActive(true);
        continueButton.gameObject.SetActive(false);

        // 1. Setup initial state: Lock State ON, Unlock State OFF
        lockStateContainer.SetActive(true);
        lockCanvasGroup.alpha = 1f;
        lockStateContainer.transform.localScale = Vector3.one;

        unlockStateContainer.SetActive(false);
        unlockCanvasGroup.alpha = 0f;
        unlockStateContainer.transform.localScale = Vector3.one * 0.9f;
        unlockedFeatureText.text = featureName;

        if (effectMaterial != null)
        {
            effectMaterial.SetFloat("_SweepPosition", -0.5f);
        }

        // 2. Convert coordinates and place at origin target element position
        Vector2 startAnchoredPos = GetCanvasPosition(elementToUnlock);
        effectElement.anchoredPosition = startAnchoredPos;
        effectElement.localScale = Vector3.one;
        backgroundDim.alpha = 0f;

        // --- SCALED CINEMATIC TWEEN SEQUENCE ---
        Sequence unlockSeq = DOTween.Sequence();

        float moveDur = moveToCenterDuration * durationMultiplier;
        float transformDur = unlockTransformDuration * durationMultiplier;

        // Step 1: Fade background & majestic glide to screen center
        unlockSeq.Join(backgroundDim.DOFade(0.85f, moveDur));
        unlockSeq.Join(effectElement.DOAnchorPos(Vector2.zero, moveDur).SetEase(moveEase));
        unlockSeq.Join(effectElement.DOScale(1.25f, moveDur).SetEase(moveEase));

        unlockSeq.AppendInterval(0.2f * durationMultiplier);

        // Step 2: Cross-fade from Lock State to Unlock State + Shader Sweep
        unlockSeq.AppendCallback(() =>
        {
            // Fade out and shrink Lock State
            lockCanvasGroup.DOFade(0f, 0.3f * durationMultiplier);
            lockStateContainer.transform.DOScale(0.8f, 0.3f * durationMultiplier).SetEase(Ease.InBack)
                .OnComplete(() => lockStateContainer.SetActive(false));

            // Fade in and expand Unlock State
            unlockStateContainer.SetActive(true);
            unlockCanvasGroup.DOFade(1f, transformDur);
            unlockStateContainer.transform.DOScale(1f, transformDur).SetEase(Ease.OutBack);

            // Punch scale effect on the whole card container
            effectElement.DOScale(1.4f, 0.3f * durationMultiplier).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad);
        });

        if (effectMaterial != null)
        {
            unlockSeq.Join(effectMaterial.DOFloat(1.5f, "_SweepPosition", 0.9f * durationMultiplier).SetEase(Ease.InOutSine));
        }

        unlockSeq.AppendInterval(0.4f * durationMultiplier);

        // Step 3: Reveal Continue button gently
        unlockSeq.AppendCallback(() =>
        {
            continueButton.gameObject.SetActive(true);
            continueButton.transform.localScale = Vector3.zero;
            continueButton.transform.DOScale(1f, 0.4f * durationMultiplier).SetEase(Ease.OutBack);
        });
    }

    private void OnContinueClicked()
    {
        continueButton.interactable = false;

        float dismissDur = 0.4f * durationMultiplier;
        float fadeDur = 0.25f * durationMultiplier;

        Sequence dismissSeq = DOTween.Sequence();
        dismissSeq.Append(continueButton.transform.DOScale(0f, fadeDur).SetEase(Ease.InBack));
        dismissSeq.Join(effectElement.DOScale(0f, dismissDur).SetEase(Ease.InBack));
        dismissSeq.Join(backgroundDim.DOFade(0f, dismissDur));

        dismissSeq.OnComplete(() =>
        {
            effectRootCanvasGroup.SetActive(false);
            continueButton.interactable = true;
            onContinueCallback?.Invoke();
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