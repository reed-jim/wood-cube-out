using UnityEngine;
using UnityEngine.UI;

namespace UI.FlowChart
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class UIFlowConnection : MaskableGraphic
    {
        [Header("Connected Flow Elements")]
        [SerializeField] private UIFlowElement sourceElement;
        [SerializeField] private UIFlowElement targetElement;

        [Header("Line Settings")]
        [SerializeField] private float lineThickness = 3f;

        private RectTransform containerRect;

        protected override void Awake()
        {
            base.Awake();
            containerRect = transform.parent as RectTransform;
        }

        public void SetElements(UIFlowElement source, UIFlowElement target)
        {
            sourceElement = source;
            targetElement = target;
            SetVerticesDirty();
        }

        private void LateUpdate()
        {
            if (sourceElement == null || targetElement == null || containerRect == null) return;

            // Automatically refresh line vertices when either flow element moves
            SetVerticesDirty();
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
            vertex.position = end + normal;   vh.AddVert(vertex);
            vertex.position = end - normal;   vh.AddVert(vertex);

            vh.AddTriangle(count, count + 1, count + 2);
            vh.AddTriangle(count + 2, count + 3, count);
        }
    }
}