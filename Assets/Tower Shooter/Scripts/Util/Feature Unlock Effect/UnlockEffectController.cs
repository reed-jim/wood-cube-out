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

    [Header("States Inside Effect Element")]
    [SerializeField] private GameObject lockIconObject;
    [SerializeField] private TextMeshProUGUI featureText;
    [SerializeField] private Button continueButton;
    [SerializeField] private Image targetEffectImage;          // Image component using the custom shader material

    [Header("Cinematic Slow Pacing Settings")]
    [SerializeField] private float moveToCenterDuration = 1.0f;  // Slower, majestic transition
    [SerializeField] private float unlockTransformDuration = 0.8f;
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

        if (targetEffectImage != null)
        {
            // Instantiate material instance so it doesn't leak or override asset settings globally
            effectMaterial = new Material(targetEffectImage.material);
            targetEffectImage.material = effectMaterial;
        }
    }

    public void PlayUnlockEffect(RectTransform elementToUnlock, string unlockedFeatureName, Action onComplete = null)
    {
        onContinueCallback = onComplete;
        effectRootCanvasGroup.SetActive(true);
        continueButton.gameObject.SetActive(false);

        // 1. Setup initial Locked state appearance
        lockIconObject.SetActive(true);
        lockIconObject.transform.localScale = Vector3.one;
        featureText.text = "Locked Content";
        featureText.color = new Color(0.7f, 0.7f, 0.7f, 1f);

        if (effectMaterial != null)
        {
            effectMaterial.SetFloat("_SweepPosition", -0.5f);
        }

        // 2. Convert coordinates and place at origin target element position
        Vector2 startAnchoredPos = GetCanvasPosition(elementToUnlock);
        effectElement.anchoredPosition = startAnchoredPos;
        effectElement.localScale = Vector3.one;
        backgroundDim.alpha = 0f;

        // --- SLOW CINEMATIC TWEEN SEQUENCE ---
        Sequence unlockSeq = DOTween.Sequence();

        // Step 1: Slow fade background & majestic glide to screen center
        unlockSeq.Join(backgroundDim.DOFade(0.85f, moveToCenterDuration));
        unlockSeq.Join(effectElement.DOAnchorPos(Vector2.zero, moveToCenterDuration).SetEase(moveEase));
        unlockSeq.Join(effectElement.DOScale(1.25f, moveToCenterDuration).SetEase(moveEase));

        unlockSeq.AppendInterval(0.2f); // Brief pause at center for dramatic effect

        // Step 2: Unlock transformation & Custom Shader light sweep beam
        unlockSeq.AppendCallback(() =>
        {
            // Lock shrinks away
            lockIconObject.transform.DOScale(0f, 0.35f).SetEase(Ease.InBack)
                .OnComplete(() => lockIconObject.SetActive(false));

            // Text changes to actual unlocked feature name
            featureText.text = unlockedFeatureName;
            featureText.DOColor(Color.white, unlockTransformDuration);
            effectElement.DOScale(1.4f, 0.3f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad);
        });

        // Drive the custom URP HLSL shader sweep property from -0.5 to 1.5 smoothly
        if (effectMaterial != null)
        {
            unlockSeq.Join(effectMaterial.DOFloat(1.5f, "_SweepPosition", 0.9f).SetEase(Ease.InOutSine));
        }

        unlockSeq.AppendInterval(0.4f);

        // Step 3: Reveal Continue button gently
        unlockSeq.AppendCallback(() =>
        {
            continueButton.gameObject.SetActive(true);
            continueButton.transform.localScale = Vector3.zero;
            continueButton.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
        });
    }

    private void OnContinueClicked()
    {
        continueButton.interactable = false;

        Sequence dismissSeq = DOTween.Sequence();
        dismissSeq.Append(continueButton.transform.DOScale(0f, 0.25f).SetEase(Ease.InBack));
        dismissSeq.Join(effectElement.DOScale(0f, 0.4f).SetEase(Ease.InBack));
        dismissSeq.Join(backgroundDim.DOFade(0f, 0.4f));

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