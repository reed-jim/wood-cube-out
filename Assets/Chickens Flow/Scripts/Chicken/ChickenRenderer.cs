using System;
using System.Collections.Generic;
using Saferio.Util.SaferioTween;
using UnityEngine;

public struct ChickenBatchKey
{
    public int factionIndex;
    public string anim;

    public ChickenBatchKey(int factionIndex, string anim)
    {
        this.factionIndex = factionIndex;
        this.anim = anim;
    }

    public override bool Equals(object obj)
    {
        if (obj is ChickenBatchKey other)
        {
            return factionIndex == other.factionIndex &&
                   anim == other.anim;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return System.HashCode.Combine(factionIndex, anim);
    }
}

public class ChickenData
{
    public Vector3 position;

    public int factionIndex;
    public string anim;

    public float animStartFrame;
}

public class ChickenRenderer : MonoBehaviour
{
    [Header("Rendering")]
    [SerializeField] private Mesh chickenMesh;
    // [SerializeField] private Material[] idleMaterials;
    // [SerializeField] private Material[] walkMaterials;
    [SerializeField] private MaterialsForAnim[] materialsForAnims;
    [SerializeField] private Material shadowMaterial;

    [Header("Customize")]
    public Vector3 offset;
    public Vector3 shadowOffset;
    public Vector3 shadowSize;
    public float chickenSize;
    public float maxAnimationFrame;

    private const int BatchSize = 1023;

    private readonly List<ChickenData> chickenDataItems = new();

    private readonly Matrix4x4[] matrices = new Matrix4x4[BatchSize];
    private readonly Matrix4x4[] shadowMatrices = new Matrix4x4[BatchSize];
    private readonly Vector4[] animData = new Vector4[BatchSize];
    private readonly float[] animStartFrame = new float[BatchSize];

    private Dictionary<BaseChicken, ChickenData> dataOfChicken = new();

    private MaterialPropertyBlock propertyBlock;

    private static readonly int AnimDataID = Shader.PropertyToID("_AnimData");

    private void Awake()
    {
        SettingPopup.replayLevelEvent += Reset;
        LosePopup.replayLevelEvent += Reset;
        DebugPopup.switchLevelEvent += Reset;

        propertyBlock = new MaterialPropertyBlock();
    }

    void OnDestroy()
    {
        SettingPopup.replayLevelEvent -= Reset;
        LosePopup.replayLevelEvent -= Reset;
        DebugPopup.switchLevelEvent -= Reset;
    }

    private void Update()
    {
        Render();
    }

    public void Reset()
    {
        chickenDataItems.Clear();
        dataOfChicken.Clear();

        System.Array.Clear(matrices, 0, matrices.Length);
        System.Array.Clear(animData, 0, animData.Length);
        System.Array.Clear(animStartFrame, 0, animStartFrame.Length);
    }

    private void Render()
    {
        Dictionary<ChickenBatchKey, List<int>> batches = new();

        // Group chickens
        for (int i = 0; i < chickenDataItems.Count; i++)
        {
            ChickenData chicken = chickenDataItems[i];

            ChickenBatchKey key = new(
                chicken.factionIndex,
                chicken.anim
            );

            if (!batches.TryGetValue(key, out var list))
            {
                list = new List<int>();
                batches.Add(key, list);
            }

            list.Add(i);
        }


        // Draw each group
        foreach (var batch in batches)
        {
            DrawBatch(
                batch.Key,
                batch.Value
            );
        }

        RenderShadow();
    }

    private void RenderShadow()
    {
        for (int i = 0; i < ChickenManager.Ins.chickens.Count; i++)
        {
            BaseChicken chicken = ChickenManager.Ins.chickens[i];

            if (chicken.isDone)
            {
                continue;
            }

            shadowMatrices[i] = Matrix4x4.TRS(
                    chicken.transform.position + shadowOffset,
                    Quaternion.identity,
                    shadowSize
                );
        }

        Graphics.DrawMeshInstanced(
                chickenMesh,
                0,
                shadowMaterial,
                shadowMatrices,
                chickenDataItems.Count
            );
    }

    private void DrawBatch(
        ChickenBatchKey key,
        List<int> indexes)
    {
        Material material = GetMaterial(key);

        int start = 0;

        while (start < indexes.Count)
        {
            int count = Mathf.Min(
                BatchSize,
                indexes.Count - start
            );

            for (int i = 0; i < count; i++)
            {
                ChickenData chicken =
                    chickenDataItems[indexes[start + i]];

                matrices[i] = Matrix4x4.TRS(
                    chicken.position + offset,
                    Quaternion.identity,
                    chickenSize * Vector3.one
                );

                // x = animation
                // y = faction
                animData[i] = new Vector4(
                    0,
                    chicken.factionIndex,
                    0,
                    0
                );

                animStartFrame[i] =
                    chicken.animStartFrame;
            }

            propertyBlock.Clear();

            propertyBlock.SetVectorArray(
                AnimDataID,
                animData
            );

            propertyBlock.SetFloatArray(
                "_AnimStartFrame",
                animStartFrame
            );

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

        for (int i = 0; i < chickens.Count; i++)
        {
            BaseChicken chicken = chickens[i];

            ChickenData chickenData = new ChickenData();

            chickenData.position = chicken.transform.position;
            chickenData.factionIndex = FactionMapper.Ins.GetFactionIndex(chicken.faction);
            chickenData.anim = chicken.currentAnimation;
            chickenData.animStartFrame = UnityEngine.Random.Range(0, maxAnimationFrame);

            chickenDataItems.Add(chickenData);

            dataOfChicken.Add(chicken, chickenData);
        }
    }

    public void UpdateChickenRendering(BaseChicken chicken)
    {
        if (dataOfChicken.ContainsKey(chicken))
        {
            ChickenData chickenData = dataOfChicken[chicken];

            chickenData.position = chicken.transform.position;
            chickenData.factionIndex = FactionMapper.Ins.GetFactionIndex(chicken.faction);
            chickenData.anim = chicken.currentAnimation;
        }
    }

    public void RemoveChickenRendering(BaseChicken chicken)
    {
        if (dataOfChicken.ContainsKey(chicken))
        {
            ChickenData chickenData = dataOfChicken[chicken];

            chickenDataItems.Remove(chickenData);
            dataOfChicken.Remove(chicken);
        }
    }

    private Material GetMaterial(ChickenBatchKey key)
    {
        foreach (var materialsForAnim in materialsForAnims)
        {
            if (key.anim == materialsForAnim.animation)
            {
                return materialsForAnim.materials[key.factionIndex];
            }
        }

        return materialsForAnims[0].materials[key.factionIndex];
    }
}

[Serializable]
public class MaterialsForAnim
{
    public string animation;
    public Material[] materials;
}
