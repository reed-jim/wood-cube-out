using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the UI/Jet/EnginePlume material on a single Image.
/// - Smooths progress and the high-cycle blend (no pops on jumps)
/// - Accumulates wrapped animation phases on the CPU (double precision), so the
///   plume never freezes, never time-warps when speed changes, and never loses
///   precision after hours of idle play
/// - Feeds the Image's canvas-space rect (the Image has no UVs without a sprite)
/// Put on the Image that uses the EnginePlume material.
/// </summary>
[RequireComponent(typeof(Image))]
public class EngineJetDriver : MonoBehaviour
{
    [Header("Game inputs")]
    [Range(0f, 1f)] public float progress = 0f;      // your game progress 0..1
    [Range(0f, 1f)] public float highCycle = 0f;     // 0..1 target; 1 = progress ignored
    [Range(0f, 1f)] public float highLevel = 0.92f;  // fixed power look used in high cycle mode

    [Header("Smoothing (seconds)")]
    public float progressSmoothTime = 0.35f;
    public float highCycleSmoothTime = 0.8f;

    const double Wrap = 256.0;            // must match WRAP in the shader
    const double TwoPi = Math2PI;
    const double Math2PI = 6.283185307179586;

    static readonly int ID_Progress = Shader.PropertyToID("_Progress");
    static readonly int ID_High = Shader.PropertyToID("_HighCycle");
    static readonly int ID_HighLevel = Shader.PropertyToID("_HighLevel");
    static readonly int ID_Phases = Shader.PropertyToID("_Phases");
    static readonly int ID_Rect = Shader.PropertyToID("_Rect");

    Image _image;
    Material _mat;
    float _curProg, _progVel, _curHigh, _highVel;
    double _flow, _boil, _flick, _breath;
    readonly Vector3[] _corners = new Vector3[4];

    /// <summary>Set progress. snap = true skips smoothing.</summary>
    public void SetProgress(float p, bool snap = false)
    {
        progress = Mathf.Clamp01(p);
        if (snap) { _curProg = progress; _progVel = 0f; }
    }

    /// <summary>Set high cycle target 0..1 (the shader/driver eases toward it).</summary>
    public void SetHighCycle(float t, bool snap = false)
    {
        highCycle = Mathf.Clamp01(t);
        if (snap) { _curHigh = highCycle; _highVel = 0f; }
    }

    /// <summary>Convenience: on/off, e.g. SetHighCycleMode(cyclesPerSecond > 60).</summary>
    public void SetHighCycleMode(bool on) => SetHighCycle(on ? 1f : 0f);

    void OnEnable()
    {
        _image = GetComponent<Image>();
        _mat = new Material(_image.material);   // own instance, safe to modify
        _image.material = _mat;
        _curProg = progress;
        _curHigh = highCycle;
    }

    void OnDisable()
    {
        if (_image != null) _image.material = null;
        if (_mat != null) Destroy(_mat);
    }

    static float Smooth01(float x) { x = Mathf.Clamp01(x); return x * x * (3f - 2f * x); }
    static float SmoothStep(float a, float b, float x) { return Smooth01((x - a) / (b - a)); }

    void Update()
    {
        progress += Time.deltaTime;

        if (progress > 1)
        {
            progress = 0;
        }
    }

    void LateUpdate()
    {
        if (_mat == null) return;

        float dt = Mathf.Min(Time.unscaledDeltaTime, 0.1f);   // idle game: ignore timeScale, clamp hitches

        _curProg = Mathf.SmoothDamp(_curProg, progress, ref _progVel, progressSmoothTime, Mathf.Infinity, dt);
        _curHigh = Mathf.SmoothDamp(_curHigh, highCycle, ref _highVel, highCycleSmoothTime, Mathf.Infinity, dt);
        _curProg = Mathf.Clamp01(_curProg);
        _curHigh = Mathf.Clamp01(_curHigh);

        // same effective-progress math as the shader
        float h = Smooth01(_curHigh);
        float ep = Mathf.Clamp01(Mathf.Lerp(_curProg, highLevel, h));
        float pc = ep * (1.6f - 0.6f * ep);
        float over = SmoothStep(0.85f, 1f, ep);

        // animation speeds (phase is accumulated -> speed changes are smooth)
        double flowRate = Mathf.Lerp(0.9f, 2.4f, pc) * (1f + 0.35f * over) * Mathf.Lerp(1f, 1.25f, h);
        double boilRate = Mathf.Lerp(0.35f, 0.9f, pc);
        double flickRate = Mathf.Lerp(2.0f, 3.0f, pc);
        double breathRate = TwoPi / 10.0;      // ~10 s breathing cycle

        _flow = (_flow + dt * flowRate) % Wrap;
        _boil = (_boil + dt * boilRate) % Wrap;
        _flick = (_flick + dt * flickRate) % Wrap;
        _breath = (_breath + dt * breathRate) % TwoPi;

        _mat.SetFloat(ID_Progress, _curProg);
        _mat.SetFloat(ID_High, _curHigh);
        _mat.SetFloat(ID_HighLevel, highLevel);
        _mat.SetVector(ID_Phases, new Vector4((float)_flow, (float)_boil, (float)_flick, (float)_breath));
        _mat.SetVector(ID_Rect, GetCanvasRect());
    }

    // UI batching puts vertices in the owning canvas's local space, so give the
    // shader the Image rect in that same space.
    Vector4 GetCanvasRect()
    {
        var rt = (RectTransform)transform;
        var canvas = _image.canvas;
        rt.GetWorldCorners(_corners);
        Transform ct = canvas != null ? canvas.transform : null;

        float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
        for (int i = 0; i < 4; i++)
        {
            Vector3 p = ct != null ? ct.InverseTransformPoint(_corners[i]) : _corners[i];
            minX = Mathf.Min(minX, p.x); maxX = Mathf.Max(maxX, p.x);
            minY = Mathf.Min(minY, p.y); maxY = Mathf.Max(maxY, p.y);
        }
        return new Vector4(minX, minY, maxX, maxY);
    }
}
