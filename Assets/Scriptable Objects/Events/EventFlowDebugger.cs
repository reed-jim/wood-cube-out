using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace NamPhuThuy.Events
{
    public class EventFlowDebugger : MonoBehaviour
    {
        [System.Serializable]
        public class EventFlowEntry
        {
            public string eventName;
            public string sender;
            public string receiver;
            public float timestamp;
            public Color color = Color.white;
            public EventType type;
            public string details;

            public enum EventType
            {
                Invoke,
                Subscribe,
                Unsubscribe
            }
        }

        [Header("Visual Settings")]
        [SerializeField] private bool enableVisualDebug = true;
        [SerializeField] private bool showInGameView = true;
        [SerializeField] private int maxVisibleEntries = 15;
        [SerializeField] private int maxStoredEntries = 100;
        [SerializeField] private float entryLifetime = 10f;

        [Header("Filter Settings")]
        [SerializeField] private List<string> eventNameFilters = new List<string>();
        [SerializeField] private bool useWhitelist = false;

        [Header("GUI Settings")]
        [SerializeField] private Rect debugWindowRect = new Rect(10, 10, 400, 300);

        [SerializeField] public List<EventFlowEntry> eventFlow = new List<EventFlowEntry>();
        private Vector2 scrollPosition;
        private Dictionary<string, Color> eventColors = new Dictionary<string, Color>();
        private int colorIndex = 0;

        private readonly Color[] colors = new Color[]
        {
            Color.cyan, Color.yellow, Color.magenta, Color.green,
            Color.red, Color.blue, new Color(1f, 0.5f, 0f),
            new Color(0.5f, 0f, 1f), new Color(0f, 1f, 0.5f), new Color(1f, 0f, 0.5f)
        };

        public static EventFlowDebugger Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            if (eventFlow.Count > 0)
            {
                float currentTime = Time.time;
                eventFlow.RemoveAll(entry => currentTime - entry.timestamp > entryLifetime);
            }
        }

        public static void LogEventInvoke(string eventName, string sender, string details = "")
        {
            if (Instance == null || !Instance.enableVisualDebug) return;
            Instance.AddEventEntry(eventName, sender, "", EventFlowEntry.EventType.Invoke, details);
        }

        public static void LogEventReceive(string eventName, string sender, string receiver, string details = "")
        {
            if (Instance == null || !Instance.enableVisualDebug) return;
            Instance.AddEventEntry(eventName, sender, receiver, EventFlowEntry.EventType.Invoke, details);
        }

        public static void LogEventSubscribe(string eventName, string subscriber)
        {
            if (Instance == null || !Instance.enableVisualDebug) return;
            Instance.AddEventEntry(eventName, "", subscriber, EventFlowEntry.EventType.Subscribe, "");
        }

        public static void LogEventUnsubscribe(string eventName, string subscriber)
        {
            if (Instance == null || !Instance.enableVisualDebug) return;
            Instance.AddEventEntry(eventName, "", subscriber, EventFlowEntry.EventType.Unsubscribe, "");
        }

        private void AddEventEntry(string eventName, string sender, string receiver, EventFlowEntry.EventType type, string details)
        {
            if (eventNameFilters.Count > 0)
            {
                bool isFiltered = eventNameFilters.Any(filter => eventName.Contains(filter));
                if ((useWhitelist && !isFiltered) || (!useWhitelist && isFiltered))
                {
                    return;
                }
            }

            var entry = new EventFlowEntry
            {
                eventName = eventName,
                sender = sender,
                receiver = receiver,
                timestamp = Time.time,
                color = GetEventColor(eventName),
                type = type,
                details = details
            };

            eventFlow.Add(entry);

            if (eventFlow.Count > maxStoredEntries)
            {
                eventFlow.RemoveAt(0);
            }

            string logMessage = $"[EVENT {type.ToString().ToUpper()}] {eventName}";
            if (!string.IsNullOrEmpty(sender)) logMessage += $" from {sender}";
            if (!string.IsNullOrEmpty(receiver)) logMessage += $" -> {receiver}";
            if (!string.IsNullOrEmpty(details)) logMessage += $" ({details})";

            Debug.Log(logMessage);
        }

        private Color GetEventColor(string eventName)
        {
            if (!eventColors.ContainsKey(eventName))
            {
                eventColors[eventName] = colors[colorIndex % colors.Length];
                colorIndex++;
            }
            return eventColors[eventName];
        }

        private void OnGUI()
        {
            if (!enableVisualDebug || !showInGameView) return;

            debugWindowRect = GUILayout.Window(0, debugWindowRect, DrawDebugWindow, "Event Flow Debugger");
        }

        private void DrawDebugWindow(int windowID)
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear", GUILayout.Width(50)))
            {
                eventFlow.Clear();
            }
            if (GUILayout.Button(enableVisualDebug ? "Pause" : "Resume", GUILayout.Width(60)))
            {
                enableVisualDebug = !enableVisualDebug;
            }
            GUILayout.Label($"Entries: {eventFlow.Count}", GUILayout.Width(80));
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(250));

            var recentEntries = eventFlow.TakeLast(maxVisibleEntries).ToList();

            foreach (var entry in recentEntries)
            {
                DrawEventEntry(entry);
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        private void DrawEventEntry(EventFlowEntry entry)
        {
            Color originalColor = GUI.color;

            GUILayout.BeginVertical(GUI.skin.box);

            GUI.color = entry.color;
            GUILayout.BeginHorizontal();

            string typeIcon = entry.type switch
            {
                EventFlowEntry.EventType.Invoke => "▶",
                EventFlowEntry.EventType.Subscribe => "📥",
                EventFlowEntry.EventType.Unsubscribe => "📤",
                _ => "?"
            };

            GUILayout.Label($"{typeIcon} {entry.timestamp:F1}s", GUILayout.Width(60));
            GUILayout.Label(entry.eventName);
            GUILayout.EndHorizontal();

            GUI.color = originalColor;

            if (!string.IsNullOrEmpty(entry.sender))
            {
                GUILayout.Label($"  From: {entry.sender}");
            }

            if (!string.IsNullOrEmpty(entry.receiver))
            {
                GUILayout.Label($"  To: {entry.receiver}");
            }

            if (!string.IsNullOrEmpty(entry.details))
            {
                GUILayout.Label($"  Details: {entry.details}");
            }

            GUILayout.EndVertical();
        }

        [ContextMenu("Clear Event Log")]
        public void ClearLog()
        {
            eventFlow.Clear();
        }

        [ContextMenu("Toggle Debug")]
        public void ToggleDebug()
        {
            enableVisualDebug = !enableVisualDebug;
        }
    }
}