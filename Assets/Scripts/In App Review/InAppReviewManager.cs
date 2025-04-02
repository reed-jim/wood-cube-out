// using System.Collections;
// using Google.Play.Review;
// using Unity.VisualScripting;
// using UnityEngine;
// using UnityEngine.iOS;

// public class InAppReviewManager : MonoBehaviour
// {
// #if UNITY_ANDROID
//     private ReviewManager _reviewManager;
//     private PlayReviewInfo _playReviewInfo;
//     private Coroutine _coroutine;
// #endif

//     void Awake()
//     {
//         RatePopup.launchInAppReviewEvent += RateAndReview;
//     }

//     private void Start()
//     {
// #if UNITY_ANDROID
//         _coroutine = StartCoroutine(InitReview());
// #endif
//     }

//     void OnDestroy()
//     {
//         RatePopup.launchInAppReviewEvent += RateAndReview;
//     }

//     public void RateAndReview()
//     {
// #if UNITY_IOS
//         Device.RequestStoreReview();
// #elif UNITY_ANDROID
//         StartCoroutine(LaunchReview());
// #endif
//     }

// #if UNITY_ANDROID
//     private IEnumerator InitReview(bool force = false)
//     {
//         if (_reviewManager == null) _reviewManager = new ReviewManager();

//         var requestFlowOperation = _reviewManager.RequestReviewFlow();
//         yield return requestFlowOperation;
//         if (requestFlowOperation.Error != ReviewErrorCode.NoError)
//         {
//             if (force) OpenReviewPage();
//             yield break;
//         }

//         _playReviewInfo = requestFlowOperation.GetResult();
//     }

//     public IEnumerator LaunchReview()
//     {
//         if (_playReviewInfo == null)
//         {
//             if (_coroutine != null) StopCoroutine(_coroutine);
//             yield return StartCoroutine(InitReview(true));
//         }

//         var launchFlowOperation = _reviewManager.LaunchReviewFlow(_playReviewInfo);
//         yield return launchFlowOperation;
//         _playReviewInfo = null;
//         if (launchFlowOperation.Error != ReviewErrorCode.NoError)
//         {
//             OpenReviewPage();
//             yield break;
//         }
//     }
// #endif

//     public void OpenReviewPage()
//     {
//         string url = "https://play.google.com/store/apps/details?id=com.gplay.wood.cube.out";

//         Application.OpenURL(url);
//     }

//     // private ReviewManager _reviewManager;
//     // private PlayReviewInfo _playReviewInfo;

//     // private void Awake()
//     // {
//     //     RatePopup.launchInAppReviewEvent += LaunchInAppReview;

//     //     _reviewManager = new ReviewManager();
//     // }

//     // private void OnDestroy()
//     // {
//     //     RatePopup.launchInAppReviewEvent += LaunchInAppReview;
//     // }

//     // private void LaunchInAppReview()
//     // {
//     //     StartCoroutine(LaunchingInAppReview());
//     // }

//     // private IEnumerator LaunchingInAppReview()
//     // {
//     //     var requestFlowOperation = _reviewManager.RequestReviewFlow();

//     //     yield return requestFlowOperation;

//     //     if (requestFlowOperation.Error != ReviewErrorCode.NoError)
//     //     {
//     //         yield break;
//     //     }

//     //     _playReviewInfo = requestFlowOperation.GetResult();

//     //     yield return new WaitForSeconds(3);

//     //     var launchFlowOperation = _reviewManager.LaunchReviewFlow(_playReviewInfo);

//     //     yield return launchFlowOperation;

//     //     _playReviewInfo = null;

//     //     Debug.Log("SAFERIO " + launchFlowOperation.IsDone + "/" + launchFlowOperation.IsSuccessful);

//     //     if (launchFlowOperation.Error != ReviewErrorCode.NoError)
//     //     {
//     //         yield break;
//     //     }
//     // }
// }
