using System.Collections.Generic;
using Dreamteck.Splines;
using UnityEngine;

namespace BusFlow
{
    // Bọc 1 SplineComputer dựng động lúc runtime (từ điểm bake trong BusPathItemConfig) + bảng
    // khoảng cách -> percent, y hệt cách GrainPool.cs (Chickens Flow) di chuyển arrow đều tốc độ
    // trên spline. Không cần SplineComputer tồn tại sẵn trong scene - tự tạo, tự dọn khi Dispose().
    public class RuntimeSplinePath
    {
        private readonly SplineComputer computer;
        private readonly List<float> distanceTable = new List<float>();
        private readonly List<float> percentTable = new List<float>();
        private float totalLength;

        public float Length => totalLength;
        public bool IsLoop { get; private set; }

        public RuntimeSplinePath(Transform parent, List<Vector3> localPoints, bool isLoop)
        {
            IsLoop = isLoop;

            GameObject go = new GameObject("RuntimeSplinePath");
            go.transform.SetParent(parent, false);

            computer = go.AddComponent<SplineComputer>();
            computer.type = Spline.Type.CatmullRom;
            computer.is2D = true;

            SplinePoint[] splinePoints = new SplinePoint[localPoints.Count];

            for (int i = 0; i < localPoints.Count; i++)
            {
                splinePoints[i] = new SplinePoint(localPoints[i]);
            }

            computer.SetPoints(splinePoints, SplineComputer.Space.Local);

            if (isLoop && localPoints.Count >= 3 && !computer.isClosed)
            {
                computer.Close();
            }

            computer.RebuildImmediate();

            BuildDistanceTable();
        }

        public SplineComputer Computer => computer;

        private void BuildDistanceTable()
        {
            const int samples = 400;

            distanceTable.Clear();
            percentTable.Clear();

            float accumulated = 0f;
            Vector3 previous = computer.Evaluate(0f).position;

            distanceTable.Add(0f);
            percentTable.Add(0f);

            for (int i = 1; i <= samples; i++)
            {
                float percent = i / (float)samples;
                Vector3 position = computer.Evaluate(percent).position;

                accumulated += Vector3.Distance(previous, position);

                distanceTable.Add(accumulated);
                percentTable.Add(percent);

                previous = position;
            }

            totalLength = accumulated;
        }

        private float DistanceToPercent(float distance)
        {
            distance = Mathf.Clamp(distance, 0f, totalLength);

            for (int i = 1; i < distanceTable.Count; i++)
            {
                if (distance <= distanceTable[i])
                {
                    float t = Mathf.InverseLerp(distanceTable[i - 1], distanceTable[i], distance);

                    return Mathf.Lerp(percentTable[i - 1], percentTable[i], t);
                }
            }

            return 1f;
        }

        public Vector3 GetPositionAtDistance(float distance)
        {
            return computer.Evaluate(DistanceToPercent(distance)).position;
        }

        // Vòng loop kín: distance vượt Length thì vòng lại đầu. Đường thường: kẹp trong [0, Length].
        public float NormalizeDistance(float distance)
        {
            if (totalLength <= 0.0001f)
            {
                return 0f;
            }

            if (IsLoop)
            {
                distance %= totalLength;

                if (distance < 0f)
                {
                    distance += totalLength;
                }

                return distance;
            }

            return Mathf.Clamp(distance, 0f, totalLength);
        }

        // Tìm khoảng cách (dọc theo đường) của điểm trên spline gần worldPos nhất - dùng khi xe rời
        // bãi đỗ, "nhập làn" vào đúng điểm loop gần nó nhất rồi mới chạy vòng.
        public float GetNearestDistanceToWorldPoint(Vector3 worldPos)
        {
            float bestDistance = 0f;
            float bestSqr = float.MaxValue;

            for (int i = 0; i < distanceTable.Count; i++)
            {
                Vector3 sample = computer.Evaluate(percentTable[i]).position;
                float sqr = (sample - worldPos).sqrMagnitude;

                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    bestDistance = distanceTable[i];
                }
            }

            return bestDistance;
        }

        public Vector3 GetPositionAtPercent(float percent)
        {
            return computer.Evaluate(percent).position;
        }

        public Quaternion GetRotationAtPercent(float percent)
        {
            Vector3 forward = computer.Evaluate(percent).forward;

            if (forward.sqrMagnitude < 0.0001f)
            {
                return Quaternion.identity;
            }

            float angle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;

            return Quaternion.Euler(0f, 0f, angle);
        }

        public float GetNearestPercentToWorldPoint(Vector3 worldPos)
        {
            float bestPercent = 0f;
            float bestSqr = float.MaxValue;

            for (int i = 0; i < percentTable.Count; i++)
            {
                Vector3 sample = computer.Evaluate(percentTable[i]).position;
                float sqr = (sample - worldPos).sqrMagnitude;

                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    bestPercent = percentTable[i];
                }
            }

            return bestPercent;
        }

        public Quaternion GetRotationAtDistance(float distance)
        {
            Vector3 forward = computer.Evaluate(DistanceToPercent(distance)).forward;

            if (forward.sqrMagnitude < 0.0001f)
            {
                return Quaternion.identity;
            }

            // Spline 2D chạy trên mặt phẳng XY: chỉ xoay quanh trục Z theo tiếp tuyến (angle),
            // tránh dùng LookRotation 3D làm lật nghiêng đối tượng 2D (Spine) thành mỏng dẹt / biến mất.
            float angle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;

            return Quaternion.Euler(0f, 0f, angle);
        }

        // Render đường thành 1 dải mesh phẳng nhìn thấy trong game (SplineComputer thuần chỉ là dữ
        // liệu, không tự hiện). SplineRenderer của Dreamteck tự [RequireComponent] MeshFilter+Renderer.
        public void ShowRoad(Material roadMaterial, float roadWidth, Color roadColor)
        {
            if (computer == null)
            {
                return;
            }

            // Đẩy mặt đường về Z = 0.2f (nằm sau xe và khách) để tránh Z-fighting/che khuất.
            computer.transform.localPosition = new Vector3(0f, 0f, 0.2f);

            SplineRenderer renderer = computer.gameObject.GetComponent<SplineRenderer>();

            if (renderer == null)
            {
                renderer = computer.gameObject.AddComponent<SplineRenderer>();
            }

            renderer.spline = computer;
            renderer.size = roadWidth;
            renderer.color = roadColor;

            MeshRenderer meshRenderer = computer.gameObject.GetComponent<MeshRenderer>();

            if (meshRenderer != null && roadMaterial != null)
            {
                meshRenderer.sharedMaterial = roadMaterial;
            }

            renderer.RebuildImmediate();
        }

        public void Dispose()
        {
            if (computer != null)
            {
                Object.Destroy(computer.gameObject);
            }
        }
    }

    // Tạo placeholder 3D (primitive) cho khách/xe khi chưa có asset thật, tô màu theo faction qua
    // FactionMapper (BusFactionAssets) nếu có material, fallback tint qua MaterialPropertyBlock.
    public static class BusFlowVisualFactory
    {
        private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");
        private static readonly int BaseColorPropertyId = Shader.PropertyToID("_BaseColor");

        public static GameObject CreatePlaceholder(PrimitiveType primitiveType, Transform parent, Vector3 localScale, bool keepCollider)
        {
            GameObject go = GameObject.CreatePrimitive(primitiveType);
            go.transform.SetParent(parent, false);
            go.transform.localScale = localScale;

            if (!keepCollider)
            {
                Collider objectCollider = go.GetComponent<Collider>();

                if (objectCollider != null)
                {
                    Object.Destroy(objectCollider);
                }
            }

            return go;
        }

        public static void ApplyFactionVisual(MeshRenderer meshRenderer, Color faction, Material overrideMaterial, ref MaterialPropertyBlock propertyBlock)
        {
            if (overrideMaterial != null)
            {
                meshRenderer.sharedMaterial = overrideMaterial;

                return;
            }

            if (propertyBlock == null)
            {
                propertyBlock = new MaterialPropertyBlock();
            }

            meshRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(ColorPropertyId, faction);
            propertyBlock.SetColor(BaseColorPropertyId, faction);
            meshRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
