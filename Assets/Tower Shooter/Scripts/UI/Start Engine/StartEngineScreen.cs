using System;
using System.Collections;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class StartEngineScreen : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button startEngineButton;

    #region Physical Platform Phase 1
    [SerializeField] Image physicalPlatform;
    private Material physicalPlatformMat;
    #endregion

    #region Physical Screen Phase 2
    static readonly int Power = Shader.PropertyToID("_Power");
    [SerializeField] Image physicalScreen;
    [SerializeField] float bootPlatformTime = 0.6f;
    [SerializeField] float bootTime = 0.6f;
    private Material physicalScreenMat;
    #endregion

    void Awake()
    {
        startEngineButton.onClick.AddListener(StartEngine);

        physicalPlatformMat = Instantiate(physicalPlatform.material);
        physicalPlatform.material = physicalPlatformMat;

        physicalScreenMat = Instantiate(physicalScreen.material);
        physicalScreen.material = physicalScreenMat;
    }

    private void StartEngine()
    {
        StartCoroutine(BootingPlatform(onComplete: () =>
        {
            Tween.LocalPositionY(
                physicalPlatform.transform, physicalPlatform.rectTransform.localPosition.y - physicalPlatform.rectTransform.sizeDelta.y, duration: 0.3f);
            Tween.LocalPositionY(physicalScreen.transform, 0, duration: 0.3f)
            .OnComplete(() =>
            {
                StartCoroutine(BootingSystem(onComplete: () =>
                {
                    gameObject.SetActive(false);
                }));
            });
        }));
    }

    IEnumerator BootingPlatform(Action onComplete)
    {
        float start = physicalPlatformMat.GetFloat(Power);
        float dur = bootPlatformTime;

        for (float t = 0; t < dur; t += Time.unscaledDeltaTime)
        {
            physicalPlatformMat.SetFloat(Power, Mathf.Lerp(start, 1, t / dur));
            yield return null;
        }

        physicalPlatformMat.SetFloat(Power, 1);

        onComplete?.Invoke();
    }

    IEnumerator BootingSystem(Action onComplete)
    {
        float start = physicalScreenMat.GetFloat(Power);
        float dur = bootTime;

        for (float t = 0; t < dur; t += Time.unscaledDeltaTime)
        {
            physicalScreenMat.SetFloat(Power, Mathf.Lerp(start, 1, t / dur));
            yield return null;
        }

        physicalScreenMat.SetFloat(Power, 1);

        gameObject.SetActive(false);

        onComplete?.Invoke();
    }
}
