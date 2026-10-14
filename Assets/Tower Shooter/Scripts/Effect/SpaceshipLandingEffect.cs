using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SpaceshipLandingEffect : MonoBehaviour
{
    [Header("Transition Settings")]
    [Tooltip("Total duration of the spaceship landing effect in seconds.")]
    public float duration = 2.5f;

    [Tooltip("Optional animation curve to control landing acceleration/deceleration.")]
    public AnimationCurve landingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Auto Play")]
    [Tooltip("Should the landing transition start automatically when the scene/object loads?")]
    public bool playOnStart = true;

    private Image uiImage;
    private Material matInstance;
    private bool isLanding = false;

    // Property ID cache for better CPU performance in URP/HLSL
    private static readonly int ProgressProperty = Shader.PropertyToID("_Progress");

    void Awake()
    {
        uiImage = GetComponent<Image>();

        if (uiImage.material != null)
        {
            // Create a unique material instance so multiple UI elements don't share the same progress value
            matInstance = new Material(uiImage.material);
            uiImage.material = matInstance;
        }
        else
        {
            Debug.LogError("SpaceshipLandingController: Please assign a material with the landing shader to this UI Image!");
        }
    }

    /// <summary>
    /// Call this method from anywhere to trigger the landing transition.
    /// </summary>
    /// <param name="onComplete">Action to execute after the effect fully finishes.</param>
    public void StartLanding(Action onComplete = null)
    {
        if (isLanding) return;
        StartCoroutine(LandingRoutine(onComplete));
    }

    private System.Collections.IEnumerator LandingRoutine(Action onComplete)
    {
        isLanding = true;
        float elapsedTime = 0f;

        if (matInstance != null)
        {
            matInstance.SetFloat(ProgressProperty, 0f);
        }

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float rawProgress = Mathf.Clamp01(elapsedTime / duration);
            
            // Apply animation curve smoothing
            float curvedProgress = landingCurve.Evaluate(rawProgress);

            if (matInstance != null)
            {
                matInstance.SetFloat(ProgressProperty, curvedProgress);
            }

            yield return null;
        }

        // Lock final state to fully finished (Progress = 1.0)
        if (matInstance != null)
        {
            matInstance.SetFloat(ProgressProperty, 1.0f);
        }

        isLanding = false;

        // Fire the subsequent action callback
        onComplete?.Invoke();
    }

    void OnDestroy()
    {
        // Clean up the instantiated material to prevent memory leaks in Unity
        if (matInstance != null)
        {
            Destroy(matInstance);
        }
    }
}