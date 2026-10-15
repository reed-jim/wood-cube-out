using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CosmicButtonController : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 0.3f;

    private Image _image;
    private Material _materialInstance;
    private Coroutine _fadeCoroutine;

    // Matches the shader property name
    private static readonly int EffectIntensityID = Shader.PropertyToID("_EffectIntensity");

    // private void Awake()
    // {
    //     _image = GetComponent<Image>();

    //     // Instantiate material to avoid modifying the asset on disk
    //     if (_image.material != null)
    //     {
    //         _materialInstance = new Material(_image.material);
    //         _image.material = _materialInstance;
    //     }
    // }

    // do not create another material because SaferioButton have already created one
    void Start()
    {
        EnsureMaterialInstance();
    }

    /// <summary>
    /// Instantly set the intensity (0 = default sprite image, 1 = full space shader).
    /// </summary>
    public void SetIntensityInstant(float targetIntensity)
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

        EnsureMaterialInstance();
        
        targetIntensity = Mathf.Clamp01(targetIntensity);
        _materialInstance.SetFloat(EffectIntensityID, targetIntensity);
    }

    /// <summary>
    /// Smoothly transition intensity over time.
    /// </summary>
    public void TransitionIntensity(float targetIntensity)
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(AnimateIntensity(targetIntensity));
    }

    private IEnumerator AnimateIntensity(float target)
    {
        EnsureMaterialInstance();

        target = Mathf.Clamp01(target);
        float start = _materialInstance.GetFloat(EffectIntensityID);
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float current = Mathf.Lerp(start, target, time / fadeDuration);
            _materialInstance.SetFloat(EffectIntensityID, current);
            yield return null;
        }

        _materialInstance.SetFloat(EffectIntensityID, target);
    }

    private void OnDestroy()
    {
        // Clean up runtime material clone to prevent memory leaks
        if (_materialInstance != null)
        {
            Destroy(_materialInstance);
        }
    }

    private void EnsureMaterialInstance()
    {
        if (_materialInstance == null)
        {
            _image = GetComponent<Image>();

            _materialInstance = _image.material;
        }
    }
}
