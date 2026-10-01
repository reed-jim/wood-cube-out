using System.Collections;
using UnityEngine;

public class TornadoController : MonoBehaviour
{
    public UITornadoEffect tornadoEffect;
    public CanvasGroup canvasGroup;

    [ContextMenu("Trigger Tornado Out")]
    public void SuckIntoTornado(float duration = 1.5f)
    {
        StartCoroutine(AnimateTornado(0f, 1f, duration, fadeUI: true));
    }

    [ContextMenu("Trigger Tornado In")]
    public void ReleaseFromTornado(float duration = 1.5f)
    {
        StartCoroutine(AnimateTornado(1f, 0f, duration, fadeUI: false));
    }

    private IEnumerator AnimateTornado(float start, float end, float duration, bool fadeUI)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Smooth exponential curve for punchy vortex motion
            float curve = Mathf.SmoothStep(0f, 1f, t);

            tornadoEffect.tornadoProgress = Mathf.Lerp(start, end, curve);

            if (canvasGroup != null && fadeUI)
            {
                // Gradually collapse alpha as it gets sucked up
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, curve * curve);
            }
            else if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, curve);
            }

            yield return null;
        }

        tornadoEffect.tornadoProgress = end;
    }
}
