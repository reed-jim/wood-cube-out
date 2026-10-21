using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.FlowChart
{
    public enum FlowOperation
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Exponent
    }

    [RequireComponent(typeof(CanvasRenderer))]
    public class UIFlowConnection : MaskableGraphic
    {
        [Header("Connected Flow Elements")]
        [SerializeField] private UIFlowElement sourceElement;
        [SerializeField] private UIFlowElement targetElement;

        [Header("Operation Settings")]
        [SerializeField] private FlowOperation operation = FlowOperation.Add;
        [SerializeField] private TextMeshProUGUI operationBadgeText; // Optional UI text badge placed at the midpoint

        [Header("Line Settings")]
        [SerializeField] private float lineThickness = 3f;
        [SerializeField] private Vector2 operationTextOffset;

        private RectTransform containerRect;
        private RectTransform badgeRect;

        protected override void Awake()
        {
            base.Awake();
            containerRect = transform.parent as RectTransform;

            if (operationBadgeText != null)
            {
                badgeRect = operationBadgeText.rectTransform;
                UpdateBadgeDisplay();
            }
        }

        public void SetConnection(UIFlowElement source, UIFlowElement target, FlowOperation op)
        {
            sourceElement = source;
            targetElement = target;
            operation = op;
            UpdateBadgeDisplay();
            SetVerticesDirty();
        }

        public void SetOperation(FlowOperation op)
        {
            operation = op;
            UpdateBadgeDisplay();
        }

        private void UpdateBadgeDisplay()
        {
            if (operationBadgeText == null) return;

            operationBadgeText.text = operation switch
            {
                FlowOperation.Add => "+",
                FlowOperation.Subtract => "-",
                FlowOperation.Multiply => "×",
                FlowOperation.Divide => "÷",
                FlowOperation.Exponent => "^",
                _ => "+"
            };
        }

        private void LateUpdate()
        {
            if (sourceElement == null || targetElement == null || containerRect == null) return;

            // Refresh line vertices and badge position as elements move
            SetVerticesDirty();
            UpdateBadgePosition();
        }

        private void UpdateBadgePosition()
        {
            if (badgeRect == null) return;

            Vector2 p1 = GetLocalPoint(sourceElement.RectTransform);
            Vector2 p2 = GetLocalPoint(targetElement.RectTransform);
            Vector2 midPoint = (p1 + p2) * 0.5f;

            badgeRect.anchoredPosition = midPoint + operationTextOffset;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (sourceElement == null || targetElement == null || containerRect == null) return;

            Vector2 p1 = GetLocalPoint(sourceElement.RectTransform);
            Vector2 p2 = GetLocalPoint(targetElement.RectTransform);

            DrawLineSegment(vh, p1, p2, lineThickness);
        }

        private Vector2 GetLocalPoint(RectTransform target)
        {
            Vector3 worldPos = target.position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                containerRect,
                RectTransformUtility.WorldToScreenPoint(null, worldPos),
                null,
                out Vector2 localPoint
            );
            return localPoint;
        }

        private void DrawLineSegment(VertexHelper vh, Vector2 start, Vector2 end, float thickness)
        {
            Vector2 dir = (end - start).normalized;
            if (dir == Vector2.zero) return;

            Vector2 normal = new Vector2(-dir.y, dir.x) * (thickness * 0.5f);

            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;

            int count = vh.currentVertCount;

            vertex.position = start - normal; vh.AddVert(vertex);
            vertex.position = start + normal; vh.AddVert(vertex);
            vertex.position = end + normal; vh.AddVert(vertex);
            vertex.position = end - normal; vh.AddVert(vertex);

            vh.AddTriangle(count, count + 1, count + 2);
            vh.AddTriangle(count + 2, count + 3, count);
        }
    }
}