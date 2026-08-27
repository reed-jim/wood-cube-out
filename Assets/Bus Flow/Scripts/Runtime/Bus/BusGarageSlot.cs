using System;
using UnityEngine;

namespace BusFlow
{
    // 1 ô trong bãi đỗ (hàng chờ) xe bus. row 0 = hàng đầu, hàng duy nhất được phép pick.
    [Serializable]
    public class BusGarageSlot
    {
        public Vector3 position;
        public int column;
        public int row;

        public BusFlowGarageBusView bus;
    }
}
