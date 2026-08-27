using System;
using System.Collections.Generic;
using System.Linq;
using AntsFlow;
using PrimeTween;
using UnityEngine;

namespace BusFlow
{
    // Tương đương ChickenManager (Chickens Flow) nhưng cho hành khách xe bus:
    // - Lưới ô dựng từ LevelItem.tiles (picture -> tile), mỗi ô đúng 1 khách (không multi-cell).
    // - "Reachable" = khách ở hàng thấp nhất còn lại trong đúng cột của nó (giống isStuck bên
    //   ChickenManager) - chỉ khách hàng đầu mới được xe đón, khách phía trên phải chờ dồn xuống.
    // - Có thêm backlog (extraPassengerRounds) để tổng số khách > lưới ban đầu hiển thị trên map,
    //   khách mới từ backlog "rơi" vào ô trống cao nhất của cột sau mỗi lần dồn xuống.
    public class PassengerBoardManager : Singleton<PassengerBoardManager>
    {
        public Material passengerFallbackMaterial;

        public Dictionary<Vector2Int, WaitingPassengerSlot> slots;
        public Dictionary<int, List<WaitingPassengerSlot>> slotsByColumn;
        public List<BusFlowPassenger> activePassengers;

        private List<Color> backlogQueue;
        private Vector2 tileSpacing;
        private int totalPassengerCount;
        private int boardedPassengerCount;
        private FactionMapper factionMapper;

        [Header("Layout")]
        public Vector3 boardOrigin = new Vector3(0f, 3.5f, 0f);

        public static event Action allPassengersBoardedEvent;
        public static event Action<BusFlowPassenger> passengerBoardedEvent;

        public bool AllBoarded => boardedPassengerCount >= totalPassengerCount;
        public int RemainingPassengerCount => totalPassengerCount - boardedPassengerCount;

        private float boardScale = 1f;

        public void StartLevel(AntsFlow.LevelItem levelItem, Vector2 spacing, int extraRounds, FactionMapper mapper, Vector3 sharedBoardOrigin, float sharedBoardScale)
        {
            tileSpacing = spacing;
            factionMapper = mapper;
            boardOrigin = sharedBoardOrigin;
            boardScale = sharedBoardScale;

            ClearExisting();

            slots = new Dictionary<Vector2Int, WaitingPassengerSlot>();
            slotsByColumn = new Dictionary<int, List<WaitingPassengerSlot>>();
            activePassengers = new List<BusFlowPassenger>();

            List<AntsFlow.BaseTile> tiles = levelItem.tiles;

            if (tiles == null || tiles.Count == 0)
            {
                return;
            }

            // PHẢI trùng khớp TUYỆT ĐỐI công thức SimulateMap (căn giữa X theo centerCol, dịch Y theo
            // minRow + boardOriginY) vì spline được vẽ quanh preview đó. Lệch 1 chút là spline ra ngoài.
            float minCol = tiles.Min(t => t.position.x);
            float maxCol = tiles.Max(t => t.position.x);
            float minRowTile = tiles.Min(t => t.position.y);
            float centerCol = (minCol + maxCol) * 0.5f;

            for (int i = 0; i < tiles.Count; i++)
            {
                AntsFlow.BaseTile tile = tiles[i];

                int column = Mathf.RoundToInt(tile.position.x);
                int row = Mathf.RoundToInt(tile.position.y);

                Vector2Int key = new Vector2Int(column, row);

                if (slots.ContainsKey(key))
                {
                    continue;
                }

                WaitingPassengerSlot slot = new WaitingPassengerSlot();
                slot.column = column;
                slot.row = row;
                // Nhân boardScale để co cả map cho vừa khung camera; spline scale cùng hệ số qua
                // busPathRoot nên vẫn dính nhau.
                slot.position = boardOrigin + boardScale * AntsFlow.LevelItem.BusTileToLocal(
                    tile.position.x, tile.position.y, centerCol, minRowTile, tileSpacing);

                slots.Add(key, slot);
                AddToColumn(column, slot);

                BusFlowPassenger passenger = SpawnPassengerAt(tile.Faction, slot);

                slot.passenger = passenger;
                activePassengers.Add(passenger);
            }

            backlogQueue = new List<Color>();

            for (int round = 0; round < extraRounds; round++)
            {
                for (int i = 0; i < tiles.Count; i++)
                {
                    backlogQueue.Add(tiles[i].Faction);
                }
            }

            ShuffleList(backlogQueue);

            totalPassengerCount = tiles.Count + backlogQueue.Count;
            boardedPassengerCount = 0;
        }

        private void ClearExisting()
        {
            if (activePassengers == null)
            {
                return;
            }

            for (int i = 0; i < activePassengers.Count; i++)
            {
                if (activePassengers[i] != null)
                {
                    Destroy(activePassengers[i].gameObject);
                }
            }
        }

        private void AddToColumn(int column, WaitingPassengerSlot slot)
        {
            if (!slotsByColumn.TryGetValue(column, out List<WaitingPassengerSlot> list))
            {
                list = new List<WaitingPassengerSlot>();
                slotsByColumn[column] = list;
            }

            list.Add(slot);
        }

        private BusFlowPassenger SpawnPassengerAt(Color faction, WaitingPassengerSlot slot)
        {
            BusFlowPassenger passenger = BusFlowPassenger.CreatePlaceholder(transform, tileSpacing * boardScale);

            Material overrideMaterial = factionMapper != null ? factionMapper.GetPassengerMaterial(faction) : null;

            if (overrideMaterial == null)
            {
                overrideMaterial = passengerFallbackMaterial;
            }

            passenger.Setup(faction, slot.column, slot.row, overrideMaterial);
            passenger.transform.position = slot.position;

            return passenger;
        }

        [Header("Pickup")]
        public float pickupRadius = 1.6f;

        // "Reachable" = khách ở hàng thấp nhất còn lại trong đúng cột (tra list, không collider).
        public bool TryPickReachablePassenger(Color faction, out BusFlowPassenger picked)
        {
            foreach (var columnEntry in slotsByColumn)
            {
                WaitingPassengerSlot frontSlot = columnEntry.Value
                    .Where(s => s.passenger != null && !s.passenger.isMoving && !s.passenger.isBoarded)
                    .OrderBy(s => s.row)
                    .FirstOrDefault();

                if (frontSlot != null && frontSlot.passenger.faction == faction)
                {
                    picked = frontSlot.passenger;

                    BoardPassenger(picked);

                    return true;
                }
            }

            picked = null;

            return false;
        }

        // Đón khách theo bán kính: quét các khách hàng đầu trong tầm của xe bus, ưu tiên khách gần xe nhất
        public bool TryPickupPassengersForBus(BaseBus bus, float customRadius, out int pickedCount)
        {
            pickedCount = 0;

            if (bus == null || bus.remaining <= 0 || slotsByColumn == null)
            {
                return false;
            }

            float effectiveRadius = customRadius > 0f ? customRadius : pickupRadius * boardScale;
            Vector2 busPos2D = new Vector2(bus.transform.position.x, bus.transform.position.y);

            List<(WaitingPassengerSlot slot, BusFlowPassenger passenger, float distance)> candidates =
                new List<(WaitingPassengerSlot, BusFlowPassenger, float)>();

            foreach (var columnEntry in slotsByColumn)
            {
                WaitingPassengerSlot frontSlot = columnEntry.Value
                    .Where(s => s.passenger != null && !s.passenger.isMoving && !s.passenger.isBoarded)
                    .OrderBy(s => s.row)
                    .FirstOrDefault();

                if (frontSlot != null && frontSlot.passenger != null && frontSlot.passenger.faction == bus.faction)
                {
                    Vector2 passengerPos2D = new Vector2(frontSlot.passenger.transform.position.x, frontSlot.passenger.transform.position.y);
                    float dist = Vector2.Distance(busPos2D, passengerPos2D);

                    if (dist <= effectiveRadius)
                    {
                        candidates.Add((frontSlot, frontSlot.passenger, dist));
                    }
                }
            }

            if (candidates.Count == 0)
            {
                return false;
            }

            candidates.Sort((a, b) => a.distance.CompareTo(b.distance));

            int toPick = Mathf.Min(candidates.Count, bus.remaining);
            pickedCount = toPick;

            for (int i = 0; i < toPick; i++)
            {
                var candidate = candidates[i];
                WaitingPassengerSlot slot = candidate.slot;
                BusFlowPassenger passenger = candidate.passenger;

                slot.passenger = null;
                passenger.isBoarded = true;
                activePassengers.Remove(passenger);

                boardedPassengerCount++;
                passengerBoardedEvent?.Invoke(passenger);

                passenger.AnimateBoarding(bus.transform, duration: 0.25f);

                MovePassengersDownInColumn(slot.column);
            }

            if (AllBoarded)
            {
                allPassengersBoardedEvent?.Invoke();
            }

            return true;
        }

        // Đếm số khách "reachable" (hàng đầu mỗi cột) hiện có theo từng màu - dùng cho
        // BusGarageManager xếp hạng độ khó khi sinh xe mới (giống GetNumNotBlockedTilePerFaction
        // bên WashingMachineManager).
        public Dictionary<Color, int> GetReachablePassengerCountsByFaction()
        {
            Dictionary<Color, int> result = new Dictionary<Color, int>();

            if (slotsByColumn == null)
            {
                return result;
            }

            foreach (var columnEntry in slotsByColumn)
            {
                WaitingPassengerSlot frontSlot = columnEntry.Value
                    .Where(s => s.passenger != null && !s.passenger.isMoving && !s.passenger.isBoarded)
                    .OrderBy(s => s.row)
                    .FirstOrDefault();

                if (frontSlot == null)
                {
                    continue;
                }

                Color faction = frontSlot.passenger.faction;

                if (!result.ContainsKey(faction))
                {
                    result[faction] = 0;
                }

                result[faction]++;
            }

            return result;
        }

        // Dùng cho booster: hút liên tục tới maxCount khách cùng màu (mỗi lần vẫn qua đúng cơ chế
        // reachable + dồn xuống, không phá vỡ tính nhất quán của lưới).
        public int BoardAllMatching(Color faction, int maxCount)
        {
            int boardedThisCall = 0;

            while (boardedThisCall < maxCount)
            {
                if (!TryPickReachablePassenger(faction, out _))
                {
                    break;
                }

                boardedThisCall++;
            }

            return boardedThisCall;
        }

        private void BoardPassenger(BusFlowPassenger passenger)
        {
            Vector2Int key = new Vector2Int(passenger.column, passenger.row);

            if (slots.TryGetValue(key, out WaitingPassengerSlot slot))
            {
                slot.passenger = null;
            }

            passenger.isBoarded = true;
            activePassengers.Remove(passenger);

            boardedPassengerCount++;

            passengerBoardedEvent?.Invoke(passenger);

            Destroy(passenger.gameObject);

            MovePassengersDownInColumn(passenger.column);

            if (AllBoarded)
            {
                allPassengersBoardedEvent?.Invoke();
            }
        }

        // Dồn khách xuống lấp ô trống thấp nhất trong một cột cụ thể, rồi bơm thêm khách mới từ backlog vào đỉnh cột.
        private void MovePassengersDownInColumn(int column)
        {
            if (!slotsByColumn.TryGetValue(column, out List<WaitingPassengerSlot> columnSlotsList))
            {
                return;
            }

            List<WaitingPassengerSlot> columnSlots = columnSlotsList.OrderBy(s => s.row).ToList();

            int writeIndex = 0;

            for (int readIndex = 0; readIndex < columnSlots.Count; readIndex++)
            {
                WaitingPassengerSlot readSlot = columnSlots[readIndex];

                if (readSlot.passenger == null || readSlot.passenger.isBoarded)
                {
                    continue;
                }

                WaitingPassengerSlot writeSlot = columnSlots[writeIndex];

                if (writeSlot != readSlot)
                {
                    BusFlowPassenger passenger = readSlot.passenger;

                    writeSlot.passenger = passenger;
                    readSlot.passenger = null;

                    passenger.column = writeSlot.column;
                    passenger.row = writeSlot.row;
                    passenger.isMoving = true;

                    Vector3 destination = writeSlot.position;

                    Tween.Position(passenger.transform, destination, duration: 0.25f)
                        .OnComplete(() =>
                        {
                            if (passenger != null)
                            {
                                passenger.isMoving = false;
                            }
                        });
                }

                writeIndex++;
            }

            RefillBacklogForColumn(column);
        }

        private void RefillBacklogForColumn(int column)
        {
            if (backlogQueue == null || backlogQueue.Count == 0)
            {
                return;
            }

            if (!slotsByColumn.TryGetValue(column, out List<WaitingPassengerSlot> columnSlotsList))
            {
                return;
            }

            WaitingPassengerSlot topEmptySlot = columnSlotsList
                .Where(s => s.passenger == null)
                .OrderByDescending(s => s.row)
                .FirstOrDefault();

            if (topEmptySlot == null)
            {
                return;
            }

            Color faction = backlogQueue[0];
            backlogQueue.RemoveAt(0);

            BusFlowPassenger passenger = SpawnPassengerAt(faction, topEmptySlot);

            topEmptySlot.passenger = passenger;
            activePassengers.Add(passenger);
        }

        // Dồn khách xuống lấp ô trống thấp nhất trong tất cả các cột
        private void MovePassengersDown()
        {
            if (slotsByColumn == null)
            {
                return;
            }

            foreach (var column in slotsByColumn.Keys)
            {
                MovePassengersDownInColumn(column);
            }
        }

        private void RefillBacklog()
        {
            if (backlogQueue == null || backlogQueue.Count == 0 || slotsByColumn == null)
            {
                return;
            }

            foreach (var column in slotsByColumn.Keys)
            {
                RefillBacklogForColumn(column);
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
