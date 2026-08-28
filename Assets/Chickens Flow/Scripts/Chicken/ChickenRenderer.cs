using System;
using System.Collections.Generic;
using Saferio.Util.SaferioTween;
using UnityEngine;

// 1. Keep strings, but implement fast IEquatable to eliminate Boxing GC
public readonly struct ChickenBatchKey : IEquatable<ChickenBatchKey>
{
    public readonly int factionIndex;
    public readonly string anim;

    public ChickenBatchKey(int factionIndex, string anim)
    {
        this.factionIndex = factionIndex;
        this.anim = anim;
    }

    public bool Equals(ChickenBatchKey other)
    {
        return factionIndex == other.factionIndex && string.Equals(anim, other.anim, StringComparison.Ordinal);
    }

    public override bool Equals(object obj)
    {
        return obj is ChickenBatchKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        // Faster hash calculation without System.HashCode.Combine overhead
        return (factionIndex * 397) ^ (anim != null ? anim.GetHashCode() : 0);
    }
}

// 2. ChickenData remains intact as a class matching your structure
public class ChickenData
{
    public Vector3 position;
    public float scale;
    public int factionIndex;
    public string anim;
    public float animStartFrame;
    public float highlight;
}

public class ChickenRenderer : MonoBehaviour
{
    [Header("Rendering")]
    [SerializeField] private Mesh chickenMesh;
    [SerializeField] private MaterialsForAnim[] materialsForAnims;
    [SerializeField] private Material shadowMaterial;

    [Header("Customize")]
    public Vector3 offset;
    public Vector3 shadowOffset;
    public Vector3 shadowSize;
    public float chickenSize;
    public float maxAnimationFrame;

    private const int BatchSize = 1023;

    private readonly List<ChickenData> chickenDataItems = new(128);
    private readonly Dictionary<BaseChicken, ChickenData> dataOfChicken = new(128);

    private readonly Matrix4x4[] matrices = new Matrix4x4[BatchSize];
    private readonly Matrix4x4[] shadowMatrices = new Matrix4x4[300];
    private readonly Vector4[] animData = new Vector4[BatchSize];
    private readonly float[] animStartFrame = new float[BatchSize];
    private readonly float[] spriteSizes = new float[BatchSize];
    private readonly float[] isChickensHighlighting = new float[BatchSize];

    // Reuse collections across frames to achieve ZERO GC in Update/Render
    private readonly Dictionary<ChickenBatchKey, List<int>> batchBuffers = new();
    private readonly List<List<int>> listPool = new();

    private MaterialPropertyBlock propertyBlock;

    private static readonly int AnimDataID = Shader.PropertyToID("_AnimData");
    private static readonly int AnimStartFrame = Shader.PropertyToID("_AnimStartFrame");
    private static readonly int SpriteSize = Shader.PropertyToID("_SpriteSize");
    private static readonly int Highlight = Shader.PropertyToID("_Highlight");

    private void Awake()
    {
        SettingPopup.replayLevelEvent += Reset;
        LosePopup.replayLevelEvent += Reset;
        DebugPopup.switchLevelEvent += Reset;
        WinPopup.resetLevelEvent += Reset;

        propertyBlock = new MaterialPropertyBlock();
    }

    private void OnDestroy()
    {
        SettingPopup.replayLevelEvent -= Reset;
        LosePopup.replayLevelEvent -= Reset;
        DebugPopup.switchLevelEvent -= Reset;
        WinPopup.resetLevelEvent -= Reset;
    }

    private void Update()
    {
        Render();
    }

    public void Reset()
    {
        chickenDataItems.Clear();
        dataOfChicken.Clear();

        Array.Clear(matrices, 0, matrices.Length);
        Array.Clear(animData, 0, animData.Length);
        Array.Clear(animStartFrame, 0, animStartFrame.Length);
        Array.Clear(spriteSizes, 0, spriteSizes.Length);
        Array.Clear(isChickensHighlighting, 0, isChickensHighlighting.Length);
    }

    private void Render()
    {
        ClearBatchBuffers();

        // Group chickens without allocating 'new Dictionary' or 'new List' every frame
        int totalItems = chickenDataItems.Count;
        for (int i = 0; i < totalItems; i++)
        {
            ChickenData chicken = chickenDataItems[i];
            ChickenBatchKey key = new(chicken.factionIndex, chicken.anim);

            if (!batchBuffers.TryGetValue(key, out var list))
            {
                list = GetPooledList();
                batchBuffers.Add(key, list);
            }

            list.Add(i);
        }

        // Draw each group
        foreach (var batch in batchBuffers)
        {
            DrawBatch(batch.Key, batch.Value);
        }

        RenderShadow();
    }

    private void RenderShadow()
    {
        var chickens = ChickenManager.Ins.chickens;
        int maxShadows = shadowMatrices.Length;
        int index = 0;
        int shadowScale = 1;

        for (int i = 0; i < chickens.Count; i++)
        {
            if (index >= maxShadows) break;

            BaseChicken chicken = chickens[i];
            if (chicken.isDone || !chicken.isStuck) continue;

            shadowScale = chicken.numColumnTaken;

            Vector3 position = chicken.transform.position + shadowOffset;
            shadowMatrices[index] = Matrix4x4.TRS(position, Quaternion.identity, shadowScale * shadowSize);
            index++;
        }

        // Draw only active shadows directly (avoids writing off-screen matrices)
        if (index > 0)
        {
            Graphics.DrawMeshInstanced(
                chickenMesh,
                0,
                shadowMaterial,
                shadowMatrices,
                index
            );
        }
    }

    private void DrawBatch(ChickenBatchKey key, List<int> indexes)
    {
        MaterialsForAnim materialsForAnim = GetMaterialsForAnim(key);
        Material material = materialsForAnim.materials[key.factionIndex];
        float defaultSpriteSize = materialsForAnim.defaultSpriteSize;

        Vector3 localScale = chickenSize * Vector3.one;

        int start = 0;
        int totalIndexes = indexes.Count;

        while (start < totalIndexes)
        {
            int count = Mathf.Min(BatchSize, totalIndexes - start);

            for (int i = 0; i < count; i++)
            {
                ChickenData chicken = chickenDataItems[indexes[start + i]];

                matrices[i] = Matrix4x4.TRS(
                    chicken.position + offset,
                    Quaternion.identity,
                    localScale
                );

                animData[i] = new Vector4(0, chicken.factionIndex, 0, 0);
                animStartFrame[i] = chicken.animStartFrame;
                spriteSizes[i] = chicken.scale * defaultSpriteSize;
                isChickensHighlighting[i] = chicken.highlight;
            }

            propertyBlock.Clear();
            propertyBlock.SetVectorArray(AnimDataID, animData);
            propertyBlock.SetFloatArray(AnimStartFrame, animStartFrame);
            propertyBlock.SetFloatArray(SpriteSize, spriteSizes);
            propertyBlock.SetFloatArray(Highlight, isChickensHighlighting);

            Graphics.DrawMeshInstanced(
                chickenMesh,
                0,
                material,
                matrices,
                count,
                propertyBlock
            );

            start += count;
        }
    }

    // =====================================================
    // Public API
    // =====================================================

    public void RenderChickens(List<BaseChicken> chickens)
    {
        chickenDataItems.Clear();
        dataOfChicken.Clear();

        for (int i = 0; i < chickens.Count; i++)
        {
            BaseChicken chicken = chickens[i];
            if (!chicken.isStuck) continue;

            ChickenData chickenData = new ChickenData
            {
                position = chicken.transform.position,
                scale = chicken.numColumnTaken,
                factionIndex = FactionMapper.Ins.GetFactionIndex(chicken.faction),
                anim = chicken.currentAnimation,
                animStartFrame = UnityEngine.Random.Range(0, maxAnimationFrame),
                highlight = chicken.highlight
            };

            chickenDataItems.Add(chickenData);
            dataOfChicken.Add(chicken, chickenData);
        }
    }

    public void UpdateChickenRendering(BaseChicken chicken)
    {
        if (dataOfChicken.TryGetValue(chicken, out ChickenData chickenData))
        {
            chickenData.position = chicken.transform.position;
            chickenData.factionIndex = FactionMapper.Ins.GetFactionIndex(chicken.faction);
            chickenData.anim = chicken.currentAnimation;
            chickenData.highlight = chicken.highlight;
        }
    }

    public void RemoveChickenRendering(BaseChicken chicken)
    {
        if (dataOfChicken.TryGetValue(chicken, out ChickenData chickenData))
        {
            chickenDataItems.Remove(chickenData);
            dataOfChicken.Remove(chicken);
        }
    }

    private MaterialsForAnim GetMaterialsForAnim(ChickenBatchKey key)
    {
        for (int i = 0; i < materialsForAnims.Length; i++)
        {
            var materialsForAnim = materialsForAnims[i];
            if (string.Equals(key.anim, materialsForAnim.animation, StringComparison.Ordinal))
            {
                return materialsForAnim;
            }
        }

        return materialsForAnims[0];
    }

    // =====================================================
    // Internal Buffer Pooling (Prevents Frame-rate Spikes)
    // =====================================================

    private void ClearBatchBuffers()
    {
        foreach (var list in batchBuffers.Values)
        {
            list.Clear();
            listPool.Add(list);
        }
        batchBuffers.Clear();
    }

    private List<int> GetPooledList()
    {
        int poolCount = listPool.Count;
        if (poolCount > 0)
        {
            List<int> list = listPool[poolCount - 1];
            listPool.RemoveAt(poolCount - 1);
            return list;
        }
        return new List<int>(64);
    }
}

[Serializable]
public class MaterialsForAnim
{
    public string animation;
    public Material[] materials;
    public float defaultSpriteSize;
}
