using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class IntegratedEnemyManager : MonoBehaviour
{
    [Header("Simulation Settings")]
    [SerializeField] private int enemyCount = 1000;
    [SerializeField] private float enemySpeed = 2.0f;
    [SerializeField] private Vector3 basePosition = Vector3.zero;

    [Header("Enemy Mesh Rendering")]
    [SerializeField] private Mesh enemyMesh;
    [SerializeField] private Material enemyMaterial;
    [SerializeField] private float enemySize = 0.25f;

    [Header("Health Bar Rendering")]
    [SerializeField] private Mesh quadMesh; // Standard 1x1 Quad
    [SerializeField] private Material healthBarMaterial;
    [SerializeField] private Vector3 barOffset = new Vector3(0, 0.4f, 0);
    [SerializeField] private Vector2 barScale = new Vector2(0.6f, 0.08f);
    [SerializeField] private float catchUpSpeed = 5.0f;

    // Core Data Buffers (DOD)
    private NativeArray<float3> positions;
    private NativeArray<StatBlock> baseStats;
    private NativeArray<StatModifier> modifiers;
    private NativeArray<int2> modifierRanges;

    // Evaluated Stats & Health State
    private NativeArray<float> calculatedStats; // [EnemyIndex * StatType.Count + StatIndex]
    private NativeArray<float> currentHP;        // Raw HP pool
    private NativeArray<float> targetHPPercent;  // Actual HP (0.0 to 1.0)
    private NativeArray<float> visualHPPercent;  // Lazy interpolating HP (0.0 to 1.0)

    // Reusable GC-Free Batch Arrays for DrawMeshInstanced
    private Matrix4x4[] enemyMatricesBatch = new Matrix4x4[1023];
    private Matrix4x4[] healthMatricesBatch = new Matrix4x4[1023];
    private float[] batchVisualHP = new float[1023];
    private float[] batchActualHP = new float[1023];

    private MaterialPropertyBlock healthPropertyBlock;
    private JobHandle simulationHandle;
    private JobHandle lastFrameJobHandle;

    private void Awake()
    {
        healthPropertyBlock = new MaterialPropertyBlock();

        int statCount = (int)StatType.Count;

        // Allocate Memory
        positions = new NativeArray<float3>(enemyCount, Allocator.Persistent);
        baseStats = new NativeArray<StatBlock>(enemyCount * statCount, Allocator.Persistent);
        calculatedStats = new NativeArray<float>(enemyCount * statCount, Allocator.Persistent);

        // Modifiers Setup (Empty initially for demo)
        modifiers = new NativeArray<StatModifier>(0, Allocator.Persistent);
        modifierRanges = new NativeArray<int2>(enemyCount, Allocator.Persistent);

        currentHP = new NativeArray<float>(enemyCount, Allocator.Persistent);
        targetHPPercent = new NativeArray<float>(enemyCount, Allocator.Persistent);
        visualHPPercent = new NativeArray<float>(enemyCount, Allocator.Persistent);

        InitializeData();
    }

    private void InitializeData()
    {
        var random = new Unity.Mathematics.Random(12345);
        int statCount = (int)StatType.Count;

        for (int i = 0; i < enemyCount; i++)
        {
            positions[i] = new float3(random.NextFloat(-50, 50), random.NextFloat(-50, 50), 0);

            // Initialize Base Stats (e.g. Armor = 10, AD = 15)
            int baseOffset = i * statCount;
            baseStats[baseOffset + (int)StatType.Armor] = new StatBlock { Base = 10f };
            baseStats[baseOffset + (int)StatType.AttackDamage] = new StatBlock { Base = 15f };

            // Setup Base Max HP (storing in FlatAdd for calculation simplicity)
            // Let's set initial Max HP to 100
            currentHP[i] = 100f;
            targetHPPercent[i] = 1.0f;
            visualHPPercent[i] = 1.0f;
        }
    }

    private void Update()
    {
        JobHandle pipelineHandle = lastFrameJobHandle;

        // 1. STAT JOB: Recalculate dirty/current stats across all enemies
        var statsJob = new CalculateStatsJob
        {
            BaseStats = baseStats,
            Modifiers = modifiers,
            ModifierRanges = modifierRanges,
            CalculatedStats = calculatedStats
        };
        pipelineHandle = statsJob.Schedule(enemyCount, 128);

        // 2. MOVEMENT JOB: Move enemies towards base
        var moveJob = new MoveEnemiesJob
        {
            positionsRead = positions,
            positionsWrite = positions,
            target = basePosition,
            speed = enemySpeed,
            deltaTime = Time.deltaTime
        };
        pipelineHandle = moveJob.Schedule(enemyCount, 256, pipelineHandle);

        // 3. LAZY HEALTH INTERPOLATION JOB: Smooth visual HP bars towards target HP
        var lazyHealthJob = new UpdateLazyHealthJob
        {
            CurrentVisualHP = visualHPPercent,
            TargetHP = targetHPPercent,
            CatchUpSpeed = catchUpSpeed,
            DeltaTime = Time.deltaTime
        };
        pipelineHandle = lazyHealthJob.Schedule(enemyCount, 256, pipelineHandle);

        var healthSimJob = new SimulateHealthChangesJob
        {
            TargetHPPercent = targetHPPercent,
            Seed = (uint)(UnityEngine.Time.frameCount + 1),
            ChangeProbability = 0.02f // 2% of enemies change health each frame
        };

        // Chain it into your job pipeline
        pipelineHandle = healthSimJob.Schedule(enemyCount, 256, pipelineHandle);

        // Wait for Jobs to complete before rendering pipeline
        lastFrameJobHandle = pipelineHandle;
        lastFrameJobHandle.Complete();

        // // 4. BATCH RENDER: Draws both Enemy Meshes and Health Quad Overlay
        RenderEnemiesAndHealth();

        // TriggerRandomHealthChanges();
    }

    private void RenderEnemiesAndHealth()
    {
        int remaining = enemyCount;
        int offset = 0;

        while (remaining > 0)
        {
            int batchSize = math.min(remaining, 1023);

            for (int i = 0; i < batchSize; i++)
            {
                int index = offset + i;
                Vector3 p = positions[index];

                // Enemy Transformation
                enemyMatricesBatch[i] = Matrix4x4.TRS(p, Quaternion.identity, Vector3.one * enemySize);

                // Health Bar Transformation (Offset vertically above enemy)
                Vector3 healthBarPos = p + barOffset;
                healthMatricesBatch[i] = Matrix4x4.TRS(healthBarPos, Quaternion.identity, new Vector3(barScale.x, barScale.y, 1f));

                // Instanced Health Data
                batchVisualHP[i] = visualHPPercent[index];
                batchActualHP[i] = targetHPPercent[index];
            }

            // Draw Enemy Meshes
            Graphics.DrawMeshInstanced(enemyMesh, 0, enemyMaterial, enemyMatricesBatch, batchSize);

            // Draw Health Bars via Instanced Material Properties
            healthPropertyBlock.SetFloatArray("_HealthPercent", batchVisualHP);
            healthPropertyBlock.SetFloatArray("_ActualPercent", batchActualHP);
            Graphics.DrawMeshInstanced(quadMesh, 0, healthBarMaterial, healthMatricesBatch, batchSize, healthPropertyBlock);

            offset += batchSize;
            remaining -= batchSize;
        }
    }

    // Example API: Taking Damage reads calculated Armor from Stat System
    public void ApplyDamage(int enemyIndex, float rawDamage)
    {
        int armorIndex = (enemyIndex * (int)StatType.Count) + (int)StatType.Armor;
        float currentArmor = calculatedStats[armorIndex];

        // LoL Damage Mitigation Formula: Damage Multiplier = 100 / (100 + Armor)
        float damageMultiplier = 100f / (100f + math.max(0, currentArmor));
        float mitigatedDamage = rawDamage * damageMultiplier;

        float maxHP = 100f; // Can be pulled from StatType if registered
        currentHP[enemyIndex] = math.max(0f, currentHP[enemyIndex] - mitigatedDamage);

        // Update Actual Health Ratio immediately (Visual bar will lazily catch up in Update)
        targetHPPercent[enemyIndex] = currentHP[enemyIndex] / maxHP;
    }

    private void OnDestroy()
    {
        simulationHandle.Complete();

        if (positions.IsCreated) positions.Dispose();
        if (baseStats.IsCreated) baseStats.Dispose();
        if (modifiers.IsCreated) modifiers.Dispose();
        if (modifierRanges.IsCreated) modifierRanges.Dispose();
        if (calculatedStats.IsCreated) calculatedStats.Dispose();
        if (currentHP.IsCreated) currentHP.Dispose();
        if (targetHPPercent.IsCreated) targetHPPercent.Dispose();
        if (visualHPPercent.IsCreated) visualHPPercent.Dispose();
    }

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

    [BurstCompile]
    public struct SimulateHealthChangesJob : IJobParallelFor
    {
        [NativeDisableContainerSafetyRestriction] public NativeArray<float> TargetHPPercent;
        public uint Seed;
        public float ChangeProbability; // e.g., 0.05f for 5% chance per frame

        public void Execute(int index)
        {
            // Unique random generator per index/thread
            var random = new Unity.Mathematics.Random(Seed + (uint)index * 71254u);

            if (random.NextFloat() < ChangeProbability)
            {
                // Randomly set health between 5% and 100%
                TargetHPPercent[index] = random.NextFloat(0.05f, 1.0f);
            }
        }
    }

    private void TriggerRandomHealthChanges()
    {
        var healthSimJob = new SimulateHealthChangesJob
        {
            TargetHPPercent = targetHPPercent,
            Seed = (uint)(UnityEngine.Time.frameCount + 1),
            ChangeProbability = 0.02f // 2% of enemies change health each frame
        };

        // Chain it into your job pipeline
        JobHandle simHandle = healthSimJob.Schedule(enemyCount, 256);
        simHandle.Complete();
    }
}
