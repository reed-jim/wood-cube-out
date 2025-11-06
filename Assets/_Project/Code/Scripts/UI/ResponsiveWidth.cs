using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class ResponsiveWidth : MonoBehaviour
{
    public enum FitMode { PercentOfScreen, MatchReferenceWidth }
    public enum FixedEdge { Left, Center, Right }

    [Header("Sizing")]
    public FitMode mode = FitMode.MatchReferenceWidth;

    [Range(0f, 1f)]
    public float screenWidthPercent = 0.9f; // used in PercentOfScreen
    public float referenceDesignWidth = 1080f;      // used in MatchReferenceWidth
    public float referenceElementWidth = 300f;      // used in MatchReferenceWidth

    [Header("Edge to keep fixed (in parent space)")]
    public FixedEdge fixedEdge = FixedEdge.Left;

    [Header("Bounds (optional)")]
    public float minWidth = 0f;
    public float maxWidth = 0f;

    [Header("Safety")]
    public bool useSafeArea = true;
    public bool watchForSizeChanges = true;

    RectTransform rt;
    Canvas rootCanvas;
    Vector2 lastScreenSize;
    Rect lastSafeArea;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    void OnEnable()
    {
        CacheScreen();
        Apply();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isPlaying) Apply();
    }
#endif

    void Update()
    {
        if (!watchForSizeChanges) return;
        if (ScreenChanged())
        {
            Apply();
            CacheScreen();
        }
    }

    void Apply()
    {
        if (rt == null) rt = GetComponent<RectTransform>();
        if (rootCanvas == null) rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null) return;

        // Current width (canvas units)
        float oldWidth = rt.rect.width;

        // Target width (screen px -> canvas units)
        float screenPxWidth = useSafeArea ? Screen.safeArea.width : (float)Screen.width;
        float targetPx = 0f;

        if (mode == FitMode.PercentOfScreen)
        {
            targetPx = Mathf.Clamp01(screenWidthPercent) * screenPxWidth;
        }
        else
        {
            if (referenceDesignWidth <= 0f) referenceDesignWidth = 1080f;
            float frac = Mathf.Max(0f, referenceElementWidth / referenceDesignWidth);
            targetPx = screenPxWidth * frac;
        }

        if (minWidth > 0f) targetPx = Mathf.Max(minWidth, targetPx);
        if (maxWidth > 0f) targetPx = Mathf.Min(maxWidth, targetPx);

        float targetUnits = PixelsToCanvasUnits(targetPx, rootCanvas);

        bool stretched = rt.anchorMin.x != rt.anchorMax.x;

        if (!stretched)
        {
            // Fixed anchors: change sizeDelta.x, then offset anchoredPosition to keep chosen edge fixed.
            Vector2 sd = rt.sizeDelta;
            float newWidth = targetUnits;
            float deltaW = newWidth - oldWidth;

            // Apply new width
            sd.x = newWidth;
            rt.sizeDelta = sd;

            // Compensate movement to keep the selected edge fixed
            // anchoredPosition is at the pivot
            float pivot = rt.pivot.x;
            Vector2 ap = rt.anchoredPosition;

            switch (fixedEdge)
            {
                case FixedEdge.Left:
                    // Move pivot right by pivot*deltaW to keep left edge constant
                    ap.x += deltaW * pivot;
                    break;
                case FixedEdge.Right:
                    // Move pivot left by (1-pivot)*deltaW to keep right edge constant
                    ap.x -= deltaW * (1f - pivot);
                    break;
                case FixedEdge.Center:
                    // Keep center: adjust by (pivot-0.5)*deltaW
                    ap.x += deltaW * (pivot - 0.5f);
                    break;
            }
            rt.anchoredPosition = ap;
        }
        else
        {
            // Stretched anchors: keep the chosen edge’s offset constant and solve for the other.
            float parentWidth = GetParentWidth(rt);
            float stretchPortion = (rt.anchorMax.x - rt.anchorMin.x) * parentWidth;

            Vector2 offMin = rt.offsetMin; // left, bottom (left is negative)
            Vector2 offMax = rt.offsetMax; // right, top  (right is positive)

            float currentWidth = stretchPortion + (offMax.x - offMin.x);
            float deltaW = targetUnits - currentWidth;

            switch (fixedEdge)
            {
                case FixedEdge.Left:
                    // Keep left offset (offMin.x) the same; change offMax.x to hit target width
                    offMax.x = offMin.x + (targetUnits - stretchPortion);
                    break;
                case FixedEdge.Right:
                    // Keep right offset (offMax.x) the same; change offMin.x
                    offMin.x = offMax.x - (targetUnits - stretchPortion);
                    break;
                case FixedEdge.Center:
                    // Distribute symmetrically to preserve center
                    float half = (targetUnits - stretchPortion) * 0.5f;
                    offMin.x = -half;
                    offMax.x = +half;
                    break;
            }

            rt.offsetMin = offMin;
            rt.offsetMax = offMax;
        }
    }

    static float PixelsToCanvasUnits(float px, Canvas canvas)
    {
        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            // If you use World Space, adjust this to your world scale convention.
            return px / 100f;
        }
        var scaler = canvas.GetComponent<CanvasScaler>();
        float scale = scaler ? scaler.scaleFactor : canvas.scaleFactor;
        if (scale <= 0f) scale = 1f;
        return px / scale;
    }

    static float GetParentWidth(RectTransform child)
    {
        var parent = child.parent as RectTransform;
        return parent ? parent.rect.width : 0f;
    }

    void CacheScreen()
    {
        lastScreenSize = new Vector2(Screen.width, Screen.height);
        lastSafeArea = Screen.safeArea;
    }

    bool ScreenChanged()
    {
        return lastScreenSize.x != Screen.width ||
               lastScreenSize.y != Screen.height ||
               lastSafeArea != Screen.safeArea;
    }
}
