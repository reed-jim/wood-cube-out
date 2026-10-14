using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class FullscreenEffectController : MonoBehaviour
{
    private Material instanceMaterial;
    private Graphic uiGraphic;
    private Coroutine glitchCoroutine;

    private static readonly int LongPressProgressID = Shader.PropertyToID("_LongPressProgress");
    private static readonly int GlitchTriggerID = Shader.PropertyToID("_GlitchTrigger");

    private void Awake()
    {
        uiGraphic = GetComponent<Graphic>();
        if (uiGraphic.material != null)
        {
            instanceMaterial = new Material(uiGraphic.material);
            uiGraphic.material = instanceMaterial;
        }
    }

    private void OnDestroy()
    {
        if (instanceMaterial != null)
        {
            Destroy(instanceMaterial);
        }
    }

    public void SetLongPressProgress(float progress)
    {
        if (instanceMaterial != null)
        {
            instanceMaterial.SetFloat(LongPressProgressID, Mathf.Clamp01(progress));
        }
    }

    public void PlayReleaseEffect()
    {
        if (glitchCoroutine != null)
        {
            StopCoroutine(glitchCoroutine);
        }
        glitchCoroutine = StartCoroutine(AnimateReleaseRoutine());
    }

    private IEnumerator AnimateReleaseRoutine()
    {
        float duration = 0.4f;
        float elapsed = 0f;

        if (instanceMaterial != null)
        {
            instanceMaterial.SetFloat(LongPressProgressID, 0f);
        }

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            if (instanceMaterial != null)
            {
                instanceMaterial.SetFloat(GlitchTriggerID, t);
            }
            yield return null;
        }

        if (instanceMaterial != null)
        {
            instanceMaterial.SetFloat(GlitchTriggerID, 0f);
        }
    }
}