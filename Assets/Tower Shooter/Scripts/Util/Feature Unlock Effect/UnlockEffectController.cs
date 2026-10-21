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
    [SerializeField] private float moveToCenterDuration = 1.4f; // Slower, more deliberate start
    [SerializeField] private float unlockTransformDuration = 1.0f;
    [SerializeField] private Ease moveEase = Ease.OutQuart;

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

        if (unlockStateImage != null)
        {
            effectMaterial = new Material(unlockStateImage.material);
            unlockStateImage.material = effectMaterial;
        }
    }

    public void PlayUnlockEffect(RectTransform elementToUnlock, string featureName, Action onComplete = null)
    {
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
        }

        // 2. Convert coordinates and place at origin target element position
        Vector2 startAnchoredPos = GetCanvasPosition(elementToUnlock);
        lockStateContainer.anchoredPosition = startAnchoredPos;
        lockStateContainer.localScale = Vector3.one;
        backgroundDim.alpha = 0f;

        // --- CINEMATIC TWEEN SEQUENCE ---
        Sequence unlockSeq = DOTween.Sequence();

        // Step 1: Fade background & smooth, slow glide to screen center
        unlockSeq.Join(backgroundDim.DOFade(0.85f, moveToCenterDuration));

        unlockSeq.AppendInterval(0.5f); // Delay 0.5 seconds
        
        unlockSeq.Join(lockStateContainer.DOAnchorPos(Vector2.zero, moveToCenterDuration).SetEase(moveEase));
        unlockSeq.Join(lockStateContainer.DOScale(1.25f, moveToCenterDuration).SetEase(moveEase));

        // Clearer pause at the center to let the player focus before the transformation
        unlockSeq.AppendInterval(0.35f);

        // Step 2: Cross-fade from Lock State to Unlock State + Shader Sweep
        unlockSeq.AppendCallback(() =>
        {
            // Fade out and shrink Lock State
            lockCanvasGroup.DOFade(0f, 0.4f);
            lockStateContainer.DOScale(0.8f, 0.4f).SetEase(Ease.InBack)
                .OnComplete(() => lockStateContainer.gameObject.SetActive(false));

            // Fade in and expand Unlock State
            unlockStateContainer.gameObject.SetActive(true);
            unlockCanvasGroup.DOFade(1f, unlockTransformDuration);
            unlockStateContainer.DOScale(1f, unlockTransformDuration).SetEase(Ease.OutBack);

            // Punch scale effect on the whole card container
            unlockStateContainer.DOScale(1.4f, 0.35f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad);
        });

        if (effectMaterial != null)
        {
            // Sweep beam animation
            unlockSeq.Join(effectMaterial.DOFloat(1.5f, "_SweepPosition", 1.1f).SetEase(Ease.InOutSine));

            // Punchy brightness flash on the card shader
            effectMaterial.DOFloat(1.2f, "_UnlockFlash", 0.2f)
                .SetLoops(2, LoopType.Yoyo);
        }

        unlockSeq.AppendInterval(0.5f);

        // Step 3: Reveal Continue button gently
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

        float dismissDur = 0.45f;
        float fadeDur = 0.3f;

        Sequence dismissSeq = DOTween.Sequence();
        dismissSeq.Append(continueButton.transform.DOScale(0f, fadeDur).SetEase(Ease.InBack));
        dismissSeq.Join(unlockStateContainer.DOScale(0f, dismissDur).SetEase(Ease.InBack));
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