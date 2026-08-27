using System;
using UnityEngine;

namespace BusFlow
{
    // 1 ô trong lưới hành khách. Giữ ref hành khách đang đứng ở đó (null = trống).
    [Serializable]
    public class WaitingPassengerSlot
    {
        public Vector3 position;
        public int column;
        public int row;

        public BusFlowPassenger passenger;
    }
}
