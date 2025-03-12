using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class StartLoading : MonoBehaviour
{
    public Text txtLoading;
    public Image progressBar;
    public bool clearData;
    public void Init()
    {
        Application.targetFrameRate = 60;
        progressBar.fillAmount = 0f;
  
            StartCoroutine(ChangeScene());
    

    
        StartCoroutine(LoadingText());
    }

    // Use this for initialization
    IEnumerator ChangeScene()
    {
        //yield return new WaitForSeconds(2f);

        // we start loading the scene
        //scene_name = GameUtils.SceneName.HOME_SCENE;
        string sceneLoad = SceneName.HOME_SCENE;
        if(UseProfile.CurrentLevel <= RemoteConfigController.GetIntConfig(FirebaseConfig.START_LEVEL_BACK_TO_HOME, 5))
            sceneLoad = SceneName.GAME_PLAY;

        var _asyncOperation = Addressables.LoadSceneAsync(sceneLoad);
        //_asyncOperation.allowSceneActivation = false;
        //Debug.Log("_asyncOperation " + _asyncOperation.progress);
        //// while the scene loads, we assign its progress to a target that we'll use to fill the progress bar smoothly
        while (!_asyncOperation.IsDone)
        {
            progressBar.fillAmount = Mathf.Clamp01(_asyncOperation.PercentComplete / 0.9f);
            yield return null;
        }

        //// we switch to the new scene
        //_asyncOperation.allowSceneActivation = true;
    }

    IEnumerator LoadingText()
    {
        var wait = new WaitForSeconds(1f);
        while (true)
        {
            txtLoading.text = Localization.Get("s_loading") + " .";
            yield return wait;

            txtLoading.text = Localization.Get("s_loading") + "..";
            yield return wait;

            txtLoading.text = Localization.Get("s_loading") + "...";
            yield return wait;

        }
    }

   

}
