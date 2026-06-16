#if UNITY_EDITOR
using System;

namespace SearchEngine.Additions
{
    public static class EventSender
    {
        public static void SendEvent(Action action)
        {
            var handler = action;
            if (handler != null)
                handler();
        }

        public static void SendEvent<T1>(Action<T1> action, T1 p1)
        {
            var handler = action;
            if (handler != null)
                handler(p1);
        }

        public static void SendEvent<T1, T2>(Action<T1, T2> action, T1 p1, T2 p2)
        {
            var handler = action;
            if (handler != null)
                handler(p1, p2);
        }

        public static void SendEvent<T1, T2, T3>(Action<T1, T2, T3> action, T1 p1, T2 p2, T3 p3)
        {
            var handler = action;
            if (handler != null)
                handler(p1, p2, p3);
        }

        public static void SendEvent<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 p1, T2 p2, T3 p3, T4 p4)
        {
            var handler = action;
            if (handler != null)
                handler(p1, p2, p3, p4);
        }
    }
}

#endif