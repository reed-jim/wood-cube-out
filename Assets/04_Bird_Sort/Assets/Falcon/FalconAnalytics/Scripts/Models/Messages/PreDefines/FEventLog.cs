using System;
using System.Collections.Generic;
using Falcon.FalconAnalytics.Scripts.Models.Attributes;
using Falcon.FalconAnalytics.Scripts.Models.Messages.Abstracts;
using Falcon.FalconCore.Scripts.Utils;
using UnityEngine.Scripting;

namespace Falcon.FalconAnalytics.Scripts.Models.Messages.PreDefines
{
    [Serializable]
    public class FEventLog : BaseFalconLog
    {
        [FKey(Name = nameof(eventName) + "$")] public string eventName;

        public string paramsStr;

        // ReSharper disable once InconsistentNaming
        [FKey(RemoveIfNull = true)] public int? currentLevel;

        [Preserve]
        public FEventLog()
        {
        }

        public FEventLog(string eventName, Dictionary<string, object> param = null, int? currentLevel = null)
        {
            LogParams(eventName, param, currentLevel);
            this.eventName = eventName;
            if (param == null || param.Count == 0)
                paramsStr = null;
            else
                paramsStr = JsonUtil.ToJson(param);
            this.currentLevel = currentLevel;
        }

        public override string Event => "f_sdk_event_data";
    }
}