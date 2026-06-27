using UnityEngine;

public class SafeArea : MonoBehaviour
{
    [SerializeField] private Canvas canvas;

    private Rect _lastSafeArea;
    private Vector2 _lastScreenSize;
    private ScreenOrientation _lastOrientation;
    private Vector2 _lastCanvasSize;
    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        AdjustToSafeArea(true);
    }

    private void Start()
    {
        AdjustToSafeArea(true);
    }

    private void Update()
    {
        Rect safeArea = Screen.safeArea;
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        ScreenOrientation orientation = Screen.orientation;
        
        Vector2 canvasSize = Vector2.zero;
        if (canvas == null && _rectTransform != null)
        {
            canvas = _rectTransform.GetComponentInParent<Canvas>();
        }
        if (canvas != null)
        {
            canvasSize = new Vector2(canvas.pixelRect.width, canvas.pixelRect.height);
        }

        if (safeArea != _lastSafeArea || screenSize != _lastScreenSize || orientation != _lastOrientation || canvasSize != _lastCanvasSize)
        {
            AdjustToSafeArea(false);
        }
    }

    private void OnValidate()
    {
        AdjustToSafeArea(true);
    }

    private void AdjustToSafeArea(bool force)
    {
        if (_rectTransform == null)
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        if (_rectTransform == null) return;

        Rect safeArea = Screen.safeArea;
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        ScreenOrientation orientation = Screen.orientation;

        if (canvas == null)
        {
            canvas = _rectTransform.GetComponentInParent<Canvas>();
        }

        if (canvas == null)
        {
            return;
        }

        Vector2 canvasSize = new Vector2(canvas.pixelRect.width, canvas.pixelRect.height);

        if (canvasSize.x == 0 || canvasSize.y == 0)
        {
            // Do not cache if the canvas is not yet initialized or inactive.
            // This ensures we will try again next Update when the canvas size is valid.
            return;
        }

        if (!force && safeArea == _lastSafeArea && screenSize == _lastScreenSize && orientation == _lastOrientation && canvasSize == _lastCanvasSize)
        {
            return;
        }

        // Cache values only after successfully calculating anchors
        _lastSafeArea = safeArea;
        _lastScreenSize = screenSize;
        _lastOrientation = orientation;
        _lastCanvasSize = canvasSize;

        Vector2 safeAreaMin = safeArea.position;
        Vector2 safeAreaMax = safeArea.position + safeArea.size;

        Vector2 minAnchor = new Vector2(safeAreaMin.x / canvasSize.x, safeAreaMin.y / canvasSize.y);
        Vector2 maxAnchor = new Vector2(safeAreaMax.x / canvasSize.x, safeAreaMax.y / canvasSize.y);

        _rectTransform.anchorMin = minAnchor;
        _rectTransform.anchorMax = maxAnchor;

        _rectTransform.offsetMin = Vector2.zero;
        _rectTransform.offsetMax = Vector2.zero;
    }
}
