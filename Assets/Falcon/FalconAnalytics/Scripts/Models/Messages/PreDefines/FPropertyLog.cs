using System;
using Falcon.FalconAnalytics.Scripts.Models.Attributes;
using Falcon.FalconAnalytics.Scripts.Models.Messages.Abstracts;
using UnityEngine.Scripting;

namespace Falcon.FalconAnalytics.Scripts.Models.Messages.PreDefines
{
    [Serializable]
    public class FPropertyLog : BaseFalconLog
    {
        [FKey(Name = nameof(pName) + "$")]public string pName;
        [FKey(Name = nameof(pValue) + "$")]public string pValue;

        public int priority;

        // ReSharper disable once InconsistentNaming
        [FKey(RemoveIfNull = true)]public int? currentLevel;

        [Preserve]
        public FPropertyLog()
        {
        }

        public FPropertyLog(string pName, string pValue, int priority, int? currentLevel = null)
        {
            LogParams(pName, pValue, priority, currentLevel);
            this.pName = pName;
            this.pValue = pValue;

            this.priority = CheckNumberNonNegative(priority, nameof(priority));
            this.currentLevel = currentLevel;
        }

        public override string Event => "f_sdk_property_data";
    }
}