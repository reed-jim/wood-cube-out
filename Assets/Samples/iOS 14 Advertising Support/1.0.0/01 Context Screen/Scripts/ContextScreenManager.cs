using System.Collections;
using Unity.Advertisement.IosSupport.Components;
using Unity.Notifications.iOS;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.Advertisement.IosSupport.Samples
{
    /// <summary>
    /// This component will trigger the context screen to appear when the scene starts,
    /// if the user hasn't already responded to the iOS tracking dialog.
    /// </summary>
    public class ContextScreenManager : MonoBehaviour
    {
        /// <summary>
        /// The prefab that will be instantiated by this component.
        /// The prefab has to have an ContextScreenView component on its root GameObject.
        /// </summary>
        public ContextScreenView contextScreenPrefab;

        void Start()
        {
            // #if UNITY_IOS
            //             // check with iOS to see if the user has accepted or declined tracking
            //             var status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();

            //             Debug.Log("SAFERIO Request " + status);

            //             if (status == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            //             {
            //                 Debug.Log("SAFERIO Request");

            //                 ATTrackingStatusBinding.RequestAuthorizationTracking();

            //                 // var contextScreen = Instantiate(contextScreenPrefab).GetComponent<ContextScreenView>();

            //                 // // after the Continue button is pressed, and the tracking request
            //                 // // has been sent, automatically destroy the popup to conserve memory
            //                 // contextScreen.sentTrackingAuthorizationRequest += () => Destroy(contextScreen.gameObject);
            //             }
            // #else
            //             Debug.Log("Unity iOS Support: App Tracking Transparency status not checked, because the platform is not iOS.");
            // #endif

            StartCoroutine(LoadNextScene());
        }

        private IEnumerator LoadNextScene()
        {
            yield return new WaitForSeconds(1);

#if UNITY_IOS && !UNITY_EDITOR
            var status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();

            if (status == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                ATTrackingStatusBinding.RequestAuthorizationTracking();

                // var contextScreen = Instantiate(contextScreenPrefab).GetComponent<ContextScreenView>();

                // // after the Continue button is pressed, and the tracking request
                // // has been sent, automatically destroy the popup to conserve memory
                // contextScreen.sentTrackingAuthorizationRequest += () => Destroy(contextScreen.gameObject);
            }

            while (status == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();

                yield return null;
            }
#endif

            StartCoroutine(RequestNotificationAuthorization());

            yield return null;
        }

        IEnumerator RequestNotificationAuthorization()
        {
#if UNITY_IOS && !UNITY_EDITOR
            var authorizationOption = AuthorizationOption.Alert | AuthorizationOption.Badge;
            using (var req = new AuthorizationRequest(authorizationOption, true))
            {
                while (!req.IsFinished)
                {
                    yield return null;
                }

                string res = "\n RequestAuthorization:";
                res += "\n finished: " + req.IsFinished;
                res += "\n granted :  " + req.Granted;
                res += "\n error:  " + req.Error;
                res += "\n deviceToken:  " + req.DeviceToken;
                Debug.Log(res);
            }
#endif

            SceneManager.LoadScene("Bootstrap");

            yield return null;
        }
    }
}
