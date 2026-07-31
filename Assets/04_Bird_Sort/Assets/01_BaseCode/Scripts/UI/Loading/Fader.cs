using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NamPhuThuy.GamePlay;

public class Fader : MonoBehaviour
{
    [HideInInspector] public bool start = false;
    [HideInInspector] public float fadeDamp = 0.0f;
    [HideInInspector] public string fadeScene;
    [HideInInspector] public float alpha = 0.0f;
    [HideInInspector] public Color fadeColor;
    [HideInInspector] public bool isFadeIn = false;
    [HideInInspector] public AsyncOperation externalOp;

    CanvasGroup myCanvas;
    Image bg;
    float lastTime = 0;
    bool startedLoading = false;
    AsyncOperation _loadOp;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
        SceneTransition.levelSceneTransitionCompleteEvent += OnGameplayStartLevel;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        SceneTransition.levelSceneTransitionCompleteEvent -= OnGameplayStartLevel;
    }

    public void InitiateFader()
    {
        DontDestroyOnLoad(gameObject);

        if (transform.GetComponent<CanvasGroup>())
            myCanvas = transform.GetComponent<CanvasGroup>();

        if (transform.GetComponentInChildren<Image>())
        {
            bg = transform.GetComponent<Image>();
            bg.color = fadeColor;
        }

        if (myCanvas && bg)
        {
            myCanvas.alpha = 0.0f;
            StartCoroutine(FadeIt());
        }
        else
            Debug.LogWarning("Something is missing please reimport the package.");
    }

    public void InitiateFaderWithExternalOp()
    {
        DontDestroyOnLoad(gameObject);

        if (transform.GetComponent<CanvasGroup>())
            myCanvas = transform.GetComponent<CanvasGroup>();

        if (transform.GetComponentInChildren<Image>())
        {
            bg = transform.GetComponent<Image>();
            bg.color = fadeColor;
        }

        if (myCanvas && bg)
        {
            myCanvas.alpha = 0.0f; // Bắt đầu trong suốt, sẽ fade in to black
            StartCoroutine(FadeIt());
        }
        else
            Debug.LogWarning("Something is missing please reimport the package.");
    }

    IEnumerator FadeIt()
    {
        while (!start)
            yield return null;

        lastTime = Time.time;
        float coDelta;
        bool hasFadedIn = false;

        while (!hasFadedIn)
        {
            coDelta = Time.time - lastTime;

            if (!isFadeIn)
            {
                // Fade to black
                alpha = newAlpha(coDelta, 1, alpha);

                if (externalOp != null)
                {
                    if (alpha >= 1f && externalOp.progress >= 0.9f)
                    {
                        externalOp.allowSceneActivation = true;
                    }
                }
                else
                {
                    if (!startedLoading)
                    {
                        startedLoading = true;
                        _loadOp = SceneManager.LoadSceneAsync(fadeScene);
                        _loadOp.allowSceneActivation = false;
                    }

                    if (alpha >= 1f && _loadOp != null && _loadOp.progress >= 0.9f)
                    {
                        _loadOp.allowSceneActivation = true;
                    }
                }
            }
            else
            {
                // Fade out (sáng lại)
                alpha = newAlpha(coDelta, 0, alpha);
                if (alpha <= 0)
                {
                    hasFadedIn = true;
                }
            }

            lastTime = Time.time;
            myCanvas.alpha = alpha;
            yield return null;
        }

        Initiate.DoneFading();
        Destroy(gameObject);
    }

    float newAlpha(float delta, int to, float currAlpha)
    {
        switch (to)
        {
            case 0:
                currAlpha -= fadeDamp * delta;
                if (currAlpha <= 0) currAlpha = 0;
                break;
            case 1:
                currAlpha += fadeDamp * delta;
                if (currAlpha >= 1) currAlpha = 1;
                break;
        }
        return currAlpha;
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == GameConstants.GAMEPLAY_SCENE || scene.name == SceneName.GAME_PLAY)
        {
            return; // Gameplay scene: không fade in ngay, đợi levelSceneTransitionCompleteEvent
        }

        // Non-gameplay (Home scene, v.v.): fade out ngay khi scene load xong
        start = true;
        alpha = 1f;
        isFadeIn = true;
        StartCoroutine(FadeIt());
    }

    void OnGameplayStartLevel()
    {
        var sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == GameConstants.GAMEPLAY_SCENE || sceneName == SceneName.GAME_PLAY)
        {
            // SceneTransition đã hoàn thành → fade out black overlay
            start = true;
            alpha = 1f;
            myCanvas.alpha = 1f;
            isFadeIn = true;
            StartCoroutine(FadeIt());
        }
    }
}
