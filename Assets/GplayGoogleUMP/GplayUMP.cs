using System;
using System.Collections;
using GoogleMobileAds.Ump.Api;
using UnityEngine;
using UnityEngine.Events;

namespace GPlay.GplayGoogleUMP
{
    public class GplayUMP
    {
        private const string ON_OFF_CMP = "on_off_cmp_config";
        private const string IS_CHOOSE_CMP = "is_choose_cmp";

        private static Action<bool> _onConsentValidated;


        public static bool IsChooseCMP
        {
            get => PlayerPrefs.GetInt(IS_CHOOSE_CMP, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(IS_CHOOSE_CMP, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        private static IEnumerator loadConfigCrountine;
        private static IEnumerator timeOutCrountine;

        public static void ShowConsentForm(Action<bool> onConsentValidated)
        {
            _onConsentValidated = onConsentValidated;
            UnityAction cmpAction = () =>
            {
                ConsentRequestParameters request = new ConsentRequestParameters
                {
                    TagForUnderAgeOfConsent = false,
                };

                // Check the current consent information status.
                ConsentInformation.Update(request, OnConsentInfoUpdated);
            };

            if (IsChooseCMP)
            {
                cmpAction?.Invoke();
            }
            else
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    onConsentValidated?.Invoke(true);
                    return;
                }
                else
                {
                    loadConfigCrountine = Helper.StartAction(() =>
                    {
                        bool onCMP = RemoteConfigController.GetBoolConfig(ON_OFF_CMP, true);

                        if (onCMP)
                        {
                            // Set tag for under age of consent.
                            // Here false means users are not under age of consent.
                            cmpAction?.Invoke();
                        }
                        else
                        {
                            onConsentValidated?.Invoke(true);
                        }

                        if (timeOutCrountine != null)
                            GameController.Instance.StopCoroutine(timeOutCrountine);

                    }, () => RemoteConfigController.isFetchDone);

                    timeOutCrountine = Helper.StartAction(() =>
                    {
                        onConsentValidated?.Invoke(true);
                        if (loadConfigCrountine != null)
                            GameController.Instance.StopCoroutine(loadConfigCrountine);
                    }, 3);


                    GameController.Instance.StartCoroutine(loadConfigCrountine);
                    GameController.Instance.StartCoroutine(timeOutCrountine);
                }
            }
        }

        private static void OnConsentInfoUpdated(FormError consentError)
        {
            if (consentError != null)
            {
                // Handle the error.
                Debug.LogError("FalconUMP > OnConsentInfoUpdated === Error: " + consentError);
                _onConsentValidated?.Invoke(false);
                return;
            }

            // If the error is null, the consent information state was updated.
            // You are now ready to check if a form is available.
            if (ConsentInformation.ConsentStatus == ConsentStatus.Required)
            {
                ConsentForm.Load((form, error) =>
                {
                    if (error != null)
                    {
                        Debug.LogError("FalconUMP > OnConsentInfoUpdated === ConsentForm load error: " + error);
                        _onConsentValidated?.Invoke(false);
                        return;
                    }

                    form.Show(formError =>
                    {
                        if (formError != null)
                        {
                            Debug.LogError("FalconUMP > OnConsentInfoUpdated === ConsentForm load error: " + formError);
                            _onConsentValidated?.Invoke(false);
                            return;
                        }

                        _onConsentValidated?.Invoke(true);
                        IsChooseCMP = true;
                    });
                });
            }
            else
            {
                Debug.Log("FalconUMP > OnConsentInfoUpdated === ConsentStatus: " + ConsentInformation.ConsentStatus);
                _onConsentValidated?.Invoke(true);
            }
        }

        /// <summary>
        /// Check if it's necessary to show the Privacy Options Form
        /// </summary>
        public static bool RequirePrivacyOptionsForm => ConsentInformation.PrivacyOptionsRequirementStatus ==
                                                        PrivacyOptionsRequirementStatus.Required;

        /// <summary>
        /// Show Privacy Options form if required
        /// </summary>
        public static void ShowPrivacyOptionsForm(UnityAction actionError)
        {
            ConsentForm.ShowPrivacyOptionsForm((FormError formError) =>
            {
                if (formError != null)
                {
                    Debug.LogError("FalconUMP > ShowPrivacyOptionsForm === Error: " + formError);
                    actionError?.Invoke();
                }
            });
        }
    }
}

