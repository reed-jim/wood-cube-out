using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class StatUpgradeEffectController : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float duration = 0.6f;
    [SerializeField] private Ease easeType = Ease.OutQuad;

    [Header("Visual Variations")]
    [SerializeField] private Color[] colorThemes = new Color[]
    {
        new Color(1.0f, 0.6f, 0.1f), // Solar Gold / Orange
        new Color(0.2f, 0.8f, 1.0f), // Hextech Blue
        new Color(1.0f, 0.3f, 0.8f), // Arcane Magenta
        new Color(0.3f, 1.0f, 0.5f)  // Emerald Crit
    };

    [SerializeField] private Image targetImage;

    private Material matInstance;
    private Tween activeTween;
    
    private const string ProgressParam = "_Progress";
    private const string SeedParam = "_Seed";
    private const string ColorParam = "_Color";

    private bool IsPlaying => activeTween != null && activeTween.IsActive() && activeTween.IsPlaying();

    void Awake()
    {
        if (targetImage.material != null)
        {
            matInstance = new Material(targetImage.material);
            targetImage.material = matInstance;
        }

        SetProgress(0f);
    }

    void OnDestroy()
    {
        activeTween?.Kill();
        if (matInstance != null)
        {
            Destroy(matInstance);
        }
    }

    /// <summary>
    /// Triggers the upgrade animation with randomized variation if not already playing.
    /// </summary>
    public bool TryTriggerUpgrade()
    {
        if (IsPlaying || matInstance == null) return false;

        // 1. Randomize Seed so particle trajectories and noise patterns change every time
        float randomSeed = Random.Range(0f, 999.5f);
        matInstance.SetFloat(SeedParam, randomSeed);

        // 2. Randomize Color Theme if themes are provided
        if (colorThemes != null && colorThemes.Length > 0)
        {
            Color selectedColor = colorThemes[Random.Range(0, colorThemes.Length)];
            matInstance.SetColor(ColorParam, selectedColor);
        }

        // Reset progress before animating
        SetProgress(0f);

        // Play Tween from 0 to 1
        activeTween = DOTween.To(() => GetProgress(), x => SetProgress(x), 1f, duration)
            .SetEase(easeType)
            .OnComplete(() => SetProgress(0f));

        return true;
    }

    private float GetProgress()
    {
        return matInstance.GetFloat(ProgressParam);
    }

    private void SetProgress(float value)
    {
        matInstance.SetFloat(ProgressParam, value);
    }
}