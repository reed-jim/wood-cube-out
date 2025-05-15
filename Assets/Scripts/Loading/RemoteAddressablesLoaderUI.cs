using TMPro;
using UnityEngine;

public class RemoteAddressablesLoaderUI : MonoBehaviour
{
    [SerializeField] private TMP_Text downloadProgressText;

    void Awake()
    {
        RemoteAddressablesLoader.updateDownloadProgressEvent += UpdateDownloadProgress;

        downloadProgressText.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        RemoteAddressablesLoader.updateDownloadProgressEvent -= UpdateDownloadProgress;
    }

    private void UpdateDownloadProgress(string text)
    {
        if (!downloadProgressText.gameObject.activeInHierarchy)
        {
            downloadProgressText.gameObject.SetActive(true);
        }

        downloadProgressText.text = text;
    }
}
