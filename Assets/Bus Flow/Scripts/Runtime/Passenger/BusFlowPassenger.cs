using PrimeTween;
using UnityEngine;

namespace BusFlow
{
    // Placeholder 3D cho 1 hành khách. KHÔNG có Collider (xem PassengerBoardManager) - việc "xe có
    // đón được khách không" được xét bằng tra cứu list theo cột (O(numColumn)), không quét va chạm
    // từng khách, để chịu được hàng trăm khách cùng lúc mà không tốn chi phí physics.
    public class BusFlowPassenger : MonoBehaviour
    {
        public Color faction;
        public int column;
        public int row;
        public bool isBoarded;
        public bool isMoving;

        private MeshRenderer meshRenderer;
        private MaterialPropertyBlock materialPropertyBlock;

        public void Setup(Color passengerFaction, int passengerColumn, int passengerRow, Material overrideMaterial)
        {
            faction = passengerFaction;
            column = passengerColumn;
            row = passengerRow;
            isBoarded = false;
            isMoving = false;

            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
            }

            BusFlowVisualFactory.ApplyFactionVisual(meshRenderer, faction, overrideMaterial, ref materialPropertyBlock);
        }

        public void AnimateBoarding(Transform targetBus, float duration = 0.25f, System.Action onComplete = null)
        {
            isBoarded = true;
            Vector3 startScale = transform.localScale;
            Vector3 startPos = transform.position;

            Tween.Custom(0f, 1f, duration: duration, ease: Ease.InQuad, onValueChange: progress =>
            {
                if (targetBus != null)
                {
                    Vector3 busPos = targetBus.position;
                    busPos.z = startPos.z;
                    transform.position = Vector3.Lerp(startPos, busPos, progress);
                }
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
            }).OnComplete(() =>
            {
                onComplete?.Invoke();
                if (gameObject != null)
                {
                    Destroy(gameObject);
                }
            });
        }

        public static BusFlowPassenger CreatePlaceholder(Transform parent, Vector2 spacing = default)
        {
            float beadDiameter = spacing != default && Mathf.Min(spacing.x, spacing.y) > 0.01f
                ? Mathf.Min(spacing.x, spacing.y) * 0.95f
                : 0.45f;

            GameObject go = BusFlowVisualFactory.CreatePlaceholder(
                PrimitiveType.Sphere,
                parent,
                new Vector3(beadDiameter, beadDiameter, beadDiameter),
                keepCollider: false);

            go.name = "Passenger";

            return go.AddComponent<BusFlowPassenger>();
        }
    }
}
