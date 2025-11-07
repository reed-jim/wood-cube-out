using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class BootLoader : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(LoadMainScene());
    }

    IEnumerator LoadMainScene()
    {
        // Skip one frame, so in case of pause event, we can react
        yield return null;

        SceneManager.LoadScene(GameConstants.LOADING_SCENE);

        // Addressables.LoadSceneAsync(GameConstants.LOADING_SCENE);
    }

    System.Collections.IEnumerator LoadFromGitHub()
    {
        var handle = Addressables.LoadContentCatalogAsync("https://raw.githubusercontent.com/ninhnk2000/wood-cube-out-content/refs/heads/main/Content/catalog_21.hash");

        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("SAFERIO Catalog loaded successfully.");
            
            

            Addressables.LoadSceneAsync(GameConstants.LOADING_SCENE);
        }
        else
        {
            Debug.LogError("SAFERIO Failed to load catalog.");
        }
    }

    IEnumerator DownloadWithProgress(object key)
    {
        var handle = Addressables.DownloadDependenciesAsync(key, true);
        while (!handle.IsDone)
        {
            Debug.Log($"Downloading: {handle.PercentComplete * 100}%");
            yield return null;
        }

        Debug.Log("Download complete.");
    }
}
