using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using System;
using Unity.Collections.LowLevel.Unsafe;

[BurstCompile(CompileSynchronously = true)]
public struct CalculateStatsJob : IJobParallelFor
{
    [NativeDisableContainerSafetyRestriction, ReadOnly] public NativeArray<StatBlock> BaseStats;       // Count = Entities * StatType.Count
    [NativeDisableContainerSafetyRestriction, ReadOnly] public NativeArray<StatModifier> Modifiers;   // All active modifiers
    [NativeDisableContainerSafetyRestriction, ReadOnly] public NativeArray<int2> ModifierRanges;       // (Start, Length) per Entity

    [NativeDisableContainerSafetyRestriction, WriteOnly] public NativeArray<float> CalculatedStats;    // Output flat array

    public void Execute(int entityIndex)
    {
        int statCount = (int)StatType.Count;
        int baseOffset = entityIndex * statCount;

        // Temporary array on stack to accumulate totals
        Span<StatBlock> stats = stackalloc StatBlock[statCount];

        // 1. Copy base stats for this entity
        for (int i = 0; i < statCount; i++)
        {
            stats[i] = BaseStats[baseOffset + i];
            stats[i].PercentMulti = 1.0f; // Multiplicative base is 1.0
        }

        // 2. Accumulate modifiers
        int2 range = ModifierRanges[entityIndex];
        int modStart = range.x;
        int modCount = range.y;

        for (int i = 0; i < modCount; i++)
        {
            StatModifier mod = Modifiers[modStart + i];
            int statIdx = (int)mod.Stat;

            switch (mod.Type)
            {
                case ModifierType.Flat:
                    stats[statIdx].FlatAdd += mod.Value;
                    break;
                case ModifierType.PercentAdd:
                    stats[statIdx].PercentAdd += mod.Value;
                    break;
                case ModifierType.PercentMulti:
                    stats[statIdx].PercentMulti *= (1.0f + mod.Value);
                    break;
            }
        }

        // 3. Output evaluated values
        for (int i = 0; i < statCount; i++)
        {
            CalculatedStats[baseOffset + i] = stats[i].Calculate();
        }
    }
}

[BurstCompile]
public struct ApplyStatConversionJob : IJobParallelFor
{
    public NativeArray<float> CalculatedStats; // Read/Write
    public float ConversionRatio; // e.g., 0.10f

    public void Execute(int entityIndex)
    {
        int baseIdx = entityIndex * (int)StatType.Count;

        float armor = CalculatedStats[baseIdx + (int)StatType.Armor];
        CalculatedStats[baseIdx + (int)StatType.AttackDamage] += armor * ConversionRatio;
    }
}
