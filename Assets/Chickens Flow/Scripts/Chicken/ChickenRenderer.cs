using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ChickenRenderer : MonoBehaviour
{
    [Header("Rendering")]
    [SerializeField] private Mesh chickenMesh;
    [SerializeField] private Material chickenMaterial;

    [Header("Customize")]
    public Vector3 offset;
    public float chickenSize;

    private const int BatchSize = 1023;

    private readonly List<Vector3> positions = new();
    private readonly List<bool> walking = new();

    private readonly Matrix4x4[] matrices = new Matrix4x4[BatchSize];
    private readonly Vector4[] animData = new Vector4[BatchSize];

    private MaterialPropertyBlock propertyBlock;

    private static readonly int AnimDataID = Shader.PropertyToID("_AnimData");

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
    }

    private void Update()
    {
        Render();
    }

    private void Render()
    {
        int total = positions.Count;
        int start = 0;

        while (start < total)
        {
            int count = Mathf.Min(BatchSize, total - start);

            for (int i = 0; i < count; i++)
            {
                int index = start + i;

                matrices[i] = Matrix4x4.TRS(
                    positions[index] + offset,
                    Quaternion.identity,
                    chickenSize * Vector3.one);

                // x = animation
                // 0 = Idle
                // 1 = Walk
                animData[i] = new Vector4(walking[index] ? 1f : 0f, 0, 0, 0);
            }

            propertyBlock.Clear();
            propertyBlock.SetVectorArray(AnimDataID, animData);

            Graphics.DrawMeshInstanced(
                chickenMesh,
                0,
                chickenMaterial,
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

    public int Count => positions.Count;

    public void Clear()
    {
        positions.Clear();
        walking.Clear();
    }

    public void AddChicken(Vector3 position, bool isWalking = false)
    {
        positions.Add(position);
        walking.Add(isWalking);
    }

    public void RemoveChicken(int index)
    {
        if ((uint)index >= positions.Count)
            return;

        positions.RemoveAt(index);
        walking.RemoveAt(index);
    }

    public void SetPosition(int index, Vector3 position)
    {
        if ((uint)index >= positions.Count)
            return;

        positions[index] = position;
    }

    public Vector3 GetPosition(int index)
    {
        if ((uint)index >= positions.Count)
            return Vector3.zero;

        return positions[index];
    }

    public void SetWalking(int index, bool isWalking)
    {
        if ((uint)index >= walking.Count)
            return;

        walking[index] = isWalking;
    }

    public bool IsWalking(int index)
    {
        if ((uint)index >= walking.Count)
            return false;

        return walking[index];
    }

    public void SetPositions(IList<Vector3> newPositions)
    {
        positions.Clear();
        walking.Clear();

        for (int i = 0; i < newPositions.Count; i++)
        {
            positions.Add(newPositions[i]);
            walking.Add(false);
        }
    }

    public void SetPositions(IList<Vector3> newPositions, IList<bool> walkingStates)
    {
        positions.Clear();
        walking.Clear();

        int count = Mathf.Min(newPositions.Count, walkingStates.Count);

        for (int i = 0; i < count; i++)
        {
            positions.Add(newPositions[i]);
            walking.Add(walkingStates[i]);
        }
    }

    public void SetAllWalking(bool isWalking)
    {
        for (int i = 0; i < walking.Count; i++)
            walking[i] = isWalking;
    }
}
