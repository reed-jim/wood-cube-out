using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace MyGame.Runtime
{
    [System.Serializable]
    public struct ForceZone
    {
        public string name;
        public Vector2 center;
        public Vector2 size;
        [Range(0f, 360f)] public float angleDegrees;
        public float force;
    }

    [System.Serializable]
    public struct WarehouseWall
    {
        public string name;
        public Vector2 center;
        public Vector2 size;
        [Range(0f, 360f)] public float angleDegrees;
    }

    [System.Serializable]
    public struct MouthConfig
    {
        public string name;
        public Vector2 center;
        public float radius;
    }

    public sealed class ParticleField : MonoBehaviour
    {
        [Header("Hình học lồng — 2 capsule nằm ngang, mỗi capsule tự chỉnh độ dài riêng (local space)")]
        [SerializeField] private Vector2 center = Vector2.zero;
        [SerializeField] private float innerHalfLength = 2.0f;
        [SerializeField] private float outerHalfLength = 2.0f;
        [SerializeField] private float innerRadius = 1.5f;
        [SerializeField] private float outerRadius = 3.0f;

        [Header("Vật lý hạt (Burst: Verlet integration + PBD va chạm)")]
        [SerializeField] private int maxParticles = 1500;
        [SerializeField] private float particleRadius = 0.09f;
        [SerializeField] private float gravity = 9f;
        [SerializeField] private float damping = 0.995f;
        [SerializeField] private float minMass = 0.7f;
        [SerializeField] private float maxMass = 1.4f;
        [SerializeField] private float maxStepPerFrame = 0.5f;
        [SerializeField] private int collisionIterations = 6;
        [SerializeField] private float maxPbdCorrection = 0.15f;
        [SerializeField] private float spawnJitterRadius = 0.12f;
        [Range(0f, 1f)]
        [SerializeField] private float collisionAbsorption = 0.85f;
        [Range(0.5f, 3f)]
        [SerializeField] private float simulationSpeed = 1f;

        [Header("Thua")]
        [SerializeField] private float loseGraceSeconds = 5f;

        [Header("Vật lý pha NGOÀI kho")]
        [SerializeField] private float outsideGravity = 9f;
        [SerializeField] private float outsideDamping = 0.999f;
        [Range(0f, 1f)]
        [SerializeField] private float outsideCollisionAbsorption = 0.85f;
        [Range(0f, 1f)]
        [SerializeField] private float outsideWallFriction = 0.05f;

        [Header("Vùng lực — tự đặt hướng quanh vành (local space)")]
        [SerializeField] private List<ForceZone> forceZones = new List<ForceZone>();

        [Header("Tường/vật cản rắn — tự đặt trong khoang (local space)")]
        [SerializeField] private List<WarehouseWall> walls = new List<WarehouseWall>();

        [Header("Miệng truck — chỉnh tay vị trí + size từng miệng (miệng thứ i = cột i)")]
        [SerializeField] private List<MouthConfig> mouths = new List<MouthConfig>();
        [SerializeField] private float mouthRadius = 0.45f;
        [SerializeField] private float flyDuration = 0.35f;

        [Header("Cửa vào (đỉnh vành ngoài) — phễu dẫn hạt do bạn tự dựng bằng wall")]
        [SerializeField] private float gapHalfWidth = 0.3f;

        [Header("Sound hạt vào truck")]
        [SerializeField] private AudioSource particleEnterSoundSource;
        [SerializeField] private float particleEnterSoundCooldown = 0.06f;

        private float lastParticleEnterSoundTime = -999f;

        [Header("Hiệu ứng mảnh vỡ kính (cosmetic, ParticleSystem riêng — KHÔNG phải hạt sim NativeArray)")]
        [SerializeField] private ParticleSystem shardBurstSystem;
        [SerializeField] private int shardsPerEmit = 1;
        [SerializeField] private float shardHorizontalSpeedMin = 1.5f;
        [SerializeField] private float shardHorizontalSpeedMax = 3.5f;
        [SerializeField] private float shardVerticalSpeedMin = 0.5f;
        [SerializeField] private float shardVerticalSpeedMax = 2f;

        [Header("Hiệu ứng burst lúc block hàng xóm unlock (ParticleSystem riêng, tách khỏi shard vỡ kính để tự tinh chỉnh)")]
        [SerializeField] private ParticleSystem unlockBurstSystem;
        [SerializeField] private int unlockBurstsPerEmit = 1;
        [SerializeField] private float unlockBurstHorizontalSpeedMin = 1.5f;
        [SerializeField] private float unlockBurstHorizontalSpeedMax = 3.5f;
        [SerializeField] private float unlockBurstVerticalSpeedMin = 0.5f;
        [SerializeField] private float unlockBurstVerticalSpeedMax = 2f;

        internal const int GrainGrid = 4;
        internal const int GrainVariantCount = GrainGrid * GrainGrid;

        private int capacity = 600;
        private Color32[] palette;
        private int[] colorCount;
        private int total;
        private int enteredCount;
        private bool loseFired;
        private float loseGraceTimer;
        private bool loseCountdownActive;

        private NativeArray<float2> positions;
        private NativeArray<float2> prevPositions;
        private NativeArray<float2> renderPrevPositions;
        private NativeArray<float> masses;
        private NativeArray<int> colorsNative;
        private NativeArray<bool> settledNative;
        // Biến thể hình dạng (bóng khối + vết xước) mà ParticleRenderer bốc từ atlas cho từng hạt.
        // Cố định suốt đời hạt để vết xước không nhấp nháy khi hạt di chuyển.
        private NativeArray<byte> variants;
        private NativeArray<float2> deltas;
        private NativeParallelMultiHashMap<int, int> hashMap;
        private NativeArray<ForceZoneData> zonesNative;
        private NativeArray<WallData> wallsNative;
        private int activeCount;

        private readonly List<TruckColumnRuntime> columns = new List<TruckColumnRuntime>();
        private readonly List<Vector2> mouthPositions = new List<Vector2>();
        private readonly List<float> mouthRadii = new List<float>();
        private readonly List<(int particleIndex, int columnIndex)> pendingAbsorptions = new List<(int, int)>();
        private readonly HashSet<int> claimedThisFrame = new HashSet<int>();
        private readonly List<(int index, float d2)> mouthCandidates = new List<(int, float)>();

        private struct Flying { public float2 start, target, pos; public float t; public int color, columnIndex, tierIndex; public byte variant; }
        private readonly List<Flying> flying = new List<Flying>();
        private Vector2[] targetsLocal;

        public int Total => total;
        public int EnteredTotal => enteredCount;
        public int Capacity => capacity;
        public bool IsOverCapacity => total > capacity;

        public event Action OnLose;
        public event Action<int> OnParticleAbsorbed;
        public event Action<float> OnLoseCountdownTick;
        public event Action OnLoseCountdownCancelled;

        public void Configure(int capacity, int colorCountN)
        {
            this.capacity = capacity;
            colorCount = new int[Mathf.Max(0, colorCountN)];

            // SpawnParticleLocal chặn không cho total vượt maxParticles, nên đặt capacity >=
            // maxParticles làm IsOverCapacity vĩnh viễn false => điều kiện thua không bao giờ nổ.
            if (capacity >= maxParticles)
                Debug.LogWarning($"{nameof(ParticleField)}: warehouseCapacity ({capacity}) >= " +
                    $"maxParticles ({maxParticles}) nên người chơi không bao giờ thua vì quá tải kho. " +
                    "Đặt warehouseCapacity nhỏ hơn maxParticles nếu đây không phải ý đồ.");
        }

        public void SetPalette(Color32[] colors) => palette = colors;

        public void SetTruckTargets(Vector3[] worldByColumn)
        {
            if (worldByColumn == null) { targetsLocal = null; return; }
            targetsLocal = new Vector2[worldByColumn.Length];
            for (int c = 0; c < worldByColumn.Length; c++)
            {
                Vector3 local = transform.InverseTransformPoint(worldByColumn[c]);
                targetsLocal[c] = new Vector2(local.x, local.y);
            }
        }

        // public void RegisterColumn(TruckColumnRuntime col) => columns.Add(col);

        private Vector2 MouthLocal(int i, int n)
        {
            float midR = (innerRadius + outerRadius) * 0.5f;
            float t = n <= 1 ? 0.5f : i / (float)(n - 1);
            float x = Mathf.Lerp(-innerHalfLength * 0.8f, innerHalfLength * 0.8f, t);
            return center + new Vector2(x, -midR);
        }

        private void Awake()
        {
            positions = new NativeArray<float2>(maxParticles, Allocator.Persistent);
            prevPositions = new NativeArray<float2>(maxParticles, Allocator.Persistent);
            renderPrevPositions = new NativeArray<float2>(maxParticles, Allocator.Persistent);
            masses = new NativeArray<float>(maxParticles, Allocator.Persistent);
            colorsNative = new NativeArray<int>(maxParticles, Allocator.Persistent);
            settledNative = new NativeArray<bool>(maxParticles, Allocator.Persistent);
            variants = new NativeArray<byte>(maxParticles, Allocator.Persistent);
            deltas = new NativeArray<float2>(maxParticles, Allocator.Persistent);
            hashMap = new NativeParallelMultiHashMap<int, int>(maxParticles, Allocator.Persistent);
        }

        private void OnDestroy()
        {
            if (positions.IsCreated) positions.Dispose();
            if (prevPositions.IsCreated) prevPositions.Dispose();
            if (renderPrevPositions.IsCreated) renderPrevPositions.Dispose();
            if (masses.IsCreated) masses.Dispose();
            if (colorsNative.IsCreated) colorsNative.Dispose();
            if (settledNative.IsCreated) settledNative.Dispose();
            if (variants.IsCreated) variants.Dispose();
            if (deltas.IsCreated) deltas.Dispose();
            if (hashMap.IsCreated) hashMap.Dispose();
            if (zonesNative.IsCreated) zonesNative.Dispose();
            if (wallsNative.IsCreated) wallsNative.Dispose();
        }

        public void SpawnParticle(int color)
        {
            float spawnX = UnityEngine.Random.Range(-gapHalfWidth * 0.8f, gapHalfWidth * 0.8f);
            SpawnParticleLocal(color, new float2(center.x + spawnX, center.y + outerRadius + 0.3f));
        }

        public void SpawnParticleAtWorld(int color, Vector3 worldPos)
        {
            Vector3 local = transform.InverseTransformPoint(worldPos);
            SpawnParticleLocal(color, new float2(local.x, local.y));
        }

        public void EmitShardBurst(int color, Vector3 worldPos)
        {
            if (shardBurstSystem == null || palette == null || color < 0 || color >= palette.Length) return;

            float sign = UnityEngine.Random.value < 0.5f ? -1f : 1f;
            float vx = sign * UnityEngine.Random.Range(shardHorizontalSpeedMin, shardHorizontalSpeedMax);
            float vy = UnityEngine.Random.Range(shardVerticalSpeedMin, shardVerticalSpeedMax);

            var emitParams = new ParticleSystem.EmitParams
            {
                position = worldPos,
                startColor = palette[color],
                velocity = new Vector3(vx, vy, 0f)
            };
            shardBurstSystem.Emit(emitParams, shardsPerEmit);
        }

        public void EmitUnlockBurst(int color, Vector3 worldPos)
        {
            if (unlockBurstSystem == null || palette == null || color < 0 || color >= palette.Length) return;

            float sign = UnityEngine.Random.value < 0.5f ? -1f : 1f;
            float vx = sign * UnityEngine.Random.Range(unlockBurstHorizontalSpeedMin, unlockBurstHorizontalSpeedMax);
            float vy = UnityEngine.Random.Range(unlockBurstVerticalSpeedMin, unlockBurstVerticalSpeedMax);

            var emitParams = new ParticleSystem.EmitParams
            {
                position = worldPos,
                startColor = palette[color],
                velocity = new Vector3(vx, vy, 0f)
            };
            unlockBurstSystem.Emit(emitParams, unlockBurstsPerEmit);
        }

        public void SpawnParticleLocal(int color, float2 localPos)
        {
            if (colorCount == null || color < 0 || color >= colorCount.Length) return;
            if (activeCount >= maxParticles)
            {
                Debug.LogWarning($"{nameof(ParticleField)}: đã đạt maxParticles ({maxParticles}), bỏ qua hạt mới.");
                return;
            }

            if (spawnJitterRadius > 0f)
            {
                Vector2 jitter = UnityEngine.Random.insideUnitCircle * spawnJitterRadius;
                localPos += new float2(jitter.x, jitter.y);
            }

            int idx = activeCount;
            positions[idx] = localPos;
            prevPositions[idx] = localPos;
            masses[idx] = UnityEngine.Random.Range(minMass, maxMass);
            colorsNative[idx] = color;
            settledNative[idx] = false;
            variants[idx] = (byte)UnityEngine.Random.Range(0, GrainVariantCount);

            activeCount++;
            total++;
            colorCount[color]++;
        }

        private float stepAccumulator;

        private void FixedUpdate()
        {
            if (colorCount == null) return;

            NativeArray<float2>.Copy(positions, renderPrevPositions, maxParticles);

            stepAccumulator += simulationSpeed;
            while (stepAccumulator >= 1f)
            {
                if (activeCount > 0) StepSimulation(Time.fixedDeltaTime);

                ResolveAbsorption();
                RecomputeEnteredCount();
                TickLoseCondition(Time.fixedDeltaTime);
                stepAccumulator -= 1f;
            }
        }

        private void Update()
        {
            UpdateFlying();
        }

        private void SyncZones()
        {
            int n = forceZones.Count;
            int cap = Mathf.Max(1, n);
            if (!zonesNative.IsCreated || zonesNative.Length != cap)
            {
                if (zonesNative.IsCreated) zonesNative.Dispose();
                zonesNative = new NativeArray<ForceZoneData>(cap, Allocator.Persistent);
            }
            for (int i = 0; i < n; i++)
            {
                var z = forceZones[i];
                float rad = z.angleDegrees * Mathf.Deg2Rad;
                zonesNative[i] = new ForceZoneData
                {
                    center = new float2(z.center.x, z.center.y),
                    halfSize = new float2(z.size.x * 0.5f, z.size.y * 0.5f),
                    dir = new float2(math.cos(rad), math.sin(rad)),
                    force = z.force
                };
            }
        }

        private void SyncWalls()
        {
            int n = walls.Count;
            int cap = Mathf.Max(1, n);
            if (!wallsNative.IsCreated || wallsNative.Length != cap)
            {
                if (wallsNative.IsCreated) wallsNative.Dispose();
                wallsNative = new NativeArray<WallData>(cap, Allocator.Persistent);
            }
            for (int i = 0; i < n; i++)
            {
                var wl = walls[i];
                float rad = wl.angleDegrees * Mathf.Deg2Rad;
                wallsNative[i] = new WallData
                {
                    center = new float2(wl.center.x, wl.center.y),
                    halfSize = new float2(wl.size.x * 0.5f + particleRadius, wl.size.y * 0.5f + particleRadius),
                    dir = new float2(math.cos(rad), math.sin(rad))
                };
            }
        }

        private void StepSimulation(float dt)
        {
            SyncZones();
            SyncWalls();

            float cellSize = particleRadius * 2f;
            float minDist = particleRadius * 2f;
            var c2 = new float2(center.x, center.y);

            var handle = new IntegrateJob
            {
                positions = positions,
                prevPositions = prevPositions,
                masses = masses,
                settled = settledNative,
                zones = zonesNative,
                zoneCount = forceZones.Count,
                walls = wallsNative,
                wallCount = walls.Count,
                center = c2,
                innerHalfLength = innerHalfLength,
                outerHalfLength = outerHalfLength,
                innerRadius = innerRadius,
                outerRadius = outerRadius,
                gapHalfWidth = gapHalfWidth,
                collisionAbsorption = collisionAbsorption,
                gravity = gravity,
                damping = damping,
                outsideCollisionAbsorption = outsideCollisionAbsorption,
                outsideGravity = outsideGravity,
                outsideDamping = outsideDamping,
                outsideWallFriction = outsideWallFriction,
                dt = dt,
                maxStep = maxStepPerFrame
            }.Schedule(activeCount, 64);

            hashMap.Clear();
            handle = new BuildHashJob
            {
                positions = positions,
                cellSize = cellSize,
                writer = hashMap.AsParallelWriter()
            }.Schedule(activeCount, 64, handle);

            for (int iter = 0; iter < collisionIterations; iter++)
            {
                handle = new ComputeDeltaJob
                {
                    positions = positions,
                    masses = masses,
                    hash = hashMap,
                    cellSize = cellSize,
                    minDist = minDist,
                    maxCorrection = maxPbdCorrection,
                    deltas = deltas
                }.Schedule(activeCount, 64, handle);

                handle = new ApplyDeltaJob
                {
                    positions = positions,
                    deltas = deltas,
                    center = c2,
                    innerHalfLength = innerHalfLength,
                    outerHalfLength = outerHalfLength,
                    innerRadius = innerRadius,
                    outerRadius = outerRadius,
                    gapHalfWidth = gapHalfWidth,
                    settled = settledNative,
                    walls = wallsNative,
                    wallCount = walls.Count,
                    prevPositions = prevPositions,
                    collisionAbsorption = collisionAbsorption,
                    outsideCollisionAbsorption = outsideCollisionAbsorption
                }.Schedule(activeCount, 64, handle);
            }

            handle.Complete();
        }

        // settled[i] chuyển true đúng lúc hạt outside băng qua cửa vào khoang (xem IntegrateJob/
        // ApplyDeltaJob) — quét lại mỗi bước để EnteredTotal luôn khớp trạng thái mới nhất (kể cả sau
        // khi ResolveAbsorption vừa swap-remove các hạt bị hút, nên không cần tự tay ± theo từng sự kiện).
        private void RecomputeEnteredCount()
        {
            int count = 0;
            for (int i = 0; i < activeCount; i++)
                if (settledNative[i]) count++;
            enteredCount = count;
        }

        private void ResolveAbsorption()
        {
            if (columns.Count == 0 || activeCount == 0) return;

            pendingAbsorptions.Clear();
            claimedThisFrame.Clear();

            for (int c = 0; c < columns.Count; c++)
            {
                var col = columns[c];
                int remaining = col.FrontRemaining;
                if (remaining <= 0) continue;
                int wantColor = col.FrontColor;
                float2 mouth = new float2(mouthPositions[c].x, mouthPositions[c].y);
                float r = mouthRadii[c];
                float mouthSqr = r * r;

                mouthCandidates.Clear();
                for (int i = 0; i < activeCount; i++)
                {
                    if (claimedThisFrame.Contains(i)) continue;
                    if (colorsNative[i] != wantColor) continue;
                    float2 closest = ClosestPointOnSegment(prevPositions[i], positions[i], mouth);
                    float dx = closest.x - mouth.x, dy = closest.y - mouth.y;
                    float d2 = dx * dx + dy * dy;
                    if (d2 > mouthSqr) continue;
                    mouthCandidates.Add((i, d2));
                }

                if (mouthCandidates.Count == 0) continue;
                mouthCandidates.Sort((a, b) => a.d2.CompareTo(b.d2));

                int take = Mathf.Min(remaining, mouthCandidates.Count);
                for (int k = 0; k < take; k++)
                {
                    int idx = mouthCandidates[k].index;
                    pendingAbsorptions.Add((idx, c));
                    claimedThisFrame.Add(idx);
                }
            }

            if (pendingAbsorptions.Count == 0) return;

            pendingAbsorptions.Sort((a, b) => b.particleIndex.CompareTo(a.particleIndex));
            foreach (var (particleIndex, columnIndex) in pendingAbsorptions)
                AbsorbParticle(particleIndex, columnIndex);
        }

        private void PlayParticleEnterSound()
        {
            if (particleEnterSoundSource == null) return;
            if (Time.time - lastParticleEnterSoundTime < particleEnterSoundCooldown) return;
            lastParticleEnterSoundTime = Time.time;
            particleEnterSoundSource.Play();
        }

        private void AbsorbParticle(int index, int columnIndex)
        {
            int color = colorsNative[index];
            float2 fromLocal = positions[index];
            byte variant = variants[index];

            if (!columns[columnIndex].TryReserve(out int tier)) return;

            int last = activeCount - 1;
            positions[index] = positions[last];
            prevPositions[index] = prevPositions[last];
            masses[index] = masses[last];
            colorsNative[index] = colorsNative[last];
            settledNative[index] = settledNative[last];
            // Phải swap cùng nhóm với các array trên: bỏ sót thì hạt còn lại sẽ đổi vết xước
            // ngay giữa màn chơi mỗi lần có hạt khác bị hút.
            variants[index] = variants[last];
            activeCount--;

            total--;
            colorCount[color]--;

            Vector2 target = GetTruckTargetLocal(columnIndex, fromLocal);
            flying.Add(new Flying
            {
                start = fromLocal,
                target = target,
                pos = fromLocal,
                t = 0f,
                color = color,
                columnIndex = columnIndex,
                tierIndex = tier,
                variant = variant
            });

            OnParticleAbsorbed?.Invoke(columnIndex);
        }

        private static float2 ClosestPointOnSegment(float2 a, float2 b, float2 p)
        {
            float2 ab = b - a;
            float lenSq = math.dot(ab, ab);
            if (lenSq < 1e-8f) return a;
            float t = math.clamp(math.dot(p - a, ab) / lenSq, 0f, 1f);
            return a + ab * t;
        }

        private Vector2 GetTruckTargetLocal(int columnIndex, float2 fallback)
        {
            if (targetsLocal != null && columnIndex >= 0 && columnIndex < targetsLocal.Length)
                return targetsLocal[columnIndex];
            return new Vector2(fallback.x, fallback.y);
        }

        private void UpdateFlying()
        {
            if (flying.Count == 0) return;
            float inv = flyDuration > 0f ? Time.deltaTime / flyDuration : 1f;

            for (int i = flying.Count - 1; i >= 0; i--)
            {
                var f = flying[i];
                f.t += inv;
                if (f.t >= 1f)
                {
                    if (f.columnIndex >= 0 && f.columnIndex < columns.Count)
                        columns[f.columnIndex].Confirm(f.tierIndex);
                    PlayParticleEnterSound();
                    flying.RemoveAt(i);
                    continue;
                }
                float e = f.t * f.t * (3f - 2f * f.t);
                f.pos = math.lerp(f.start, f.target, e);
                flying[i] = f;
            }
        }

        public bool IsDeadlockLose()
        {
            if (total <= capacity) return false;
            for (int c = 0; c < colorCount.Length; c++)
            {
                if (colorCount[c] <= 0) continue;
                for (int i = 0; i < columns.Count; i++)
                    if (columns[i].FrontColor == c) return false;
            }
            return true;
        }

        private void TickLoseCondition(float dt)
        {
            if (loseFired) return;

            if (IsDeadlockLose())
            {
                loseCountdownActive = true;
                loseGraceTimer += dt;
                float remaining = Mathf.Max(0f, loseGraceSeconds - loseGraceTimer);
                OnLoseCountdownTick?.Invoke(remaining);

                if (loseGraceTimer >= loseGraceSeconds)
                {
                    loseFired = true;
                    OnLose?.Invoke();
                }
            }
            else if (loseCountdownActive)
            {
                loseCountdownActive = false;
                loseGraceTimer = 0f;
                OnLoseCountdownCancelled?.Invoke();
            }
        }

        internal int ActiveCount => activeCount;
        internal NativeArray<float2> Positions => positions;
        internal NativeArray<float2> PrevPositions => renderPrevPositions;
        internal NativeArray<int> ParticleColors => colorsNative;
        internal NativeArray<byte> Variants => variants;
        internal Color32[] Palette => palette;
        internal float ParticleRadius => particleRadius;

        internal int FlyingCount => flying.Count;
        internal float2 FlyingPos(int i) => flying[i].pos;
        internal int FlyingColor(int i) => flying[i].color;
        internal byte FlyingVariant(int i) => flying[i].variant;

        public Vector3 DoorWorld =>
            transform.TransformPoint(new Vector3(center.x, center.y + outerRadius, 0f));

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (forceZones != null)
            {
                Gizmos.color = Color.yellow;
                foreach (var z in forceZones)
                    DrawForceZone(z);
            }
            if (walls != null)
            {
                Gizmos.color = Color.red;
                foreach (var wl in walls)
                    DrawRect(wl.center, wl.size, wl.angleDegrees);
            }
            DrawMouths();
        }

        private void DrawMouths()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f);
            if (mouthPositions != null && mouthPositions.Count > 0)
            {
                for (int i = 0; i < mouthPositions.Count; i++)
                    DrawMouthAt(mouthPositions[i], i < mouthRadii.Count ? mouthRadii[i] : mouthRadius);
            }
            else if (mouths != null && mouths.Count > 0)
            {
                foreach (var m in mouths)
                    DrawMouthAt(m.center, m.radius > 0f ? m.radius : mouthRadius);
            }
            else
            {
                for (int i = 0; i < 4; i++)
                    DrawMouthAt(MouthLocal(i, 4), mouthRadius);
            }
        }

        private void DrawMouthAt(Vector2 local, float radius)
        {
            Vector3 w = transform.TransformPoint(new Vector3(local.x, local.y, 0f));
            Gizmos.DrawWireSphere(w, radius);
        }

        [ContextMenu("Tạo 4 miệng mặc định (chia đều)")]
        private void CreateDefaultMouths()
        {
            mouths = new List<MouthConfig>();
            for (int i = 0; i < 4; i++)
                mouths.Add(new MouthConfig { name = $"Miệng {i}", center = MouthLocal(i, 4), radius = mouthRadius });
        }

        private void DrawRect(Vector2 rectCenter, Vector2 size, float angleDegrees)
        {
            Vector3 TP(Vector2 local) => transform.TransformPoint(new Vector3(local.x, local.y, 0f));

            float rad = angleDegrees * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            Vector2 perp = new Vector2(-dir.y, dir.x);
            Vector2 half = size * 0.5f;
            Vector2 c0 = rectCenter + dir * -half.x + perp * -half.y;
            Vector2 c1 = rectCenter + dir * half.x + perp * -half.y;
            Vector2 c2 = rectCenter + dir * half.x + perp * half.y;
            Vector2 c3 = rectCenter + dir * -half.x + perp * half.y;
            Gizmos.DrawLine(TP(c0), TP(c1));
            Gizmos.DrawLine(TP(c1), TP(c2));
            Gizmos.DrawLine(TP(c2), TP(c3));
            Gizmos.DrawLine(TP(c3), TP(c0));
        }

        private void DrawForceZone(ForceZone z)
        {
            Vector3 TP(Vector2 local) => transform.TransformPoint(new Vector3(local.x, local.y, 0f));

            float rad = z.angleDegrees * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            Vector2 perp = new Vector2(-dir.y, dir.x);

            Vector2 half = z.size * 0.5f;
            Vector2 c0 = z.center + dir * -half.x + perp * -half.y;
            Vector2 c1 = z.center + dir * half.x + perp * -half.y;
            Vector2 c2 = z.center + dir * half.x + perp * half.y;
            Vector2 c3 = z.center + dir * -half.x + perp * half.y;
            Gizmos.DrawLine(TP(c0), TP(c1));
            Gizmos.DrawLine(TP(c1), TP(c2));
            Gizmos.DrawLine(TP(c2), TP(c3));
            Gizmos.DrawLine(TP(c3), TP(c0));

            float arrowLen = Mathf.Min(half.x, half.y) * 0.9f;
            Vector2 tip = z.center + dir * arrowLen;
            Gizmos.DrawLine(TP(z.center), TP(tip));
            Vector2 back = -dir * (arrowLen * 0.25f);
            Vector2 wing = perp * (arrowLen * 0.15f);
            Gizmos.DrawLine(TP(tip), TP(tip + back + wing));
            Gizmos.DrawLine(TP(tip), TP(tip + back - wing));
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            DrawCapsule(center, innerHalfLength, innerRadius);
            DrawCapsule(center, outerHalfLength, outerRadius);
            DrawDoor();
        }

        private void DrawDoor()
        {
            Vector3 TP(Vector2 local) => transform.TransformPoint(new Vector3(local.x, local.y, 0f));
            float outerY = center.y + outerRadius;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(TP(new Vector2(center.x - gapHalfWidth, outerY)), TP(new Vector2(center.x + gapHalfWidth, outerY)));
            float tick = outerRadius * 0.15f;
            Gizmos.DrawLine(TP(new Vector2(center.x - gapHalfWidth, outerY)), TP(new Vector2(center.x - gapHalfWidth, outerY + tick)));
            Gizmos.DrawLine(TP(new Vector2(center.x + gapHalfWidth, outerY)), TP(new Vector2(center.x + gapHalfWidth, outerY + tick)));
        }

        private void DrawCapsule(Vector2 c, float halfLen, float r)
        {
            Vector3 TP(Vector2 local) => transform.TransformPoint(new Vector3(local.x, local.y, 0f));

            Gizmos.DrawLine(TP(new Vector2(c.x - halfLen, c.y + r)), TP(new Vector2(c.x + halfLen, c.y + r)));
            Gizmos.DrawLine(TP(new Vector2(c.x - halfLen, c.y - r)), TP(new Vector2(c.x + halfLen, c.y - r)));

            DrawArc(new Vector2(c.x + halfLen, c.y), r, -90f, 90f);
            DrawArc(new Vector2(c.x - halfLen, c.y), r, 90f, 270f);
        }

        private void DrawArc(Vector2 arcCenter, float r, float fromDeg, float toDeg)
        {
            const int seg = 24;
            Vector3 PointAt(float deg)
            {
                float rad = deg * Mathf.Deg2Rad;
                return transform.TransformPoint(new Vector3(
                    arcCenter.x + Mathf.Cos(rad) * r, arcCenter.y + Mathf.Sin(rad) * r, 0f));
            }

            Vector3 prev = PointAt(fromDeg);
            for (int i = 1; i <= seg; i++)
            {
                Vector3 cur = PointAt(Mathf.Lerp(fromDeg, toDeg, i / (float)seg));
                Gizmos.DrawLine(prev, cur);
                prev = cur;
            }
        }
#endif
    }
}
