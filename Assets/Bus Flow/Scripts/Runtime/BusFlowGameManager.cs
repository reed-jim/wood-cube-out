using System;
using System.Collections.Generic;
using System.Linq;
using AntsFlow;
using Dreamteck.Splines;
using UnityEngine;

namespace BusFlow
{
    // Orchestrator tổng: dựng path runtime từ LevelItem.busPaths, spawn/despawn xe trên map, luật
    // gộp 3 xe cùng màu, điều kiện thắng/thua, 3 booster. Tương đương vai trò của TileManager +
    // ChickenManager gộp lại nhưng cho Bus Flow, không phụ thuộc singleton của Ants Flow.
    public class BusFlowGameManager : Singleton<BusFlowGameManager>
    {
        [Header("Level")]
        public AntsFlow.LevelItem levelItem;
        public int extraPassengerRounds = 1;
        [Tooltip("Gốc chung áp cho CẢ khách lẫn spline (giữ 2 bên cùng hệ toạ độ). Dịch cả cụm map+đường bằng field này.")]
        public Vector3 boardOrigin = Vector3.zero;

        [Header("Auto Fit (co map vừa khung camera)")]
        [Tooltip("Tự co cả map + spline cho vừa vùng camera nhìn thấy lúc play. Các tỉ lệ fit nằm trên LevelItem (dùng chung với SimulateMap).")]
        public bool autoFitBoard = true;

        [Header("Bus")]
        public Material busFallbackMaterial;
        public int maxActiveBuses = 6;
        public float busSpeed = 3f;
        [Tooltip("Index xếp hạng màu khi BusGarageManager sinh xe mới: 0 = luôn chọn màu khách đang " +
                 "reachable nhiều nhất (dễ), cao hơn = cố tình bỏ qua màu đó, chọn xuống màu hiếm hơn (khó).")]
        public int busDifficultyLevel = 0;

        [Header("Road Visual (hiện đường spline trong game)")]
        public bool showRoad = true;
        [Tooltip("Material cho mặt đường. Để trống thì dùng material mặc định của SplineRenderer.")]
        public Material roadMaterial;
        public float roadWidth = 0.9f;
        public Color roadColor = new Color(0.55f, 0.55f, 0.6f, 1f);

        [Header("Lose condition")]
        [Tooltip("1 'vòng loop' tính là khoảng thời gian này (giây), không phải 1 vòng path thật.")]
        public float stallCycleSeconds = 5f;
        public int maxStallCycles = 4;

        public List<BaseBus> activeBuses;

        public static event Action winEvent;
        public static event Action loseEvent;

        public bool IsBooster1ModeActive { get; private set; }
        public bool IsBooster3ModeActive { get; private set; }

        private FactionMapper factionMapper;
        private RuntimeSplinePath pickupPathRuntime;
        private RuntimeSplinePath exitPathRuntime;

        private float stallTimer;
        private int stallCycleCount;
        private bool isGameOver;
        private int externalLoopBusCount;
        private Transform busPathRoot;
        private Vector3 effectiveOrigin;
        private float boardScale = 1f;

        protected override void MoreActionOnAwake()
        {
            base.MoreActionOnAwake();

            activeBuses = new List<BaseBus>();
        }

        private void Start()
        {
            // Test riêng (scene Bus Jam Color): gán sẵn levelItem trên Inspector rồi Play là chạy luôn.
            // Khi đi qua LevelLoader thật, levelItem chưa gán ở đây - LevelLoader tự gọi
            // StartLevel(item) sau khi build xong LevelItem từ JSON, guard null tránh chạy 2 lần/chạy
            // trước khi có data thật.
            if (levelItem != null)
            {
                StartLevel();
            }
        }

        // Entry point khi load qua LevelLoader thật (LevelMode.BusFlow): nhận levelItem vừa dựng từ
        // JSON thay vì đọc field Inspector.
        public void StartLevel(AntsFlow.LevelItem item)
        {
            levelItem = item;

            StartLevel();
        }

        private void OnDestroy()
        {
            PassengerBoardManager.allPassengersBoardedEvent -= OnAllPassengersBoarded;
        }

        private void Update()
        {
            if (isGameOver)
            {
                return;
            }

            CheckLoseByStall();
        }

        public void StartLevel()
        {
            if (levelItem == null)
            {
                Debug.LogError("BusFlowGameManager: chưa gán levelItem.");

                return;
            }

            isGameOver = false;
            stallTimer = 0f;
            stallCycleCount = 0;
            externalLoopBusCount = 0;
            IsBooster1ModeActive = false;
            IsBooster3ModeActive = false;

            for (int i = 0; i < activeBuses.Count; i++)
            {
                if (activeBuses[i] != null)
                {
                    Destroy(activeBuses[i].gameObject);
                }
            }

            activeBuses.Clear();

            factionMapper = FactionMapper.Ins;

            pickupPathRuntime?.Dispose();
            exitPathRuntime?.Dispose();

            ComputeBoardFit();

            BuildRuntimePaths();

            PassengerBoardManager.allPassengersBoardedEvent -= OnAllPassengersBoarded;
            PassengerBoardManager.allPassengersBoardedEvent += OnAllPassengersBoarded;

            PassengerBoardManager.Ins.StartLevel(levelItem, levelItem.busTileSpacing, extraPassengerRounds, factionMapper, effectiveOrigin, boardScale);

            ComputeGarageFit(out Vector3 garageOrigin, out float garageScale);
            BusGarageManager.Ins.StartLevel(levelItem, factionMapper, garageOrigin, garageScale);
        }

        // Tự đặt bãi đỗ ở đáy khung camera + co cho vừa bề rộng. Độc lập với map (map ở trên, bãi đỗ ở dưới).
        private void ComputeGarageFit(out Vector3 garageOrigin, out float garageScale)
        {
            if (!autoFitBoard)
            {
                garageOrigin = BusGarageManager.Ins.garageOrigin;
                garageScale = 1f;
                return;
            }

            BusBoardFit.Fit fit = BusBoardFit.ComputeGarage(
                Camera.main, levelItem.busGarageNumColumns, BusGarageManager.Ins.slotSpacing,
                BusGarageManager.Ins.garageOrigin, levelItem.garageWidthRatio, levelItem.garageViewportY);

            garageOrigin = fit.origin;
            garageScale = fit.scale;
        }

        // Tự co map (khách) + spline cho vừa khung camera. Đặt effectiveOrigin + boardScale để tâm map
        // rơi vào vùng nhìn thấy. Cả khách và spline dùng chung 2 giá trị này nên vẫn dính nhau.
        private void ComputeBoardFit()
        {
            if (!autoFitBoard)
            {
                effectiveOrigin = boardOrigin;
                boardScale = 1f;
                return;
            }

            BusBoardFit.Fit fit = BusBoardFit.ComputeBoard(
                Camera.main, levelItem.tiles, levelItem.busTileSpacing, boardOrigin,
                levelItem.fitWidthRatio, levelItem.fitHeightRatio, levelItem.fitCenterViewportY);

            effectiveOrigin = fit.origin;
            boardScale = fit.scale;
        }

        // Gốc + scale chung cho spline (khớp công thức khách). busPathRoot scale = boardScale để spline
        // co cùng map.
        private Transform GetOrCreateBusPathRoot()
        {
            if (busPathRoot == null)
            {
                busPathRoot = new GameObject("Bus Path Root").transform;
                busPathRoot.SetParent(transform, worldPositionStays: false);
            }

            busPathRoot.position = effectiveOrigin;
            busPathRoot.rotation = Quaternion.identity;
            busPathRoot.localScale = boardScale * Vector3.one;

            return busPathRoot;
        }

        // Test thủ công (kéo thẳng Level 1.prefab vào field Level Item): prefab có SplineComputer sống
        // trong busPaths, dùng luôn. Qua LevelLoader thật (LevelItem dựng từ JSON, không có spline
        // sống): fallback đọc điểm đã bake trong runtimeBusPathConfig.
        private void BuildRuntimePaths()
        {
            GetOrCreateBusPathRoot();

            if (!TryBuildFromLiveSplines())
            {
                TryBuildFromBakedConfig();
            }

            if (showRoad)
            {
                pickupPathRuntime?.ShowRoad(roadMaterial, roadWidth, roadColor);
                exitPathRuntime?.ShowRoad(roadMaterial, roadWidth, roadColor);
            }
        }

        private bool TryBuildFromLiveSplines()
        {
            BusPathAuthoring pickupAuthoring = levelItem.busPaths?.FirstOrDefault(p => p != null && p.role == BusPathRole.Pickup && p.spline != null);

            if (pickupAuthoring == null)
            {
                return false;
            }

            BusPathAuthoring exitAuthoring = levelItem.busPaths?.FirstOrDefault(p => p != null && p.role == BusPathRole.Exit && p.spline != null);

            // Đường đón khách role Pickup luôn là vòng loop
            bool isPickupLoop = pickupAuthoring.isLoop || pickupAuthoring.spline.isClosed || pickupAuthoring.role == BusPathRole.Pickup;
            pickupPathRuntime = new RuntimeSplinePath(busPathRoot, GetLocalPoints(pickupAuthoring.spline), isPickupLoop);

            if (exitAuthoring != null)
            {
                exitPathRuntime = new RuntimeSplinePath(busPathRoot, GetLocalPoints(exitAuthoring.spline), exitAuthoring.isLoop);
            }

            return true;
        }

        private void TryBuildFromBakedConfig()
        {
            BusPathItemConfig pickupConfig = levelItem.runtimeBusPathConfig?.paths?.FirstOrDefault(p => p != null && p.role == BusPathRole.Pickup);
            BusPathItemConfig exitConfig = levelItem.runtimeBusPathConfig?.paths?.FirstOrDefault(p => p != null && p.role == BusPathRole.Exit);

            if (pickupConfig != null)
            {
                bool isPickupLoop = pickupConfig.isLoop || pickupConfig.role == BusPathRole.Pickup;
                pickupPathRuntime = new RuntimeSplinePath(busPathRoot, pickupConfig.points, isPickupLoop);
            }
            else
            {
                Debug.LogWarning("BusFlowGameManager: levelItem chưa có bus path role=Pickup (đường đón khách).");
            }

            if (exitConfig != null)
            {
                exitPathRuntime = new RuntimeSplinePath(busPathRoot, exitConfig.points, exitConfig.isLoop);
            }
        }

        private List<Vector3> GetLocalPoints(SplineComputer spline)
        {
            SplinePoint[] points = spline.GetPoints();
            List<Vector3> result = new List<Vector3>(points.Length);

            for (int i = 0; i < points.Length; i++)
            {
                // spline.GetPoints() returns world coordinates. Convert to local space of busPathRoot (parent of RuntimeSplinePath).
                result.Add(busPathRoot != null ? busPathRoot.InverseTransformPoint(points[i].position) : transform.InverseTransformPoint(points[i].position));
            }

            return result;
        }

        public bool CanActivateMoreBuses()
        {
            int active = activeBuses.Count(b => b != null && b.state != BusFlowBusState.Done) + externalLoopBusCount;

            return active < maxActiveBuses;
        }

        public void IncreaseMaxActiveBuses(int amount)
        {
            maxActiveBuses += amount;
        }

        // Đường loop (role 0) cho WashingMachineItem-bus dùng khi được pick (LaunchOntoLoop).
        public BusFlow.RuntimeSplinePath GetLoopPath()
        {
            return pickupPathRuntime;
        }

        // WashingMachineItem chạy loop không phải BusFlowBusController nên đếm riêng để giữ giới hạn max.
        public void RegisterLoopBus()
        {
            externalLoopBusCount++;
        }

        public void UnregisterLoopBus()
        {
            externalLoopBusCount = Mathf.Max(0, externalLoopBusCount - 1);
        }

        public void SpawnBusOnMap(Color faction, int number, Vector3 garageWorldPosition)
        {
            if (pickupPathRuntime == null)
            {
                Debug.LogError("SpawnBusOnMap: chưa có bus path role=0, không thể chạy xe.");

                return;
            }

            GameObject prefab = BusGarageManager.Ins != null ? BusGarageManager.Ins.busPrefab : null;

            BaseBus controller;
            if (prefab != null)
            {
                controller = BaseBus.CreateFromPrefab(prefab, transform);
                float fitScale = BusGarageManager.Ins != null ? BusGarageManager.Ins.FitGarageScale : 1f;
                float visualScale = BusGarageManager.Ins != null ? BusGarageManager.Ins.busVisualScale : 2.2f;
                controller.transform.localScale = visualScale * fitScale * Vector3.one;
            }
            else
            {
                controller = BaseBus.CreatePlaceholder(transform);
            }

            Material overrideMaterial = factionMapper != null ? factionMapper.GetBusMaterial(faction) : null;

            if (overrideMaterial == null)
            {
                overrideMaterial = busFallbackMaterial;
            }

            controller.Init(faction, number, pickupPathRuntime, exitPathRuntime, busSpeed, overrideMaterial, garageWorldPosition);

            activeBuses.Add(controller);

            CheckMergeRule();
        }

        public void OnBusExited(BaseBus bus)
        {
            activeBuses.Remove(bus);
        }

        public void NotifyProgress()
        {
            stallCycleCount = 0;
            stallTimer = 0f;
        }

        // "3 xe cùng màu đang được pick" -> diễn cho 3 bus đứng lại, 2 bus bị gộp di chuyển về phía survivor,
        // tới nơi survivor scale to lên rồi về bình thường, 2 bus bị gộp biến mất, cập nhật tổng remaining và survivor tiếp tục chạy.
        public void CheckMergeRule()
        {
            Dictionary<Color, List<BaseBus>> byFaction = new Dictionary<Color, List<BaseBus>>();

            for (int i = 0; i < activeBuses.Count; i++)
            {
                BaseBus bus = activeBuses[i];

                if (bus == null || (bus.state != BusFlowBusState.DrivingPickup && bus.state != BusFlowBusState.Launching))
                {
                    continue;
                }

                if (!byFaction.TryGetValue(bus.faction, out List<BaseBus> list))
                {
                    list = new List<BaseBus>();
                    byFaction[bus.faction] = list;
                }

                list.Add(bus);
            }

            foreach (List<BaseBus> group in byFaction.Values)
            {
                while (group.Count >= 3)
                {
                    List<BaseBus> triple = group.Take(3).ToList();

                    group.RemoveRange(0, 3);

                    // Ưu tiên chọn xe đang chạy trên loop (DrivingPickup) làm survivor đón 2 xe mới vào
                    BaseBus survivor = triple.OrderByDescending(b => b.state == BusFlowBusState.DrivingPickup)
                                             .ThenByDescending(b => b.remaining)
                                             .First();

                    int mergedTotal = triple.Sum(b => b.remaining);
                    List<BaseBus> absorbedBuses = triple.Where(b => b != survivor).ToList();

                    // Cả 3 xe đứng lại
                    survivor.PauseDriving();
                    for (int i = 0; i < absorbedBuses.Count; i++)
                    {
                        absorbedBuses[i].PauseDriving();
                        activeBuses.Remove(absorbedBuses[i]);
                    }

                    // 2 xe bị gộp di chuyển về phía survivor
                    float mergeDuration = 0.4f;
                    int completedCount = 0;

                    for (int i = 0; i < absorbedBuses.Count; i++)
                    {
                        absorbedBuses[i].AnimateMergeInto(survivor.transform.position, mergeDuration, () =>
                        {
                            completedCount++;
                            if (completedCount >= absorbedBuses.Count)
                            {
                                if (survivor != null)
                                {
                                    survivor.PlayMergedSuccessEffect(mergedTotal);
                                }
                            }
                        });
                    }
                }
            }
        }

        private void CheckLoseByStall()
        {
            bool atMaxActive = !CanActivateMoreBuses();
            bool canPickGarage = BusGarageManager.Ins.HasAnyPickableBus();

            if (atMaxActive && !canPickGarage)
            {
                stallTimer += Time.deltaTime;

                if (stallTimer >= stallCycleSeconds)
                {
                    stallTimer = 0f;
                    stallCycleCount++;

                    if (stallCycleCount >= maxStallCycles)
                    {
                        TriggerLose();
                    }
                }
            }
            else
            {
                stallTimer = 0f;
            }
        }

        private void OnAllPassengersBoarded()
        {
            TriggerWin();
        }

        private void TriggerWin()
        {
            if (isGameOver)
            {
                return;
            }

            isGameOver = true;

            winEvent?.Invoke();
        }

        private void TriggerLose()
        {
            if (isGameOver)
            {
                return;
            }

            isGameOver = true;

            loseEvent?.Invoke();
        }

        #region Booster
        // Booster 1: bật mode "chọn 1 xe trong bãi đỗ để giải ngay" - UI nên show hết bãi đỗ khi mode
        // này bật (BusFlowGarageBusView tự đọc IsBooster1ModeActive trong OnMouseDown).
        public void ActivateBooster1()
        {
            IsBooster1ModeActive = true;
        }

        public void EndBooster1Mode()
        {
            IsBooster1ModeActive = false;
        }

        // Booster 2: xáo trộn toàn bộ xe trong bãi đỗ.
        public void ActivateBooster2()
        {
            BusGarageManager.Ins.ShuffleGarage();
        }

        // Booster 3: dừng toàn bộ xe đang chạy trên map, chờ click 1 xe để giải ngay xe đó.
        public void ActivateBooster3()
        {
            IsBooster3ModeActive = true;

            SetActiveBusesPaused(true);
        }

        public void ResolveBooster3Target(BaseBus target)
        {
            if (!IsBooster3ModeActive)
            {
                return;
            }

            IsBooster3ModeActive = false;

            SetActiveBusesPaused(false);

            PassengerBoardManager.Ins.BoardAllMatching(target.faction, target.remaining);

            target.ForceComplete();
        }

        private void SetActiveBusesPaused(bool paused)
        {
            for (int i = 0; i < activeBuses.Count; i++)
            {
                if (activeBuses[i] != null)
                {
                    activeBuses[i].isPaused = paused;
                }
            }
        }
        #endregion
    }
}
