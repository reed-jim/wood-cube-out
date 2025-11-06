using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Some functions commonly used with a button
/// </summary>
public class ButtonListener : MonoBehaviour
{
    IEnumerator CloneObjectWithAddressable(string address)
    {
        AsyncOperationHandle<GameObject> op = Addressables.InstantiateAsync(address);
        yield return op;

        if (op.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject instance = op.Result;
            // Do something with the instantiated GameObject
        }
        else
        {
            Debug.LogError("Failed to load addressable: " + address);
        }
    }

    public void DestroyThisGameObject(GameObject a)
    {
        Destroy(a);
    }

    public void SwitchActiveState(GameObject a)
    {
        if (a.activeSelf == true)
        {
            a.SetActive(false);
            return;
        }
        a.SetActive(true);
    }
}
