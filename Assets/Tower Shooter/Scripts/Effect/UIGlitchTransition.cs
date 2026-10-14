using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIGlitchTransition : MonoBehaviour
{
    private Material glitchMaterial;
    [SerializeField] private float duration = 0.8f; // Speed of the transition animation

    void Awake()
    {
        Image img = GetComponent<Image>();
        if (img != null)
        {
            // materialForRendering automatically instantiates a material instance 
            // unique to this graphic component, preventing modification of shared assets.
            glitchMaterial = img.materialForRendering;
        }
    }

    // Call this method when closing the reward popup, passing an optional onComplete callback
    public void FixGlitch(Action onComplete = null)
    {
        StartCoroutine(AnimateFix(onComplete));
    }

    public void SetGlitchProgress(float value)
    {
        if (glitchMaterial != null)
        {
            glitchMaterial.SetFloat("_Transition", value);
        }
    }

    private IEnumerator AnimateFix(Action onComplete)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);

            // Use SmoothStep for a nice easing feel on the shatter transition
            float curvedProgress = Mathf.SmoothStep(0f, 1f, progress);

            if (glitchMaterial != null)
            {
                glitchMaterial.SetFloat("_Transition", curvedProgress);
            }

            yield return null;
        }

        if (glitchMaterial != null)
        {
            glitchMaterial.SetFloat("_Transition", 1.0f);
        }

        // Invoke the callback once the animation finishes (e.g. to gameObject.SetActive(false))
        onComplete?.Invoke();
    }

    // void OnDestroy()
    // {
    //     if (glitchMaterial != null)
    //     {
    //         Destroy(glitchMaterial);
    //     }
    // }
}