using Unity.Profiling;
using UnityEditor;
using UnityEngine;

public class PlayerLoopTrackerWindow : EditorWindow
{
    private ProfilerRecorder m_PlayerLoopRecorder;
    private float m_AverageDurationMs;
    private float m_PeakDurationMs;
    private long m_SampleCount;
    
    private double m_AccumulatedTime;
    private float m_UpdateTimer;
    private const float REFRESH_INTERVAL = 0.2f; // Update UI 5 times per second to prevent jitter

    [MenuItem("Tools/Performance/Player Loop Tracker")]
    public static void ShowWindow()
    {
        GetWindow<PlayerLoopTrackerWindow>("Player Loop Tracker");
    }

    private void OnEnable()
    {
        // "CPU Total Frame Time" or the specific "PlayerLoop" marker can be tracked.
        // "PlayerLoop" measures the actual game loop execution time in ms.
        m_PlayerLoopRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "PlayerLoop", 60);
        m_PeakDurationMs = 0f;
        m_SampleCount = 0;
        m_AccumulatedTime = 0;
    }

    private void OnDisable()
    {
        // ProfilerRecorders allocate unmanaged resources and must be disposed
        m_PlayerLoopRecorder.Dispose();
    }

    private void Update()
    {
        if (!m_PlayerLoopRecorder.Valid) return;

        // Fetch the latest frame sample (converted from nanoseconds to milliseconds)
        double frameTimeMs = m_PlayerLoopRecorder.LastValue * 1e-6;

        m_AccumulatedTime += frameTimeMs;
        m_SampleCount++;

        if (frameTimeMs > m_PeakDurationMs)
        {
            m_PeakDurationMs = (float)frameTimeMs;
        }

        // Throttle UI repaints to save editor CPU overhead
        m_UpdateTimer += Time.unscaledDeltaTime;
        if (m_UpdateTimer >= REFRESH_INTERVAL)
        {
            if (m_SampleCount > 0)
            {
                m_AverageDurationMs = (float)(m_AccumulatedTime / m_SampleCount);
            }
            m_UpdateTimer = 0f;
            Repaint();
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Player Loop Performance Tracker", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Tracks the exact time spent inside Unity's PlayerLoop (in milliseconds) via ProfilerRecorder API.", MessageType.Info);

        EditorGUILayout.Space(10);

        if (!EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to begin recording Player Loop metrics.", MessageType.Warning);
            return;
        }

        // Display Metrics in a styled block
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
            if (GUILayout.Button("Reset Peak", GUILayout.Width(100), GUILayout.Height(25)))
            {
                m_PeakDurationMs = 0f;
                m_AccumulatedTime = 0;
                m_SampleCount = 0;
            }
            EditorGUILayout.Space(5);
        }
        EditorGUILayout.EndVertical();

        // Force continuous repaint while window is open in play mode
        if (EditorApplication.isPlaying)
        {
            Repaint();
        }
    }

    private Color GetPerformanceColor(float ms)
    {
        if (ms < 16.6f) return new Color(0.1f, 0.8f, 0.2f); // Good (< 60 FPS)
        if (ms < 33.3f) return new Color(0.9f, 0.6f, 0.1f); // Warning (< 30 FPS)
        return new Color(0.9f, 0.2f, 0.2f);                 // Critical (> 30 FPS drop)
    }
}