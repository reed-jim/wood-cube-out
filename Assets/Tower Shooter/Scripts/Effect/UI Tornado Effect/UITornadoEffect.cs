using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class UITornadoEffect : MonoBehaviour
{
    [Header("Tornado Dynamics")]
    [Range(0f, 1f)] public float tornadoProgress = 0f; // 0 = Normal UI, 1 = Full Tornado
    public float swirlSpeed = 500f;
    public float turns = 3f;

    [Header("Cone Shape Settings")]
    public float coneHeight = 600f;
    public float topRadius = 300f;
    public float bottomRadius = 20f;
    public float depthDisplacement = 400f;

    [Header("Noise & Turbulence")]
    public float turbulenceAmount = 15f;
    public float turbulenceFrequency = 2f;

    private readonly List<UITornadoMeshModifier> activeModifiers = new List<UITornadoMeshModifier>();

    void OnEnable()
    {
        RegisterChildGraphics();
    }

    void OnDisable()
    {
        UnregisterChildGraphics();
    }

    void Update()
    {
        if (tornadoProgress > 0f)
        {
            NotifyModifiersDirty();
        }
    }

    [ContextMenu("Register Child Graphics")]
    public void RegisterChildGraphics()
    {
        activeModifiers.Clear();
        Graphic[] graphics = GetComponentsInChildren<Graphic>(true);
        foreach (var graphic in graphics)
        {
            var modifier = graphic.GetComponent<UITornadoMeshModifier>();
            if (modifier == null)
            {
                modifier = graphic.gameObject.AddComponent<UITornadoMeshModifier>();
            }
            modifier.SetOwner(this);
            if (!activeModifiers.Contains(modifier))
            {
                activeModifiers.Add(modifier);
            }
        }
    }

    public void UnregisterChildGraphics()
    {
        foreach (var mod in activeModifiers)
        {
            if (mod != null)
            {
                DestroyImmediate(mod);
            }
        }
        activeModifiers.Clear();
    }

    public void NotifyModifiersDirty()
    {
        for (int i = 0; i < activeModifiers.Count; i++)
        {
            if (activeModifiers[i] != null)
            {
                // Trigger recalculation on the modifier instance
                activeModifiers[i].RefreshMesh();
            }
        }
    }
}
