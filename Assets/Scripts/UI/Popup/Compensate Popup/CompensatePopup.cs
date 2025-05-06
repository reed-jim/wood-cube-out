using System;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CompensatePopup : BasePopup
{
    [SerializeField] private TMP_InputField codeInput;
    [SerializeField] private TMP_Text idText;

    [SerializeField] private Button redeemButton;

    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private UserResourcesObserver userResourcesObserver;

    public static event Action updateLevelEvent;
    public static event Action updateCoinTextEvent;

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
            ShowNotification(GameConstants.WARNING, "Invalid Code");

            return;
        }

        string removeAdCode = RemoteConfigController.GetStringConfig(FirebaseConfig.REMOVE_ADS_CODE, "INVALID");
        string bigRewardCode = RemoteConfigController.GetStringConfig(FirebaseConfig.BIG_REWARD_CODE, "INVALID");

        ValidUniqueDeviceIdentifierContainer validUniqueDeviceIdentifierContainer = new ValidUniqueDeviceIdentifierContainer();

        RemoteConfigController.GetJsonConfig(FirebaseConfig.VALID_DEVICE_UNIQUE_IDENTIFIERS, out validUniqueDeviceIdentifierContainer);

        if (validUniqueDeviceIdentifierContainer.validIdentifiers == null)
        {
            ShowNotification(GameConstants.WARNING, "Invalid Code");

            return;
        }

        if (!validUniqueDeviceIdentifierContainer.validIdentifiers.Contains(GamePersistentVariable.FirebaseUserId))
        {
            ShowNotification(GameConstants.WARNING, "Invalid Code");

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

                InvokeShowResourcesEarnPopupPopupEvent(coinValue, boosterQuantites, isRemoveAd);

                DataUtility.Save(GameConstants.IS_CLAIMED_COMPENSATE_REMOVE_ADS, true);

                SaferioTracking.TrackClaimCompensateReward(removeAdCode, GamePersistentVariable.FirebaseUserId);
            }
            else
            {
                ShowNotification(GameConstants.WARNING, "The code has already been claimed");
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

                InvokeShowResourcesEarnPopupPopupEvent(coinValue, boosterQuantites, isRemoveAd);

                DataUtility.Save(GameConstants.IS_CLAIMED_COMPENSATE_BIG_REWARD, true);

                SaferioTracking.TrackClaimCompensateReward(bigRewardCode, GamePersistentVariable.FirebaseUserId);
            }
            else
            {
                ShowNotification(GameConstants.WARNING, "The code has already been claimed");
            }
        }
        else if (codeInput.text.ContainsInsensitive("gotolevel"))
        {
            string pattern = @"\d+";

            Match match = Regex.Match(codeInput.text, pattern);

            if (match.Success)
            {
                int level = int.Parse(match.Value);

                currentLevel.Value = level;
                currentLevel.Save();

                updateLevelEvent?.Invoke();
            }
        }
        else if (codeInput.text.ContainsInsensitive("addcoin"))
        {
            int coinQuantity = ExtractIntFromString(codeInput.text);

            userResourcesObserver.UserResources.CoinQuantity += coinQuantity;
            userResourcesObserver.Save();

            if (coinQuantity < 39999)
            {
                updateCoinTextEvent?.Invoke();
            }
            else
            {
                ShowNotification(GameConstants.WARNING, "Invalid Code");
            }
        }
        else if (codeInput.text.ContainsInsensitive("unlockallscrewboxes") && !GamePersistentVariable.isUnlockedAllScrewBoxes)
        {
            DataUtility.Save(GameConstants.IS_UNLOCKED_ALL_SCREW_BOXES, true);

            GamePersistentVariable.isUnlockedAllScrewBoxes = true;

            ShowNotification("Booster", "Unlock All Screw Boxes Permanently");
        }
        else
        {
            ShowNotification(GameConstants.WARNING, "Invalid Code");
        }
    }

    #region UTIL
    private int ExtractIntFromString(string input)
    {
        string pattern = @"\d+";

        Match match = Regex.Match(input, pattern);

        if (match.Success)
        {
            return int.Parse(match.Value);
        }

        return 0;
    }
    #endregion

    [System.Serializable]
    public class ValidUniqueDeviceIdentifierContainer
    {
        public string[] validIdentifiers;
    }
}
