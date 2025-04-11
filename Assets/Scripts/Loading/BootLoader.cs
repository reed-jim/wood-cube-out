using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

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

        Addressables.LoadSceneAsync(GameConstants.LOADING_SCENE);
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
}
