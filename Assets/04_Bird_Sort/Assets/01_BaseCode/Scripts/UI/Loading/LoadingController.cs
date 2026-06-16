using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingController : MonoBehaviour
{
    public static LoadingPanel Instance;
    [SerializeField] private CanvasGroup canvas = default;
    [SerializeField] private Image slider = default;
    [SerializeField] private Text loadingText;
    [SerializeField] private Button load;
    [SerializeField] private Button next;
    private bool isLoad = false;
    [HideInInspector] public bool isShowOpen = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isLoad = false;
        if (SceneLoader.showMrec) GameController.Instance.admobAds.ShowMRec();
        StartCoroutine(FadeOutScene(SceneLoader.nextScene, SceneLoader.onLoaded));

        next.onClick.AddListener(()=>StartCoroutine(FadeOutScene(SceneLoader.nextScene, SceneLoader.onLoaded)));
        load.onClick.AddListener(()=> GameController.Instance.admobAds.ShowMRec());

    }
    IEnumerator FadeInScene(Action action = null)
    {
        yield return new WaitForFixedUpdate();
        action?.Invoke();
        if (!isLoad)
        {
            isLoad = true;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator FadeOutScene(string scene, Action action = null)
    {
        slider.fillAmount = 0;
        loadingText.text = "0%";
        canvas.alpha = isShowOpen ? 0 : 1;
        canvas.DOFade(1, 0.4f);
        canvas.gameObject.SetActive(true);
        float timerPro = (GameController.Instance.admobAds.IsCanShowMrec()) ? 3 : 1 ;
        while (timerPro > 0)
        {
            timerPro -= Time.fixedDeltaTime;
            slider.fillAmount += Time.fixedDeltaTime / ((GameController.Instance.admobAds.IsCanShowMrec()) ? 6 : 2) ;
            loadingText.text = ((int)Mathf.Round(slider.fillAmount * 100)) + "%";
            yield return new WaitForFixedUpdate();
        }

        float startTime = Time.time;

        string currentScene = SceneManager.GetActiveScene().name;

        float sliderValue = slider.fillAmount;
        AsyncOperation async = SceneManager.LoadSceneAsync(scene);
        async.allowSceneActivation = false;
        while (!async.isDone)
        {
            slider.fillAmount = sliderValue + async.progress / ((GameController.Instance.admobAds.IsCanShowMrec()) ? 4 : 2);
            loadingText.text = ((int)Mathf.Round(slider.fillAmount * 100)) + "%";
            yield return null;
            if (async.progress >= 0.9f)
            {
                slider.fillAmount = 1f;
                loadingText.text = "100%";
                async.allowSceneActivation = true;
                GameController.Instance.admobAds.HideMRec();
            }
        }

        timerPro = 1;
        while (timerPro > 0)
        {
            timerPro -= Time.fixedDeltaTime;
            slider.fillAmount += Time.fixedDeltaTime / 2;
            loadingText.text = ((int)Mathf.Round(slider.fillAmount * 100)) + "%";
            yield return new WaitForFixedUpdate();
        }

        slider.fillAmount = 1;
        loadingText.text = ((int)Mathf.Round(slider.fillAmount * 100)) + "%";
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(FadeInScene(action));
        //if (!currentScene.Equals(scene))
        //{
        //    if (scene.Equals(SceneName.GAME_PLAY))
        //    {
        //        GameController.Instance.musicManager.PlayGameplaySceneBackgroundMusic();
        //    }
        //    else
        //    {
        //        GameController.Instance.musicManager.PlayHomeSceneBackgroundMusic();
        //    }
        //}
    }
}
