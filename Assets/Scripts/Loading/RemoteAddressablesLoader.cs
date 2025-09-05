// using System;
// using System.Collections;
// using System.Collections.Generic;
// using PrimeTween;
// using Spine.Unity;
// using TMPro;
// using UnityEngine;
// using UnityEngine.AddressableAssets;
// using UnityEngine.ResourceManagement.AsyncOperations;
// using UnityEngine.ResourceManagement.ResourceProviders;
// using UnityEngine.SceneManagement;
// using UnityEngine.UI;

// public class RemoteAddressablesLoader : MonoBehaviour
// {

//     [Header("REMOTE ADDRESSABLES")]
//     [SerializeField] private TMP_Text downloadProgressText;

//     [Header("NO INTERNET")]
//     [SerializeField] private RectTransform noInternetContainer;
//     [SerializeField] private Button retryConnectButton;

//     [Header("CUSTOMIZE")]
//     [SerializeField] private float maxExpectedMenuLoadTime;
//     [SerializeField] private float loadingDuration;
//     [SerializeField] private float fadeOutDuration;

//     private List<Tween> _tweens;

//     public static event Action<string> updateDownloadProgressEvent;
//     public static event Action retryLoadLevelEvent;
//     public static event Action downloadRemoteContentCompletedEvent;

//     private void Awake()
//     {
//         LoadLevelErrorScene.retryLoadRemoteContentEvent += RetryConnection;

//         _tweens = new List<Tween>();
//     }

//     void Start()
//     {
//         // StartCoroutine(LoadFromServer());
//     }

//     private void OnDestroy()
//     {
//         LoadLevelErrorScene.retryLoadRemoteContentEvent -= RetryConnection;

//         CommonUtil.StopAllTweens(_tweens);
//     }

//     #region REMOTE ADDRESSABLES
//     System.Collections.IEnumerator LoadFromServer()
//     {
//         // Addressables.InitializeAsync().Completed += handle =>
//         // {
//         //     Addressables.CheckForCatalogUpdates().Completed += catalogHandle =>
//         //     {
//         //         if (catalogHandle.Result != null && catalogHandle.Result.Count > 0)
//         //         {
//         //             Addressables.UpdateCatalogs(catalogHandle.Result).Completed += updateHandle =>
//         //             {
//         //                 StartCoroutine(DownloadWithProgress());
//         //             };
//         //         }
//         //         else
//         //         {
//         //             StartCoroutine(DownloadWithProgress());
//         //         }
//         //     };
//         // };

//         // yield return null;

//         yield return new WaitForSeconds(0.2f);

//         bool isInternetConnected = Application.internetReachability != NetworkReachability.NotReachable;

//         if (isInternetConnected)
//         {
//             updateDownloadProgressEvent?.Invoke("Retrieving data from the server. Please wait a moment.");

//             var handle = Addressables.LoadContentCatalogAsync("https://raw.githubusercontent.com/ninhnk2000/wood-cube-out-content/refs/heads/main/Content/catalog_22.hash");

//             yield return handle;

//             if (handle.Status == AsyncOperationStatus.Succeeded)
//             {
//                 updateDownloadProgressEvent?.Invoke("Catalog loaded successfully.");

//                 StartCoroutine(DownloadWithProgress());
//             }
//             else
//             {

//             }
//         }
//         else
//         {
//             updateDownloadProgressEvent?.Invoke("No internet connection detected. Please check your connection and try again.");
//         }
//     }

//     IEnumerator DownloadWithProgress()
//     {
//         WaitForSeconds waitForSeconds = new WaitForSeconds(0.2f);

//         AsyncOperationHandle downloadHandle = new AsyncOperationHandle();

//         List<object> allKeys = new List<object>();

//         foreach (var locator in Addressables.ResourceLocators)
//         {
//             foreach (var key in locator.Keys)
//             {
//                 allKeys.Add(key);
//             }
//         }

//         var handle = Addressables.GetDownloadSizeAsync(allKeys);

//         handle.Completed += handle =>
//         {
//             if (handle.Status == AsyncOperationStatus.Succeeded)
//             {
//                 long downloadSize = handle.Result;

//                 if (downloadSize > 0)
//                 {
//                     StartCoroutine(DownloadingAddressables(allKeys));
//                 }
//                 else
//                 {
//                     GamePersistentVariable.isRemoteLevelsDownloaded = true;

//                     updateDownloadProgressEvent?.Invoke($"All data has been successfully downloaded.");
//                 }
//             }
//         };

//         while (!handle.IsDone)
//         {
//             updateDownloadProgressEvent?.Invoke($"Preparing to download remote content");

//             yield return waitForSeconds;
//         }

//         yield return null;
//     }

//     private IEnumerator DownloadingAddressables(List<object> allKeys)
//     {
//         WaitForSeconds waitForSeconds = new WaitForSeconds(0.2f);

//         var downloadHandle = Addressables.DownloadDependenciesAsync(allKeys, Addressables.MergeMode.Union, true);

//         while (!downloadHandle.IsDone)
//         {
//             updateDownloadProgressEvent?.Invoke($"Downloading - {downloadHandle.PercentComplete * 100:F2}%");

//             yield return waitForSeconds;
//         }

//         GamePersistentVariable.isRemoteLevelsDownloaded = true;

//         updateDownloadProgressEvent?.Invoke($"All data has been successfully downloaded.");
//         downloadRemoteContentCompletedEvent?.Invoke();
//         // retryLoadLevelEvent?.Invoke();
//     }
//     #endregion

//     #region NO INTERNET
//     private void RetryConnection()
//     {
//         StartCoroutine(LoadFromServer());
//     }
//     #endregion
// }
