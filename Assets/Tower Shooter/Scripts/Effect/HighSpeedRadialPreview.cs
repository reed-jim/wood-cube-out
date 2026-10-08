using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Graphic))]
public class HighSpeedRadialPreview : MonoBehaviour
{
    [Header("Shader Properties")]
    [Range(0f, 1f)] public float fillAmount = 1.0f;
    public bool fillClockwise = true;
    [Range(0f, 360f)] public float fillRotation = 0.0f;

    [Space]
    [Range(0f, 1f)] public float highSpeed = 1.0f;
    [ColorUsage(false, true)] public Color streakColor = new Color(2f, 2f, 2f, 1f);
    [Range(1f, 64f)] public float streakCount = 12f;
    [Range(0.01f, 1.5f)] public float streakLength = 0.35f;
    [Range(0.001f, 1f)] public float streakSoftness = 0.15f;
    public float rotationOffset = 0.0f;
    [Range(0f, 3f)] public float intensity = 1.0f;

    [Header("Runtime Animation Preview")]
    public bool animateRotation = true;
    public float rotationSpeed = 180.0f;

    [Space]
    public bool animateFill = false;
    public float fillSpeed = 0.5f;

    [Space]
    public bool pulseIntensity = false;
    public float pulseSpeed = 4.0f;

    private Graphic _graphic;
    private Material _materialInstance;

    // Property Shader IDs
    private static readonly int FillAmountID = Shader.PropertyToID("_FillAmount");
    private static readonly int FillClockwiseID = Shader.PropertyToID("_FillClockwise");
    private static readonly int FillRotationID = Shader.PropertyToID("_FillRotation");
    private static readonly int HighSpeedID = Shader.PropertyToID("_HighSpeed");
    private static readonly int StreakColorID = Shader.PropertyToID("_StreakColor");
    private static readonly int StreakCountID = Shader.PropertyToID("_StreakCount");
    private static readonly int StreakLengthID = Shader.PropertyToID("_StreakLength");
    private static readonly int StreakSoftnessID = Shader.PropertyToID("_StreakSoftness");
    private static readonly int RotationID = Shader.PropertyToID("_Rotation");
    private static readonly int IntensityID = Shader.PropertyToID("_Intensity");

    private void OnEnable()
    {
        _graphic = GetComponent<Graphic>();
        UpdateMaterialInstance();
    }

    private void Update()
    {
        if (_graphic == null) return;

        // Ensure we operate on a unique material instance so we don't modify project assets
        if (_materialInstance == null || _graphic.material != _materialInstance)
        {
            UpdateMaterialInstance();
        }

        HandleAnimations();
        ApplyProperties();
    }

    private void UpdateMaterialInstance()
    {
        if (_graphic.material != null && _graphic.material.shader.name == "UI/HighSpeedRadial")
        {
            _materialInstance = new Material(_graphic.material);
            _graphic.material = _materialInstance;
        }
    }

    private void HandleAnimations()
    {
        float deltaTime = Application.isPlaying ? Time.deltaTime : 0.016f; // Rough editor delta time fallback

        if (animateRotation)
        {
            rotationOffset += rotationSpeed * deltaTime;
            rotationOffset %= 360f;
        }

        if (animateFill)
        {
            fillAmount = Mathf.PingPong(Time.time * fillSpeed, 1.0f);
        }

        if (pulseIntensity)
        {
            intensity = 1.0f + Mathf.Sin(Time.time * pulseSpeed) * 0.5f;
        }
    }

    private void ApplyProperties()
    {
        if (_materialInstance == null) return;

        _materialInstance.SetFloat(FillAmountID, fillAmount);
        _materialInstance.SetFloat(FillClockwiseID, fillClockwise ? 1.0f : 0.0f);
        _materialInstance.SetFloat(FillRotationID, fillRotation);

        _materialInstance.SetFloat(HighSpeedID, highSpeed);
        _materialInstance.SetColor(StreakColorID, streakColor);
        _materialInstance.SetFloat(StreakCountID, streakCount);
        _materialInstance.SetFloat(StreakLengthID, streakLength);
        _materialInstance.SetFloat(StreakSoftnessID, streakSoftness);
        _materialInstance.SetFloat(RotationID, rotationOffset * Mathf.Deg2Rad); // Convert degrees to radians for shader
        _materialInstance.SetFloat(IntensityID, intensity);
    }

    private void OnDisable()
    {
        if (_materialInstance != null)
        {
            if (Application.isPlaying)
            {
                Destroy(_materialInstance);
            }
            else
            {
                DestroyImmediate(_materialInstance);
            }
        }
    }
}