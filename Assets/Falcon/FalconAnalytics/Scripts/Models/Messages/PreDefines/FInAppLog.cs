using System;
using Falcon.FalconAnalytics.Scripts.Models.Attributes;
using Falcon.FalconAnalytics.Scripts.Models.Messages.Abstracts;
using Falcon.FalconCore.Scripts.Repositories.News;
using UnityEngine;
using UnityEngine.Scripting;

namespace Falcon.FalconAnalytics.Scripts.Models.Messages.PreDefines
{
    [Serializable]
    public class FInAppLog : BaseFalconLog
    {
        [FKey(Name = nameof(productId) + "$")] public string productId;
        [FKey(Name = nameof(where) + "$")] public string where;

        public string isoCurrencyCode;
        public string transactionId;
        // ReSharper disable once InconsistentNaming
        public decimal localizedPrice;
        
        [FKey(RemoveIfNull = true)] public string purchaseToken;
        // ReSharper disable once InconsistentNaming
        [FKey(RemoveIfNull = true)] public int? currentLevel;
        [FKey(RemoveIfNull = true)] public string purchaseMethod;

        [Preserve]
        public FInAppLog()
        {
        }

        public FInAppLog(string productId, decimal localizedPrice, string isoCurrencyCode, string where,
            string transactionId, string purchaseToken = null, int? currentLevel = null, string purchaseMethod = null)
        {
            LogParams(productId, localizedPrice, isoCurrencyCode, where, transactionId, purchaseToken, currentLevel);
            if (string.IsNullOrEmpty(isoCurrencyCode))
            {
                Debug.LogError(
                    "Dwh Log invalid field: Null or empty currency code of InAppLog, considering it as USD");
                isoCurrencyCode = "USD";
            }

            this.productId = productId;
            this.where = where;

            this.localizedPrice = CheckNumberNonNegative(localizedPrice, nameof(localizedPrice));
            this.isoCurrencyCode = isoCurrencyCode;
            this.transactionId = transactionId;
            this.purchaseToken = purchaseToken;
            this.currentLevel = currentLevel;
            this.purchaseMethod = purchaseMethod;
            FPlayerInfoRepo.InApp.Update(localizedPrice, isoCurrencyCode);
            var ltv = FPlayerInfoRepo.InApp.InAppLtv;

            inAppLtv = ltv.total;
            maxInApp = ltv.max;
            ltvIsoCurrencyCode = ltv.isoCurrencyCode;
            inAppCount = ++FPlayerInfoRepo.InApp.InAppCount;
            if (inAppCount == 1)
            {
                FPlayerInfoRepo.InApp.FirstInAppLv = FPlayerInfoRepo.MaxPassedLevel;
                FPlayerInfoRepo.InApp.FirstInAppDate = DateTime.Now;
            }
        }

        public override string Event => "f_sdk_in_app_data";
    }
}