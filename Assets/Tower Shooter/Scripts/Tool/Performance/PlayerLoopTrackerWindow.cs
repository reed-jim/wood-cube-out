using System.Collections.Generic;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

public class PlayerLoopTrackerWindow : EditorWindow
{
    private struct SubsystemTracker
    {
        public string Name;
        public ProfilerRecorder Recorder;
    }

    // Main Frame Recorder
    private ProfilerRecorder m_PlayerLoopRecorder;
    
    // Sub-system Trackers
    private List<SubsystemTracker> m_SubsystemTrackers = new List<SubsystemTracker>();

    // Metrics
    private float m_AverageDurationMs;
    private float m_PeakDurationMs;
    private long m_SampleCount;
    private double m_AccumulatedTime;
    
    // UI Refresh & Graph History
    private float m_UpdateTimer;
    private const float REFRESH_INTERVAL = 0.5f;
    private readonly float[] m_HistoryBuffer = new float[100];
    private int m_HistoryIndex;

    // Common PlayerLoop root categories to monitor
    private readonly string[] m_TargetSubsystems = new string[]
    {
        "Update.ScriptRunBehaviourUpdate",
        "RenderLoop.Draw",
        "Physics.Simulate",
        "Physics2D.Simulate",
        "WaitForTargetFPS",
        "GC.Collect",
        "EarlyUpdate.PollPlayerConnection",
        "Initialization.DirectorUpdate"
    };

    [MenuItem("Tools/Performance/Player Loop Tracker")]
    public static void ShowWindow()
    {
        GetWindow<PlayerLoopTrackerWindow>("Player Loop Tracker");
    }

    private void OnEnable()
    {
        m_PlayerLoopRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "PlayerLoop", 60);
        
        // Initialize sub-system trackers safely
        m_SubsystemTrackers.Clear();
        foreach (var markerName in m_TargetSubsystems)
        {
            try
            {
                var recorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, markerName, 30);
                if (recorder.Valid)
                {
                    m_SubsystemTrackers.Add(new SubsystemTracker
                    {
                        Name = markerName,
                        Recorder = recorder
                    });
                }
            }
            catch
            {
                // Marker might not exist in all Unity versions/platforms
            }
        }

        m_PeakDurationMs = 0f;
        m_SampleCount = 0;
        m_AccumulatedTime = 0;
        m_HistoryIndex = 0;
        for (int i = 0; i < m_HistoryBuffer.Length; i++) m_HistoryBuffer[i] = 0f;
    }

    private void OnDisable()
    {
        if (m_PlayerLoopRecorder.Valid) m_PlayerLoopRecorder.Dispose();
        
        foreach (var tracker in m_SubsystemTrackers)
        {
            if (tracker.Recorder.Valid) tracker.Recorder.Dispose();
        }
        m_SubsystemTrackers.Clear();
    }

    private void Update()
    {
        if (!m_PlayerLoopRecorder.Valid) return;

        double frameTimeMs = m_PlayerLoopRecorder.LastValue * 1e-6;

        m_AccumulatedTime += frameTimeMs;
        m_SampleCount++;

        if (frameTimeMs > m_PeakDurationMs)
        {
            m_PeakDurationMs = (float)frameTimeMs;
        }

        m_UpdateTimer += Time.unscaledDeltaTime;
        if (m_UpdateTimer >= REFRESH_INTERVAL)
        {
            if (m_SampleCount > 0)
            {
                m_AverageDurationMs = (float)(m_AccumulatedTime / m_SampleCount);
            }

            // Push to history buffer for graph
            m_HistoryBuffer[m_HistoryIndex] = (float)frameTimeMs;
            m_HistoryIndex = (m_HistoryIndex + 1) % m_HistoryBuffer.Length;

            m_UpdateTimer = 0f;
            Repaint();
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Player Loop Performance Tracker", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Tracks PlayerLoop frame timing, live history graph, and identifies top 3 expensive sub-systems.", MessageType.Info);

        EditorGUILayout.Space(5);

        if (!EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to begin recording Player Loop metrics.", MessageType.Warning);
            return;
        }

        // 1. Live Metrics Box
        GUIStyle metricStyle = new GUIStyle(EditorStyles.largeLabel)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold
        };
        metricStyle.normal.textColor = GetPerformanceColor(m_AverageDurationMs);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Average Player Loop:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"{m_AverageDurationMs:F2} ms", metricStyle);
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField($"Peak Spikes: {m_PeakDurationMs:F2} ms", EditorStyles.boldLabel);
            
            EditorGUILayout.Space(5);
            if (GUILayout.Button("Reset Peak & Stats", GUILayout.Width(130), GUILayout.Height(25)))
            {
                m_PeakDurationMs = 0f;
                m_AccumulatedTime = 0;
                m_SampleCount = 0;
            }
            EditorGUILayout.Space(5);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        // 2. Rolling History Graph
        EditorGUILayout.LabelField("Frame Time History (Last 100 Ticks)", EditorStyles.boldLabel);
        Rect graphRect = GUILayoutUtility.GetRect(0, 80, GUILayout.ExpandWidth(true));
        DrawHistoryGraph(graphRect);

        EditorGUILayout.Space(10);

        // 3. Top 3 Most Expensive Sub-systems
        EditorGUILayout.LabelField("Top 3 Expensive Sub-systems", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.Space(3);
            DrawTopSubsystems();
            EditorGUILayout.Space(3);
        }
        EditorGUILayout.EndVertical();

        if (EditorApplication.isPlaying)
        {
            Repaint();
        }
    }

    private void DrawHistoryGraph(Rect rect)
    {
        EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.15f, 1f));

        float maxScaleMs = 33.3f; // 30 FPS ceiling for graph scaling
        float targetY = rect.y + rect.height - (16.6f / maxScaleMs) * rect.height;
        Handles.color = new Color(0.2f, 0.7f, 0.3f, 0.5f);
        Handles.DrawLine(new Vector3(rect.x, targetY), new Vector3(rect.x + rect.width, targetY));

        Handles.color = new Color(0.2f, 0.6f, 1f, 1f);
        Vector3[] points = new Vector3[m_HistoryBuffer.Length];
        float stepX = rect.width / (m_HistoryBuffer.Length - 1);

        for (int i = 0; i < m_HistoryBuffer.Length; i++)
        {
            int idx = (m_HistoryIndex + i) % m_HistoryBuffer.Length;
            float val = Mathf.Clamp(m_HistoryBuffer[idx], 0f, maxScaleMs);
            float x = rect.x + (i * stepX);
            float y = rect.y + rect.height - (val / maxScaleMs) * rect.height;
            points[i] = new Vector3(x, y, 0);
        }
        Handles.DrawAAPolyLine(2f, points);
    }

    private void DrawTopSubsystems()
    {
        if (m_SubsystemTrackers.Count == 0)
        {
            EditorGUILayout.LabelField("Collecting subsystem samples...", EditorStyles.miniLabel);
            return;
        }

        // Gather valid samples and values (converted to ms)
        List<(string name, double ms)> measuredList = new List<(string, double)>();
        foreach (var tracker in m_SubsystemTrackers)
        {
            if (tracker.Recorder.Valid)
            {
                double ms = tracker.Recorder.LastValue * 1e-6;
                if (ms > 0)
                {
                    measuredList.Add((tracker.Name, ms));
                }
            }
        }

        if (measuredList.Count == 0)
        {
            EditorGUILayout.LabelField("Waiting for frame data...", EditorStyles.miniLabel);
            return;
        }

        // Sort descending by execution time (ms)
        measuredList.Sort((a, b) => b.ms.CompareTo(a.ms));

        int count = Mathf.Min(3, measuredList.Count);
        for (int i = 0; i < count; i++)
        {
            var item = measuredList[i];
            string displayName = item.name;
            if (displayName.Contains("."))
            {
                var parts = displayName.Split('.');
                displayName = parts[parts.Length - 1];
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{i + 1}. {displayName}", GUILayout.Width(220));
            EditorGUILayout.LabelField($"{item.ms:F2} ms", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
        }
    }

    private Color GetPerformanceColor(float ms)
    {
        if (ms < 16.6f) return new Color(0.1f, 0.8f, 0.2f);
        if (ms < 33.3f) return new Color(0.9f, 0.6f, 0.1f);
        return new Color(0.9f, 0.2f, 0.2f);
    }
}