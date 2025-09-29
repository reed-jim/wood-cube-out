using UnityEngine;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace NamPhuThuy.Events
{
    [CreateAssetMenu(fileName = "SwitchRouteChannel", menuName = "Events/Switch Route Channel")]

    public class SwitchRouteChannelSO : ScriptableObject
    {
        [SerializeField, TextArea]
        private string description = "Handles screen route switching events (ReturnHome, Replay, IAPReview, etc.)";

        private event Action<GameEnum.ScreenRoute> _event;

        public event Action<GameEnum.ScreenRoute> Event
        {
            add
            {
                _event += value;

#if UNITY_EDITOR
                string subscriberName = value.Target?.GetType().Name ?? "Unknown";
                EventFlowDebugger.LogEventSubscribe(name, subscriberName);
#endif
            }
            remove
            {
                _event -= value;

#if UNITY_EDITOR
                string subscriberName = value.Target?.GetType().Name ?? "Unknown";
                EventFlowDebugger.LogEventUnsubscribe(name, subscriberName);
#endif
            }
        }

        public void Invoke(GameEnum.ScreenRoute route, string senderName = null)
        {
#if UNITY_EDITOR
            if (senderName == null)
            {
                var stackTrace = new StackTrace(1, true);
                var frame = stackTrace.GetFrame(0);
                senderName = frame?.GetMethod()?.DeclaringType?.Name ?? "Unknown";
            }

            EventFlowDebugger.LogEventInvoke(name, senderName, route.ToString());
#endif

            _event?.Invoke(route);
        }

#if UNITY_EDITOR
        [ContextMenu("Debug Current Subscribers")]
        private void DebugSubscribers()
        {
            if (_event != null)
            {
                Debug.Log($"=== {name} Subscribers ===");
                var invocationList = _event.GetInvocationList();
                for (int i = 0; i < invocationList.Length; i++)
                {
                    var subscriber = invocationList[i];
                    Debug.Log($"- {subscriber.Target?.GetType().Name}.{subscriber.Method.Name}");
                }
            }
            else
            {
                Debug.Log($"{name} has no subscribers");
            }
        }
#endif
    }
}