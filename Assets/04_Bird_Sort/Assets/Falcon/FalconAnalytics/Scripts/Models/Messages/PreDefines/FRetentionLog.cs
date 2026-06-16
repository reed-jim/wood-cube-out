using System;
using Falcon.FalconAnalytics.Scripts.Models.Messages.Abstracts;
using Falcon.FalconCore.Scripts.Repositories;
using UnityEngine.Scripting;

namespace Falcon.FalconAnalytics.Scripts.Models.Messages.PreDefines
{
    [Serializable]
    public class FRetentionLog : BaseFalconLog
    {
        public string localDate;

        [Preserve]
        public FRetentionLog()
        {
        }

        public FRetentionLog(DateTime localDate)
        {
            LogParams(localDate);
            this.localDate = FTime.DateToString(localDate);
        }

        public override string Event => "f_sdk_retention_data";
    }
}