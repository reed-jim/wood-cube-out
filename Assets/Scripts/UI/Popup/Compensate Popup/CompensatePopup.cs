using System;
using System.Linq;
using TMPro;
using Unity.Android.Gradle;
using UnityEngine;
using UnityEngine.UI;

public class CompensatePopup : BasePopup
{
    [SerializeField] private TMP_InputField codeInput;
    [SerializeField] private TMP_Text idText;

    [SerializeField] private Button redeemButton;

    public static event Action<int, int[], bool> showResourcesEarnPopupEvent;
    public static event Action<string, string> showNotificationEvent;

    protected override void MoreActionInAwake()
    {
        base.MoreActionInAwake();

        redeemButton.onClick.AddListener(Redeem);

        idText.text = $"ID: {SystemInfo.deviceUniqueIdentifier}";
    }

    private void Redeem()
    {
        if (!GamePersistentVariable.isFirebaseRemoteConfigLoaded)
        {
            showNotificationEvent?.Invoke(GameConstants.WARNING, "Invalid Code");

            return;
        }

        string removeAdCode = RemoteConfigController.GetStringConfig(FirebaseConfig.REMOVE_ADS_CODE, "INVALID");
        string bigRewardCode = RemoteConfigController.GetStringConfig(FirebaseConfig.BIG_REWARD_CODE, "INVALID");

        ValidUniqueDeviceIdentifierContainer validUniqueDeviceIdentifierContainer = default;

        RemoteConfigController.GetJsonConfig(FirebaseConfig.VALID_DEVICE_UNIQUE_IDENTIFIERS, out validUniqueDeviceIdentifierContainer);

        if (!validUniqueDeviceIdentifierContainer.ValidIdentifiers.Contains(SystemInfo.deviceUniqueIdentifier))
        {
            showNotificationEvent?.Invoke(GameConstants.WARNING, "Invalid Code");

            return;
        }

        int coinValue = 0;
        int[] boosterQuantites = new int[3];
        bool isRemoveAd = false;

        if (string.Equals(codeInput.text, removeAdCode, StringComparison.OrdinalIgnoreCase))
        {
            isRemoveAd = true;

            showResourcesEarnPopupEvent?.Invoke(coinValue, boosterQuantites, isRemoveAd);
        }

        if (string.Equals(codeInput.text, removeAdCode, StringComparison.OrdinalIgnoreCase))
        {
            coinValue = 9999;
            boosterQuantites[0] = 9;
            boosterQuantites[1] = 9;
            boosterQuantites[2] = 9;

            showResourcesEarnPopupEvent?.Invoke(coinValue, boosterQuantites, true);
        }
    }

    [System.Serializable]
    public class ValidUniqueDeviceIdentifierContainer
    {
        private string[] validIdentifiers;

        public string[] ValidIdentifiers
        {
            get => validIdentifiers;
        }
    }
}
