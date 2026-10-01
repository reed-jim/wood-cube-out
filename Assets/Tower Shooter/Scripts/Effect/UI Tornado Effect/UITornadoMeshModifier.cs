using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteInEditMode]
[RequireComponent(typeof(Graphic))]
public class UITornadoMeshModifier : BaseMeshEffect
{
    private UITornadoEffect owner;
    private TMP_Text tmpText;

    protected override void Awake()
    {
        base.Awake();
        tmpText = GetComponent<TMP_Text>();
    }

    public void SetOwner(UITornadoEffect tornadoOwner)
    {
        owner = tornadoOwner;
    }

    /// <summary>
    /// Triggers vertex re-calculation without calling protected methods.
    /// </summary>
    public void RefreshMesh()
    {
        if (graphic != null)
        {
            // Standard Unity UI recalculation
            graphic.SetVerticesDirty();

            // TextMeshPro requires explicit mesh force update when modified via IMeshModifier
            if (tmpText != null)
            {
                tmpText.ForceMeshUpdate();
            }
        }
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() || owner == null || owner.tornadoProgress <= 0f)
            return;

        List<UIVertex> vertices = new List<UIVertex>();
        vh.GetUIVertexStream(vertices);

        RectTransform parentRect = owner.GetComponent<RectTransform>();
        Matrix4x4 localToParent = parentRect.worldToLocalMatrix * transform.localToWorldMatrix;

        float progress = owner.tornadoProgress;
        float timeOffset = Time.time * owner.swirlSpeed;

        for (int i = 0; i < vertices.Count; i++)
        {
            UIVertex vertex = vertices[i];

            // Transform vertex into owner UI local space
            Vector3 parentLocalPos = localToParent.MultiplyPoint3x4(vertex.position);

            // Normalize height position (0.0 at bottom, 1.0 at top)
            float normalizedHeight = Mathf.Clamp01((parentLocalPos.y + (owner.coneHeight * 0.5f)) / owner.coneHeight);

            // Radius along the funnel cone
            float targetRadius = Mathf.Lerp(owner.bottomRadius, owner.topRadius, normalizedHeight);

            // Rotational angle + continuous spin + twist along height
            float swirlAngle = (normalizedHeight * owner.turns * 360f) + timeOffset;
            float radians = swirlAngle * Mathf.Deg2Rad;

            // Perlin noise jitter for turbulence
            float noiseX = (Mathf.PerlinNoise(normalizedHeight * owner.turbulenceFrequency, Time.time * 2f) - 0.5f) * owner.turbulenceAmount;
            float noiseZ = (Mathf.PerlinNoise(Time.time * 2f, normalizedHeight * owner.turbulenceFrequency) - 0.5f) * owner.turbulenceAmount;

            // Target 3D Tornado position
            Vector3 tornadoPos;
            tornadoPos.x = Mathf.Cos(radians) * targetRadius + noiseX;
            tornadoPos.y = (normalizedHeight - 0.5f) * owner.coneHeight;
            tornadoPos.z = Mathf.Sin(radians) * owner.depthDisplacement + noiseZ;

            // Blend smoothly between flat UI layout and 3D Tornado
            Vector3 finalParentPos = Vector3.Lerp(parentLocalPos, tornadoPos, progress);

            // Fade out vertices slightly as they move deeper into the vortex
            float depthAlpha = Mathf.InverseLerp(-owner.depthDisplacement, owner.depthDisplacement, finalParentPos.z);
            vertex.color.a = (byte)(vertex.color.a * Mathf.Lerp(1.0f, depthAlpha, progress));

            // Convert back to element's local space
            vertex.position = localToParent.inverse.MultiplyPoint3x4(finalParentPos);
            vertices[i] = vertex;
        }

        vh.Clear();
        vh.AddUIVertexTriangleStream(vertices);
    }
}
