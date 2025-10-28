using System.Collections.Generic;
using GoogleMobileAds.Ump.Api;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UMPManager : MonoBehaviour
{
    void Start()
    {
        var debugSettings = new ConsentDebugSettings
        {
            // Geography appears as in EEA for debug devices.
            DebugGeography = DebugGeography.EEA,
            TestDeviceHashedIds = new List<string>
            {
                "EFC077ABD1A90039A1AA9986094B4F5C"
            }
        };

        // Create a ConsentRequestParameters object.
        // ConsentRequestParameters request = new ConsentRequestParameters
        // {
        //     ConsentDebugSettings = debugSettings,
        // };

        ConsentRequestParameters request = new ConsentRequestParameters
        {
            TagForUnderAgeOfConsent = false
        };

        // Check the current consent information status.
        ConsentInformation.Update(request, OnConsentInfoUpdated);
    }

    void OnConsentInfoUpdated(FormError consentError)
    {
        if (consentError != null)
        {
            // Handle the error.
            HandleConsentComplete();

            return;
        }

        // If the error is null, the consent information state was updated.
        // You are now ready to check if a form is available.
        ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) =>
        {
            if (formError != null)
            {
                // Consent gathering failed.
                HandleConsentComplete();

                return;
            }

            // Consent has been gathered.
            HandleConsentComplete();
        });
    }

    private void HandleConsentComplete()
    {
        SceneManager.LoadScene("Loading");
    }
}
