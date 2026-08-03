using System;
using UnityEngine;

namespace MyGame.Runtime
{
    public sealed class TruckColumnRuntime : MonoBehaviour
    {
        public int ColumnIndex { get; private set; }

        private int[] truckColors;
        private int[] truckCaps;
        private int frontPtr;
        private int frontReserved;
        private int frontArrived;

        public event Action<int> OnTruckFilled;
        public event Action<int> OnTruckPopped;

        public int TierCount => truckColors != null ? truckColors.Length : 0;
        public int TruckColorAt(int tier) => truckColors[tier];
        public int TruckCapAt(int tier) => truckCaps[tier];
        public int FrontTruckIndex => frontPtr;
        public int FrontArrived => frontArrived;
        public int FrontCap => frontPtr < TierCount ? truckCaps[frontPtr] : 0;

        public bool Exhausted => truckColors == null || frontPtr >= truckColors.Length;

        public int FrontColor => Exhausted ? -1 : truckColors[frontPtr];

        public bool CanAbsorb => frontPtr < TierCount && frontReserved < truckCaps[frontPtr];

        public int FrontRemaining => frontPtr < TierCount ? truckCaps[frontPtr] - frontReserved : 0;

        public void Init(int columnIndex, int[] truckColors, int[] truckCaps)
        {
            ColumnIndex = columnIndex;
            this.truckColors = truckColors ?? Array.Empty<int>();
            this.truckCaps = truckCaps ?? Array.Empty<int>();
            frontPtr = 0;
            frontReserved = 0;
            frontArrived = 0;
        }

        public bool TryReserve(out int tier)
        {
            tier = frontPtr;
            if (!CanAbsorb) return false;
            frontReserved++;
            return true;
        }

        public void Confirm(int tier)
        {
            if (frontPtr >= TierCount) return;
            frontArrived++;
            OnTruckFilled?.Invoke(frontPtr);

            if (frontArrived >= truckCaps[frontPtr])
            {
                // Advance TRƯỚC khi bắn event: listener (GameManager) đọc `Exhausted` ngay trong callback
                // để đếm cột đã cạn. Nếu bắn trước khi frontPtr++ thì lúc pop truck CUỐI, Exhausted vẫn
                // false ⇒ columnsExhausted không bao giờ đủ ⇒ không bao giờ thắng.
                int poppedTier = frontPtr;
                frontPtr++;
                frontReserved = 0;
                frontArrived = 0;
                OnTruckPopped?.Invoke(poppedTier);
            }
        }
    }
}
