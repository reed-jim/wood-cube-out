using System;
using Falcon.FalconAnalytics.Scripts.Enum;
using Falcon.FalconAnalytics.Scripts.Models.Attributes;
using Falcon.FalconAnalytics.Scripts.Models.Messages.Abstracts;
using Falcon.FalconCore.Scripts.Repositories.News;
using UnityEngine.Scripting;

namespace Falcon.FalconAnalytics.Scripts.Models.Messages.PreDefines
{
    [Serializable]
    public class FAdLog : BaseFalconLog
    {
        [FKey(Name = nameof(type) + "$")] public AdType type;
        [FKey(Name = nameof(adWhere) + "$")] public string adWhere;
        
        public int typeCount;

        [FKey(RemoveIfNull = true)] public string adPrecision;
        [FKey(RemoveIfNull = true)] public string adCountry;
        [FKey(RemoveIfNull = true)] public string adNetwork;
        [FKey(RemoveIfNull = true)] public string adMediation;
        
        // ReSharper disable once InconsistentNaming
        [FKey(RemoveIfNull = true)] public double? adRev;
        // ReSharper disable once InconsistentNaming
        [FKey(RemoveIfNull = true)] public int? currentLevel;

        [Preserve]
        public FAdLog()
        {
        }

        public FAdLog(AdType type, string adWhere, int? currentLevel = null, double? adLtv = null)
        {
            LogParams(type, adWhere, currentLevel, adLtv);
            this.type = type;
            this.adWhere = adWhere;
            this.currentLevel = currentLevel;

            if (adLtv.HasValue)
            {
                this.adLtv = adLtv;
                FPlayerInfoRepo.Ad.AdLtv = adLtv.Value;
            }

            typeCount = FPlayerInfoRepo.Ad.IncrementAdCount(type);
            if (type == AdType.Interstitial || type == AdType.Reward) adCount++;
        }

        public FAdLog(AdType type, string adWhere, string adPrecision, string adCountry, double adRev,
            string adNetwork, string adMediation, int? currentLevel = null, double? adLtv = null) : this(type, adWhere,
            currentLevel, adLtv)
        {
            this.adPrecision = adPrecision;
            this.adCountry = adCountry;
            this.adRev = CheckNumberNonNegative(adRev, nameof(adRev));
            this.adNetwork = adNetwork;
            this.adMediation = adMediation;
        }

        public override string Event => "f_sdk_ads_data";
    }
}