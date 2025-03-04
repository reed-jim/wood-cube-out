using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
}
