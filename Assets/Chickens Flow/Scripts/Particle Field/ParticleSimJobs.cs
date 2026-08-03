using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace MyGame.Runtime
{
    internal static class CapsuleGeometry
    {
        public static void GetRadial(float2 pos, float2 center, float halfLength,
            out float r, out float2 radial, out float2 segPoint)
        {
            float clampedX = math.clamp(pos.x - center.x, -halfLength, halfLength);
            segPoint = center + new float2(clampedX, 0f);
            float2 offset = pos - segPoint;
            r = math.length(offset);
            radial = r > 1e-4f ? offset / r : new float2(0f, 1f);
        }

        public static float2 ClampToCapsule(float2 pos, float2 center, float innerHalfLength, float outerHalfLength,
            float innerRadius, float outerRadius, float gapHalfWidth, bool allowGap)
        {
            GetRadial(pos, center, innerHalfLength, out float rInner, out float2 radialInner, out float2 segInner);
            if (rInner < innerRadius) return segInner + radialInner * innerRadius;

            if (allowGap)
            {
                float absdx = math.abs(pos.x - center.x);
                if (absdx <= gapHalfWidth) return pos;
                if (pos.y > center.y + outerRadius) return pos;
            }

            GetRadial(pos, center, outerHalfLength, out float rOuter, out float2 radialOuter, out float2 segOuter);
            if (rOuter > outerRadius) return segOuter + radialOuter * outerRadius;
            return pos;
        }

        public static float2 PushOutOfWalls(float2 pos, float2 prevPos, in NativeArray<WallData> walls, int wallCount,
            float tangentFriction, out float2 newPrev)
        {
            newPrev = prevPos;
            for (int w = 0; w < wallCount; w++)
            {
                var wl = walls[w];
                float2 perp = new float2(-wl.dir.y, wl.dir.x);
                float2 local = pos - wl.center;
                float along = math.dot(local, wl.dir);
                float side = math.dot(local, perp);
                float pa = wl.halfSize.x - math.abs(along);
                float ps = wl.halfSize.y - math.abs(side);
                if (pa <= 0f || ps <= 0f) continue;

                float2 tangentAxis;
                if (pa < ps) { pos += wl.dir * (math.sign(along) * pa); tangentAxis = perp; }
                else { pos += perp * (math.sign(side) * ps); tangentAxis = wl.dir; }

                if (tangentFriction > 0f)
                {
                    float vt = math.dot(pos - newPrev, tangentAxis);
                    newPrev += tangentAxis * (vt * tangentFriction);
                }
            }
            return pos;
        }
    }

    internal struct ForceZoneData
    {
        public float2 center;
        public float2 halfSize;
        public float2 dir;
        public float force;
    }

    internal struct WallData
    {
        public float2 center;
        public float2 halfSize;
        public float2 dir;
    }

    [BurstCompile]
    internal struct IntegrateJob : IJobParallelFor
    {
        public NativeArray<float2> positions;
        public NativeArray<float2> prevPositions;
        [ReadOnly] public NativeArray<float> masses;
        public NativeArray<bool> settled;
        [ReadOnly] public NativeArray<ForceZoneData> zones;
        public int zoneCount;
        [ReadOnly] public NativeArray<WallData> walls;
        public int wallCount;

        public float2 center;
        public float innerHalfLength;
        public float outerHalfLength;
        public float innerRadius;
        public float outerRadius;
        public float gapHalfWidth;
        public float collisionAbsorption;
        public float gravity;
        public float damping;
        public float outsideCollisionAbsorption;
        public float outsideGravity;
        public float outsideDamping;
        public float outsideWallFriction;

        public float dt;
        public float maxStep;

        public void Execute(int i)
        {
            float2 pos = positions[i];
            float2 prev = prevPositions[i];
            float invMass = 1f / math.max(0.01f, masses[i]);

            bool outside = !settled[i];
            float gravityP = outside ? outsideGravity : gravity;
            float dampingP = outside ? outsideDamping : damping;
            float absorbP = outside ? outsideCollisionAbsorption : collisionAbsorption;
            float wallFrictionP = outside ? outsideWallFriction : 0f;

            float2 wind = float2.zero;
            for (int z = 0; z < zoneCount; z++)
            {
                var zn = zones[z];
                float2 local = pos - zn.center;
                float2 perp = new float2(-zn.dir.y, zn.dir.x);
                float along = math.dot(local, zn.dir);
                float side = math.dot(local, perp);
                if (math.abs(along) <= zn.halfSize.x && math.abs(side) <= zn.halfSize.y)
                    wind += zn.dir * zn.force;
            }

            float2 accel = wind * invMass + new float2(0f, -gravityP);

            float2 velocity = (pos - prev) * dampingP;
            float2 newPos = pos + velocity + accel * (dt * dt);

            float2 delta = newPos - pos;
            float len = math.length(delta);
            if (len > maxStep) delta *= maxStep / len;
            newPos = pos + delta;

            prevPositions[i] = pos;
            float2 preContainment = newPos;
            newPos = CapsuleGeometry.ClampToCapsule(newPos, center, innerHalfLength, outerHalfLength, innerRadius, outerRadius,
                gapHalfWidth, allowGap: outside);
            if (outside && newPos.y <= center.y + outerRadius) settled[i] = true;

            float2 finalPos = CapsuleGeometry.PushOutOfWalls(newPos, prevPositions[i], walls, wallCount, wallFrictionP, out float2 newPrev);
            prevPositions[i] = newPrev;

            float2 correction = finalPos - preContainment;
            prevPositions[i] += correction * absorbP;

            positions[i] = finalPos;
        }
    }

    [BurstCompile]
    internal struct BuildHashJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float2> positions;
        public float cellSize;
        public NativeParallelMultiHashMap<int, int>.ParallelWriter writer;

        public void Execute(int i)
        {
            int2 cell = (int2)math.floor(positions[i] / cellSize);
            writer.Add(Hash(cell), i);
        }

        public static int Hash(int2 cell) => cell.x * 92837111 ^ cell.y * 689287499;
    }

    [BurstCompile]
    internal struct ComputeDeltaJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float2> positions;
        [ReadOnly] public NativeArray<float> masses;
        [ReadOnly] public NativeParallelMultiHashMap<int, int> hash;
        public float cellSize;
        public float minDist;
        public float maxCorrection;
        public NativeArray<float2> deltas;

        public void Execute(int i)
        {
            float2 pos = positions[i];
            float mi = masses[i];
            int2 cell = (int2)math.floor(pos / cellSize);
            float2 accum = float2.zero;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int key = BuildHashJob.Hash(cell + new int2(dx, dy));
                    if (!hash.TryGetFirstValue(key, out int j, out var it)) continue;
                    do
                    {
                        if (j == i) continue;
                        float2 diff = pos - positions[j];
                        float dist = math.length(diff);
                        if (dist >= minDist || dist < 1e-5f) continue;
                        float2 normal = diff / dist;
                        float overlap = minDist - dist;
                        float mj = masses[j];
                        float share = mj / (mi + mj);
                        accum += normal * (overlap * share);
                    } while (hash.TryGetNextValue(out j, ref it));
                }
            }

            float len = math.length(accum);
            if (len > maxCorrection) accum *= maxCorrection / len;

            deltas[i] = accum;
        }
    }

    [BurstCompile]
    internal struct ApplyDeltaJob : IJobParallelFor
    {
        public NativeArray<float2> positions;
        [ReadOnly] public NativeArray<float2> deltas;
        public float2 center;
        public float innerHalfLength;
        public float outerHalfLength;
        public float innerRadius;
        public float outerRadius;
        public float gapHalfWidth;
        public NativeArray<bool> settled;
        [ReadOnly] public NativeArray<WallData> walls;
        public int wallCount;
        public NativeArray<float2> prevPositions;
        public float collisionAbsorption;
        public float outsideCollisionAbsorption;

        public void Execute(int i)
        {
            float2 preContainment = positions[i] + deltas[i];
            bool outside = !settled[i];
            float2 pos = CapsuleGeometry.ClampToCapsule(preContainment, center, innerHalfLength, outerHalfLength, innerRadius, outerRadius,
                gapHalfWidth, allowGap: outside);
            if (outside && pos.y <= center.y + outerRadius) settled[i] = true;

            float2 finalPos = CapsuleGeometry.PushOutOfWalls(pos, prevPositions[i], walls, wallCount, 0f, out _);

            float2 correction = finalPos - preContainment;
            prevPositions[i] += correction * (outside ? outsideCollisionAbsorption : collisionAbsorption);

            positions[i] = finalPos;
        }
    }
}
