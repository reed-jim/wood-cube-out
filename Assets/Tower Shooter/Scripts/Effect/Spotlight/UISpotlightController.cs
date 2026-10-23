using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Image))]
public class UISpotlightController : MonoBehaviour
{
    [SerializeField] private Canvas canvas;

    [Header("Animation Settings")]
    [SerializeField] private float showDuration = 0.35f;
    [SerializeField] private float hideDuration = 0.25f;

    [Header("Test")]
    [SerializeField] private RectTransform testTarget;
    
    private Material material;
    private Image uiImage;
    private Tween currentTween;

    private static readonly int TargetRectID = Shader.PropertyToID("_TargetRect");
    private static readonly int ProgressID = Shader.PropertyToID("_Progress");
    private static readonly int FadeAlphaID = Shader.PropertyToID("_FadeAlpha");

    void Awake()
    {
        uiImage = GetComponent<Image>();
        if (uiImage != null && uiImage.material != null)
        {
            material = new Material(uiImage.material);
            uiImage.material = material;
        }

        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }

        ResetState();
    }

    private void ResetState()
    {
        if (material != null)
        {
            material.SetFloat(ProgressID, 0f);
            material.SetFloat(FadeAlphaID, 0f);
        }
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Shows spotlight: starts full screen and shrinks down to target.
    /// </summary>
    public void Show(RectTransform targetRectTransform)
    {
        if (targetRectTransform == null || material == null) return;

        currentTween?.Kill();
        gameObject.SetActive(true);

        CalculateTargetCoordinates(targetRectTransform);

        // Reset to full-screen size and full opacity, then tween progress to 1
        material.SetFloat(ProgressID, 0f);
        material.SetFloat(FadeAlphaID, 1f);

        currentTween = material.DOFloat(1f, ProgressID, showDuration);
    }

    /// <summary>
    /// Hides spotlight: stays in place at target and fades out.
    /// </summary>
    public void Hide()
    {
        if (!gameObject.activeSelf || material == null) return;

        currentTween?.Kill();

        // Fade out alpha in place
        currentTween = material.DOFloat(0f, FadeAlphaID, hideDuration)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
    }

    private void CalculateTargetCoordinates(RectTransform target)
    {
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);

        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : Camera.main;

        Vector2 minViewport, maxViewport;

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay || cam == null)
        {
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, RectTransformUtility.WorldToScreenPoint(null, corners[0]), null, out Vector2 localMin);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, RectTransformUtility.WorldToScreenPoint(null, corners[2]), null, out Vector2 localMax);

            minViewport = (localMin - canvasRect.rect.min) / canvasRect.rect.size;
            maxViewport = (localMax - canvasRect.rect.min) / canvasRect.rect.size;
        }
        else
        {
            Vector3 minScreen = cam.WorldToViewportPoint(corners[0]);
            Vector3 maxScreen = cam.WorldToViewportPoint(corners[2]);
            minViewport = new Vector2(minScreen.x, minScreen.y);
            maxViewport = new Vector2(maxScreen.x, maxScreen.y);
        }

        material.SetVector(TargetRectID, new Vector4(minViewport.x, minViewport.y, maxViewport.x, maxViewport.y));
    }
    
    #region Test
    [ContextMenu("TestShow")]
    private void TestShow()
    {
        Show(testTarget);
    }

    [ContextMenu("TestHide")]
    private void TestHide()
    {
        Hide();
    }
    #endregion
}