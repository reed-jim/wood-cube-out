using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("Simulation")]
    [SerializeField] private int enemyCount = 1000;
    [SerializeField] private float enemySpeed = 2.0f;
    [SerializeField] private Vector2 spawnArea = new Vector2(100, 100);
    [SerializeField] private Vector3 basePosition = Vector3.zero;

    [Header("Rendering")]
    [SerializeField] private Mesh enemyMesh;
    [SerializeField] private Material enemyMaterial;
    [SerializeField] private float enemySize = 0.25f;

    private NativeArray<float3> positions;
    private NativeArray<float3> velocities;
    private NativeArray<Matrix4x4> matrices;

    private MoveEnemiesJob moveJob;
    private JobHandle jobHandle;

    private const int MaxInstancesPerDraw = 1023;

    [BurstCompile]
    private struct MoveEnemiesJob : IJobParallelFor
    {
        [NativeDisableContainerSafetyRestriction, ReadOnly] public NativeArray<float3> positionsRead;
        public NativeArray<float3> positionsWrite;

        public float3 target;
        public float speed;
        public float deltaTime;

        public void Execute(int index)
        {
            float3 position = positionsRead[index];

            float3 direction = target - position;
            float distanceSq = math.lengthsq(direction);

            if (distanceSq > 0.0001f)
            {
                direction *= math.rsqrt(distanceSq);

                position += direction * speed * deltaTime;
            }

            positionsWrite[index] = position;
        }
    }

    private void Start()
    {
        positions = new NativeArray<float3>(
            enemyCount,
            Allocator.Persistent
        );

        velocities = new NativeArray<float3>(
            enemyCount,
            Allocator.Persistent
        );

        matrices = new NativeArray<Matrix4x4>(
            enemyCount,
            Allocator.Persistent
        );

        Unity.Mathematics.Random random =
            new Unity.Mathematics.Random(12345);

        for (int i = 0; i < enemyCount; i++)
        {
            float x = random.NextFloat(
                -spawnArea.x * 0.5f,
                spawnArea.x * 0.5f
            );

            float y = random.NextFloat(
                -spawnArea.y * 0.5f,
                spawnArea.y * 0.5f
            );

            positions[i] = new float3(x, y, 0);
        }
    }

    private void Update()
    {
        ScheduleMovement();

        jobHandle.Complete();

        BuildMatrices();

        DrawEnemies();
    }

    private void ScheduleMovement()
    {
        moveJob = new MoveEnemiesJob
        {
            positionsRead = positions,
            positionsWrite = positions,
            target = basePosition,
            speed = enemySpeed,
            deltaTime = Time.deltaTime
        };

        jobHandle = moveJob.Schedule(
            enemyCount,
            256
        );
    }

    private void BuildMatrices()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            float3 p = positions[i];

            matrices[i] = Matrix4x4.TRS(
                new Vector3(p.x, p.y, p.z),
                Quaternion.identity,
                Vector3.one * enemySize
            );
        }
    }

    private void DrawEnemies()
    {
        int remaining = enemyCount;
        int offset = 0;

        while (remaining > 0)
        {
            int count = math.min(
                remaining,
                MaxInstancesPerDraw
            );

            // Graphics.DrawMeshInstanced requires a managed array.
            Matrix4x4[] batch = new Matrix4x4[count];

            for (int i = 0; i < count; i++)
            {
                batch[i] = matrices[offset + i];
            }

            Graphics.DrawMeshInstanced(
                enemyMesh,
                0,
                enemyMaterial,
                batch,
                count
            );

            offset += count;
            remaining -= count;
        }
    }

    private void OnDestroy()
    {
        if (jobHandle.IsCompleted == false)
            jobHandle.Complete();

        if (positions.IsCreated)
            positions.Dispose();

        if (velocities.IsCreated)
            velocities.Dispose();

        if (matrices.IsCreated)
            matrices.Dispose();
    }
}
