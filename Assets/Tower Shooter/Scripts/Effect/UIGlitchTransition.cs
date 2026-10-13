using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIGlitchTransition : MonoBehaviour
{
    private Material glitchMaterial;
    [SerializeField] private float duration = 0.8f; // Speed of the fix animation

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

    // Call this method when the glitch gets fixed, passing an optional onComplete callback
    public void FixGlitch(Action onComplete = null)
    {
        StartCoroutine(AnimateFix(onComplete));
    }

    public void SetGlitchProgress(float value)
    {
        glitchMaterial.SetFloat("_Progress", value);
    }

    private IEnumerator AnimateFix(Action onComplete)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);

            // Set the shader property
            if (glitchMaterial != null)
            {
                glitchMaterial.SetFloat("_Progress", progress);
            }

            yield return null;
        }

        if (glitchMaterial != null)
        {
            glitchMaterial.SetFloat("_Progress", 1.0f);
        }

        // Invoke the callback once the animation finishes
        onComplete?.Invoke();
    }

    // Recommended: Clean up the instantiated material when the object is destroyed 
    // to prevent memory leaks if Unity doesn't handle it automatically.
    // void OnDestroy()
    // {
    //     if (glitchMaterial != null)
    //     {
    //         Destroy(glitchMaterial);
    //     }
    // }
}