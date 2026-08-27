using System.Collections.Generic;
using System.Linq;
using AntsFlow;
using PrimeTween;
using UnityEngine;

namespace BusFlow
{
    // Tương đương WashingMachineManager (Ants Flow) nhưng cho bãi đỗ xe bus:
    // - Lưới busGarageNumColumns x busGarageNumRows dựng SẴN và ĐIỀN ĐẦY ngay từ đầu; các ô
    //   đầu được gán xe ngay lúc StartLevel - phần còn lại KHÔNG sinh trước.
    // - Khi 1 cột bị rút cạn hoàn toàn (pick + đẩy hết xe cũ), mới sinh xe MỚI lấp lại cả cột đó,
    //   mỗi xe mới chọn màu ngay lúc sinh theo GetNextBusFactionByDifficulty (xem bên dưới) - giống
    //   hệt cách SpawnNewWashingMachine/GetNextAntSpawnByDifficulty hoạt động.
    // - Độ khó: màu được xếp hạng theo số khách "reachable" (hàng đầu mỗi cột) hiện có nhiều nhất.
    //   difficultyLevel = 0 luôn chọn đúng màu đang dư dả nhất (dễ, giúp giải nhanh); difficultyLevel
    //   cao hơn cố tình bỏ qua màu đó, chọn xuống màu hiếm hơn trong bảng xếp hạng (khó hơn).
    public class BusGarageManager : Singleton<BusGarageManager>
    {
        [Tooltip("Prefab hình cho xe bus trong bãi đỗ. Kéo Assets/Ants Flow/Prefabs/Ant Box.prefab vào " +
                 "để dùng lại hình washing machine. Để trống thì fallback dùng primitive cube.")]
        public GameObject busPrefab;
        public Material busFallbackMaterial;
        [Tooltip("Khoảng cách giữa các ô bãi đỗ trên mặt phẳng 2D: x = giữa cột, y = giữa hàng (Ants Flow dùng ~4.58x và ~4.18y).")]
        public Vector2 slotSpacing = new Vector2(4.6f, 4.2f);
        public Vector3 garageOrigin = new Vector3(0f, -6.5f, 0f);

        [Tooltip("Scale hiển thị của prefab Ant Box (Ants Flow set 2.3 trong Init). Chỉ áp dụng khi dùng busPrefab; primitive giữ scale riêng. Chỉnh cùng slotSpacing để xe không đè nhau.")]
        public float busVisualScale = 2.2f;
        [Tooltip("Bật animation phóng to (scale-in) khi xe xuất hiện, giống Init bên Ants Flow.")]
        public bool animateSpawn = true;
        [Tooltip("Độ trễ so le giữa các xe trong lượt spawn đầu (giây) để xuất hiện lần lượt thay vì cùng lúc.")]
        public float spawnStagger = 0.05f;

        [Tooltip("Sức chứa tối thiểu/tối đa của 1 xe khi được sinh ra, giới hạn bởi phần pool còn lại của faction đó.")]
        public int minBusCapacity = 5;
        public int maxBusCapacity = 15;

        public List<BusGarageSlot> slots;
        private Dictionary<int, List<BusGarageSlot>> slotsByColumn;
        private Dictionary<Color, int> busPoolRemainingByFaction;
        private FactionMapper factionMapper;

        private Vector3 fitGarageOrigin;
        private float fitGarageScale = 1f;

        public void StartLevel(AntsFlow.LevelItem levelItem, FactionMapper mapper, Vector3 computedGarageOrigin, float computedGarageScale)
        {
            factionMapper = mapper;
            fitGarageOrigin = computedGarageOrigin;
            fitGarageScale = computedGarageScale;

            ClearExisting();

            int numColumn = Mathf.Max(1, levelItem.busGarageNumColumns);
            int numRow = Mathf.Max(1, levelItem.busGarageNumRows);

            slots = new List<BusGarageSlot>();
            slotsByColumn = new Dictionary<int, List<BusGarageSlot>>();

            busPoolRemainingByFaction = new Dictionary<Color, int>();

            if (levelItem.busGarageQueue != null)
            {
                for (int i = 0; i < levelItem.busGarageQueue.Count; i++)
                {
                    BusGarageItemConfig item = levelItem.busGarageQueue[i];

                    busPoolRemainingByFaction[item.faction] = item.number;
                }
            }

            float centerCol = (numColumn - 1) * 0.5f;

            // Dựng lưới toàn ô trống trước: Row 0 ở trên (gần đường chạy), các row sau xếp dần xuống dưới.
            for (int row = 0; row < numRow; row++)
            {
                for (int column = 0; column < numColumn; column++)
                {
                    BusGarageSlot slot = new BusGarageSlot();
                    slot.column = column;
                    slot.row = row;
                    // Dùng gốc + scale do BusFlowGameManager tính (đặt bãi đỗ ở đáy khung, co vừa bề rộng).
                    slot.position = fitGarageOrigin + fitGarageScale * new Vector3(
                        (column - centerCol) * slotSpacing.x,
                        -row * slotSpacing.y,
                        0f);

                    slots.Add(slot);
                    AddToColumn(column, slot);
                }
            }

            // Điền ĐẦY grid ngay từ đầu (numColumn * numRow ô). Mỗi lần pick 1 xe hàng đầu thì cột đó
            // đẩy lên + sinh 1 xe mới ở cuối cột, nên cột luôn đầy bằng các cột khác (nếu pool còn xe).
            List<BusGarageSlot> fillOrder = slots.OrderBy(s => s.row).ThenBy(s => s.column).ToList();

            for (int i = 0; i < fillOrder.Count; i++)
            {
                // Lượt spawn đầu: so le index để xe pop-in lần lượt thay vì hiện cùng lúc.
                TrySpawnNewBusIntoSlot(fillOrder[i], staggerIndex: i);
            }
        }

        private void ClearExisting()
        {
            if (slots == null)
            {
                return;
            }

            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].bus != null)
                {
                    Destroy(slots[i].bus.gameObject);
                }
            }
        }

        private void AddToColumn(int column, BusGarageSlot slot)
        {
            if (!slotsByColumn.TryGetValue(column, out List<BusGarageSlot> list))
            {
                list = new List<BusGarageSlot>();
                slotsByColumn[column] = list;
            }

            list.Add(slot);
        }

        // Xếp hạng màu theo số khách reachable hiện có (nhiều nhất trước), chỉ tính màu còn pool.
        // difficultyLevel là index vào bảng này: 0 = màu dư dả nhất, cao hơn = cố tình bỏ qua, chọn
        // xuống màu hiếm hơn.
        private bool GetNextBusFactionByDifficulty(int difficultyLevel, out Color faction, out int capacity)
        {
            Dictionary<Color, int> reachableCounts = PassengerBoardManager.Ins != null
                ? PassengerBoardManager.Ins.GetReachablePassengerCountsByFaction()
                : new Dictionary<Color, int>();

            List<Color> ranked = busPoolRemainingByFaction
                .Where(pair => pair.Value > 0)
                .OrderByDescending(pair => reachableCounts.TryGetValue(pair.Key, out int count) ? count : 0)
                .Select(pair => pair.Key)
                .ToList();

            if (ranked.Count == 0)
            {
                faction = default;
                capacity = 0;

                return false;
            }

            int clampedDifficulty = Mathf.Clamp(difficultyLevel, 0, ranked.Count - 1);

            faction = ranked[clampedDifficulty];

            int remaining = busPoolRemainingByFaction[faction];

            capacity = Mathf.Min(remaining, Random.Range(minBusCapacity, maxBusCapacity + 1));
            capacity = Mathf.Max(1, capacity);

            busPoolRemainingByFaction[faction] = remaining - capacity;

            return true;
        }

        private bool TrySpawnNewBusIntoSlot(BusGarageSlot slot, int staggerIndex = -1)
        {
            int difficultyLevel = BusFlowGameManager.Ins != null ? BusFlowGameManager.Ins.busDifficultyLevel : 0;

            if (!GetNextBusFactionByDifficulty(difficultyLevel, out Color faction, out int capacity))
            {
                return false;
            }

            slot.bus = SpawnBusAt(faction, capacity, slot, staggerIndex);

            return true;
        }

        private BusFlowGarageBusView SpawnBusAt(Color faction, int number, BusGarageSlot slot, int staggerIndex = -1)
        {
            bool usingPrefab = busPrefab != null;

            BusFlowGarageBusView view = usingPrefab
                ? BusFlowGarageBusView.CreateFromPrefab(busPrefab, transform)
                : BusFlowGarageBusView.CreatePlaceholder(transform);

            view.Setup(faction, number, slot.column, slot.row, ResolveBusMaterial(faction));
            view.transform.position = slot.position;

            // Chỉ ép scale hiển thị khi dùng prefab (primitive tự giữ scale trong CreatePlaceholder).
            // Nhân fitGarageScale để xe co cùng lưới bãi đỗ khi auto-fit.
            float targetScale = usingPrefab ? busVisualScale * fitGarageScale : view.transform.localScale.x * fitGarageScale;

            view.transform.localScale = targetScale * Vector3.one;

            if (animateSpawn)
            {
                // Scale-in giống Init bên Ants Flow, delay so le nếu là lượt spawn đầu.
                float delay = staggerIndex >= 0 ? staggerIndex * spawnStagger : 0f;

                view.transform.localScale = Vector3.zero;

                Tween.Scale(view.transform, targetScale, duration: 0.25f, startDelay: delay, ease: Ease.OutBack);
            }

            return view;
        }

        private Material ResolveBusMaterial(Color faction)
        {
            Material overrideMaterial = factionMapper != null ? factionMapper.GetBusMaterial(faction) : null;

            return overrideMaterial != null ? overrideMaterial : busFallbackMaterial;
        }

        public bool HasAnyPickableBus()
        {
            if (slots == null)
            {
                return false;
            }

            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].row == 0 && slots[i].bus != null)
                {
                    return true;
                }
            }

            return false;
        }

        public void TryPickBus(int column)
        {
            if (slotsByColumn == null || !slotsByColumn.TryGetValue(column, out List<BusGarageSlot> columnSlots))
            {
                return;
            }

            BusGarageSlot frontSlot = columnSlots.FirstOrDefault(s => s.row == 0);

            if (frontSlot == null || frontSlot.bus == null)
            {
                return;
            }

            if (!BusFlowGameManager.Ins.CanActivateMoreBuses())
            {
                return;
            }

            BusFlowGarageBusView view = frontSlot.bus;

            frontSlot.bus = null;

            // Truyền vị trí xe trong bãi để xe nhập làn tại điểm loop gần nó nhất.
            BusFlowGameManager.Ins.SpawnBusOnMap(view.faction, view.number, view.transform.position);

            Destroy(view.gameObject);

            ShiftColumnForward(columnSlots);
            SpawnBusAtColumnBottom(columnSlots);
        }

        public float FitGarageScale => fitGarageScale;

        // "Đẩy từ dưới lên": xe hàng sau (row lớn hơn) dịch lên chiếm row vừa trống, có tween trượt mượt mà.
        private void ShiftColumnForward(List<BusGarageSlot> columnSlots)
        {
            List<BusGarageSlot> ordered = columnSlots.OrderBy(s => s.row).ToList();

            for (int i = 0; i < ordered.Count - 1; i++)
            {
                ordered[i].bus = ordered[i + 1].bus;

                if (ordered[i].bus != null)
                {
                    ordered[i].bus.row = ordered[i].row;
                    Vector3 targetPos = ordered[i].position;
                    Tween.Position(ordered[i].bus.transform, targetPos, duration: 0.25f, ease: Ease.OutQuad);
                }
            }

            ordered[ordered.Count - 1].bus = null;
        }

        // Mỗi lần pick 1 xe hàng đầu: sau khi đẩy cột lên (ShiftColumnForward), sinh 1 xe MỚI ở ô cuối
        // cột (row lớn nhất = đáy cột) để cột luôn đầy bằng các cột khác. Nếu pool hết xe thì để trống.
        private void SpawnBusAtColumnBottom(List<BusGarageSlot> columnSlots)
        {
            BusGarageSlot bottomSlot = columnSlots.OrderByDescending(s => s.row).FirstOrDefault();

            if (bottomSlot != null && bottomSlot.bus == null)
            {
                TrySpawnNewBusIntoSlot(bottomSlot);
            }
        }

        // Booster 1: giải ngay 1 xe trong bãi đỗ (không cần chạy lên map), hút hết khách cùng màu.
        public void SolveGarageBus(BusFlowGarageBusView view)
        {
            BusFlowGameManager.Ins.EndBooster1Mode();

            BusGarageSlot slot = slots.FirstOrDefault(s => s.bus == view);

            if (slot == null)
            {
                return;
            }

            PassengerBoardManager.Ins.BoardAllMatching(view.faction, view.number);

            slot.bus = null;

            Destroy(view.gameObject);

            if (slotsByColumn.TryGetValue(slot.column, out List<BusGarageSlot> columnSlots))
            {
                ShiftColumnForward(columnSlots);
                SpawnBusAtColumnBottom(columnSlots);
            }
        }

        // Booster 2: xáo trộn vị trí các xe ĐANG có trong bãi đỗ (không đụng tới pool/màu chưa sinh).
        public void ShuffleGarage()
        {
            if (slots == null)
            {
                return;
            }

            List<BusGarageSlot> filledSlots = slots.Where(s => s.bus != null).ToList();

            List<(Color faction, int number)> data = filledSlots
                .Select(s => (s.bus.faction, s.bus.number))
                .ToList();

            ShuffleList(data);

            for (int i = 0; i < filledSlots.Count; i++)
            {
                BusGarageSlot slot = filledSlots[i];
                (Color faction, int number) = data[i];

                slot.bus.Setup(faction, number, slot.column, slot.row, ResolveBusMaterial(faction));
            }
        }

        private static void ShuffleList<T>(List<T> list)
        {
            System.Random random = new System.Random();

            for (int i = list.Count - 1; i > 0; i--)
            {
                int swapIndex = random.Next(i + 1);

                (list[i], list[swapIndex]) = (list[swapIndex], list[i]);
            }
        }
    }
}
