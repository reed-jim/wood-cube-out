using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CompensatePopup : BasePopup
{
    [SerializeField] private TMP_InputField codeInput;
    [SerializeField] private TMP_Text idText;

    [SerializeField] private Button redeemButton;

    [SerializeField] private UserResourcesObserver userResourcesObserver;

    public static event Action<int, int[], bool> showResourcesEarnPopupEvent;
    public static event Action<string, string> showNotificationEvent;

    protected override void MoreActionInAwake()
    {
        base.MoreActionInAwake();

        redeemButton.onClick.AddListener(Redeem);
    }

    protected override void Show(Action onCompletedAction = null)
    {
        idText.text = $"ID: {GamePersistentVariable.FirebaseUserId}";

        base.Show(onCompletedAction);
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

        ValidUniqueDeviceIdentifierContainer validUniqueDeviceIdentifierContainer = new ValidUniqueDeviceIdentifierContainer();

        RemoteConfigController.GetJsonConfig(FirebaseConfig.VALID_DEVICE_UNIQUE_IDENTIFIERS, out validUniqueDeviceIdentifierContainer);

        if (validUniqueDeviceIdentifierContainer.validIdentifiers == null)
        {
            showNotificationEvent?.Invoke(GameConstants.WARNING, "Invalid Code");

            return;
        }

        if (!validUniqueDeviceIdentifierContainer.validIdentifiers.Contains(GamePersistentVariable.FirebaseUserId))
        {
            showNotificationEvent?.Invoke(GameConstants.WARNING, "Invalid Code");

            return;
        }

        int coinValue = 0;
        int[] boosterQuantites = new int[3];
        bool isRemoveAd = false;

        bool isClaimCompensateRemoveAd = DataUtility.Load(GameConstants.IS_CLAIMED_COMPENSATE_REMOVE_ADS, false);
        bool isClaimCompensateBigReward = DataUtility.Load(GameConstants.IS_CLAIMED_COMPENSATE_BIG_REWARD, false);

        if (string.Equals(codeInput.text, removeAdCode, StringComparison.OrdinalIgnoreCase))
        {
            if (!isClaimCompensateRemoveAd)
            {
                isRemoveAd = true;

                AdmobAdsMax.Instance.HideBanner();
                AdmobAdsMax.Instance.DestroyBanner();

                UserData.IsRemoveAds = true;

                Hide();

                showResourcesEarnPopupEvent?.Invoke(coinValue, boosterQuantites, isRemoveAd);

                DataUtility.Save(GameConstants.IS_CLAIMED_COMPENSATE_REMOVE_ADS, true);

                SaferioTracking.TrackClaimCompensateReward(removeAdCode, GamePersistentVariable.FirebaseUserId);
            }
            else
            {
                showNotificationEvent?.Invoke(GameConstants.WARNING, "The code has already been claimed");
            }
        }
        else if (string.Equals(codeInput.text, bigRewardCode, StringComparison.OrdinalIgnoreCase))
        {
            if (!isClaimCompensateBigReward)
            {
                coinValue = 9999;
                boosterQuantites[0] = 9;
                boosterQuantites[1] = 9;
                boosterQuantites[2] = 9;

                userResourcesObserver.ChangeBoosterQuantity(0, boosterQuantites[0]);
                userResourcesObserver.ChangeBoosterQuantity(1, boosterQuantites[1]);
                userResourcesObserver.ChangeBoosterQuantity(2, boosterQuantites[2]);

                userResourcesObserver.ChangeCoin(coinValue);

                Hide();

                showResourcesEarnPopupEvent?.Invoke(coinValue, boosterQuantites, isRemoveAd);

                DataUtility.Save(GameConstants.IS_CLAIMED_COMPENSATE_BIG_REWARD, true);

                SaferioTracking.TrackClaimCompensateReward(bigRewardCode, GamePersistentVariable.FirebaseUserId);
            }
            else
            {
                showNotificationEvent?.Invoke(GameConstants.WARNING, "The code has already been claimed");
            }
        }
        else
        {
            showNotificationEvent?.Invoke(GameConstants.WARNING, "Invalid Code");
        }
    }

    [System.Serializable]
    public class ValidUniqueDeviceIdentifierContainer
    {
        public string[] validIdentifiers;
    }
}
