using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RemoteAddressablesLoader : MonoBehaviour
{

    [Header("REMOTE ADDRESSABLES")]
    [SerializeField] private TMP_Text downloadProgressText;

    [Header("NO INTERNET")]
    [SerializeField] private RectTransform noInternetContainer;
    [SerializeField] private Button retryConnectButton;

    [Header("CUSTOMIZE")]
    [SerializeField] private float maxExpectedMenuLoadTime;
    [SerializeField] private float loadingDuration;
    [SerializeField] private float fadeOutDuration;

    private List<Tween> _tweens;

    public static event Action<string> updateDownloadProgressEvent;

    private void Awake()
    {
        _tweens = new List<Tween>();
    }

    void Start()
    {
        StartCoroutine(LoadFromServer());
    }

    private void OnDestroy()
    {
        CommonUtil.StopAllTweens(_tweens);
    }

    #region REMOTE ADDRESSABLES
    System.Collections.IEnumerator LoadFromServer()
    {
        // Addressables.InitializeAsync().Completed += handle =>
        // {
        //     Debug.Log("SAFERIO Addressables Initialized");

        //     Addressables.CheckForCatalogUpdates().Completed += catalogHandle =>
        //     {
        //         Debug.Log("SAFERIO Addressables Catalog " + catalogHandle.Result.Count);

        //         if (catalogHandle.Result != null && catalogHandle.Result.Count > 0)
        //         {
        //             Addressables.UpdateCatalogs(catalogHandle.Result).Completed += updateHandle =>
        //             {
        //                 StartCoroutine(DownloadWithProgress());
        //             };
        //         }
        //         else
        //         {
        //             StartCoroutine(DownloadWithProgress());
        //         }
        //     };
        // };

        // yield return null;

        updateDownloadProgressEvent?.Invoke("Retrieving data from the server. Please wait a moment.");

        yield return new WaitForSeconds(0.2f);

        bool isInternetConnected = Application.internetReachability != NetworkReachability.NotReachable;

        if (isInternetConnected)
        {
            var handle = Addressables.LoadContentCatalogAsync("https://raw.githubusercontent.com/ninhnk2000/wood-cube-out-content/refs/heads/main/Content/catalog_22.hash");

            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                updateDownloadProgressEvent?.Invoke("Catalog loaded successfully.");

                StartCoroutine(DownloadWithProgress());
            }
            else
            {

            }
        }
    }

    IEnumerator DownloadWithProgress()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.2f);

        AsyncOperationHandle downloadHandle = new AsyncOperationHandle();

        List<object> allKeys = new List<object>();

        foreach (var locator in Addressables.ResourceLocators)
        {
            foreach (var key in locator.Keys)
            {
                allKeys.Add(key);
            }
        }

        Addressables.GetDownloadSizeAsync(allKeys).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                long downloadSize = handle.Result;

                if (downloadSize > 0)
                {
                    Debug.Log($"SAFERIO Download size: {downloadSize / (1024f * 1024f):F2} MB");

                    StartCoroutine(DownloadingAddressables(allKeys));
                }
                else
                {
                    GamePersistentVariable.isRemoteLevelsDownloaded = true;

                    updateDownloadProgressEvent?.Invoke($"All data has been successfully downloaded.");

                    Debug.Log("SAFERIO All content already cached.");
                }
            }
        };

        yield return null;
    }

    private IEnumerator DownloadingAddressables(List<object> allKeys)
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.2f);

        var downloadHandle = Addressables.DownloadDependenciesAsync(allKeys, Addressables.MergeMode.Union, true);

        while (!downloadHandle.IsDone)
        {
            updateDownloadProgressEvent?.Invoke($"Downloading - {downloadHandle.PercentComplete * 100:F2}%");

            yield return waitForSeconds;
        }

        GamePersistentVariable.isRemoteLevelsDownloaded = true;

        updateDownloadProgressEvent?.Invoke($"All data has been successfully downloaded.");
    }
    #endregion

    #region NO INTERNET
    private void RetryConnection()
    {
        StartCoroutine(LoadFromServer());
    }
    #endregion
}
