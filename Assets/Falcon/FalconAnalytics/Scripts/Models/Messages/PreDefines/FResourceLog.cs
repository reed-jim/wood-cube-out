using System;
using Falcon.FalconAnalytics.Scripts.Enum;
using Falcon.FalconAnalytics.Scripts.Models.Attributes;
using Falcon.FalconAnalytics.Scripts.Models.Messages.Abstracts;
using UnityEngine.Scripting;

namespace Falcon.FalconAnalytics.Scripts.Models.Messages.PreDefines
{
    [Serializable]
    public class FResourceLog : BaseFalconLog
    {
        [FKey(Name = nameof(flowType) + "$")] public FlowType flowType;
        [FKey(Name = nameof(itemType) + "$")] public string itemType;
        [FKey(Name = nameof(currency) + "$")] public string currency;

        public string itemId;
        public long amount;

        // ReSharper disable once InconsistentNaming
        [FKey(RemoveIfNull = true)] public int? currentLevel;

        [Preserve]
        public FResourceLog()
        {
        }

        public FResourceLog(FlowType flowType, string itemType, string currency, string itemId, long amount,
            int? currentLevel = null)
        {
            LogParams(flowType, itemType, currency, itemId, amount, currency);
            this.flowType = flowType;
            this.itemType = itemType;
            this.currency = currency;

            this.itemId = itemId;
            this.amount = CheckNumberNonNegative(amount, nameof(amount));
            this.currentLevel = currentLevel;
        }

        public override string Event => "f_sdk_resource_data";
    }
}