using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class OptimizedHealthBarManager : MonoBehaviour
{
    [Header("Health Bar Setup")]
    [SerializeField] private Mesh quadMesh; // Standard 1x1 Unity Quad
    [SerializeField] private Material healthBarMaterial;
    [SerializeField] private Vector3 barOffset = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector2 barScale = new Vector2(0.8f, 0.1f);
    [SerializeField] private float catchUpSpeed = 5.0f;

    // Data buffers
    private NativeArray<float> targetHealths;   // e.g., 0.0 to 1.0
    private NativeArray<float> visualHealths;   // Interpolated 0.0 to 1.0

    // Rendering buffers (Reuse managed allocations to avoid GC)
    private Matrix4x4[] batchMatrices = new Matrix4x4[1023];
    private float[] batchVisualHP = new float[1023];
    private float[] batchActualHP = new float[1023];

    private MaterialPropertyBlock propertyBlock;

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
    }

    public void TickLazyHealth(NativeArray<float3> positions, JobHandle dependency)
    {
        // 1. Schedule health interpolation
        var lazyJob = new UpdateLazyHealthJob
        {
            CurrentVisualHP = visualHealths,
            TargetHP = targetHealths,
            CatchUpSpeed = catchUpSpeed,
            DeltaTime = Time.deltaTime
        };

        JobHandle healthHandle = lazyJob.Schedule(positions.Length, 256, dependency);
        healthHandle.Complete();

        // 2. Draw Healthbars
        DrawHealthBars(positions);
    }

    private void DrawHealthBars(NativeArray<float3> positions)
    {
        int total = positions.Length;
        int offset = 0;

        while (offset < total)
        {
            int count = math.min(total - offset, 1023);

            for (int i = 0; i < count; i++)
            {
                int index = offset + i;

                // Position offset above enemy head
                Vector3 barPos = (Vector3)positions[index] + barOffset;

                batchMatrices[i] = Matrix4x4.TRS(
                    barPos,
                    Quaternion.identity,
                    new Vector3(barScale.x, barScale.y, 1f)
                );

                batchVisualHP[i] = visualHealths[index];
                batchActualHP[i] = targetHealths[index];
            }

            // Set instance data
            propertyBlock.SetFloatArray("_HealthPercent", batchVisualHP);
            propertyBlock.SetFloatArray("_ActualPercent", batchActualHP);

            Graphics.DrawMeshInstanced(
                quadMesh,
                0,
                healthBarMaterial,
                batchMatrices,
                count,
                propertyBlock
            );

            offset += count;
        }
    }
}

[BurstCompile]
public struct UpdateLazyHealthJob : IJobParallelFor
{
    public NativeArray<float> CurrentVisualHP; // Reads/Writes visual state
    [NativeDisableContainerSafetyRestriction, ReadOnly] public NativeArray<float> TargetHP; // Actual stats system health
    public float CatchUpSpeed;
    public float DeltaTime;

    public void Execute(int index)
    {
        // Smooth damp / lerp catchup effect
        CurrentVisualHP[index] = math.lerp(
            CurrentVisualHP[index],
            TargetHP[index],
            CatchUpSpeed * DeltaTime
        );
    }
}
