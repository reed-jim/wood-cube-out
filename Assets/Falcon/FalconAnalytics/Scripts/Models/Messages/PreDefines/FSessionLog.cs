using System;
using Falcon.FalconAnalytics.Scripts.Models.Attributes;
using Falcon.FalconAnalytics.Scripts.Models.Messages.Abstracts;
using Falcon.FalconCore.Scripts.Repositories;
using UnityEngine.Scripting;

namespace Falcon.FalconAnalytics.Scripts.Models.Messages.PreDefines
{
    [Serializable]
    public class FSessionLog : BaseFalconLog
    {
        [FKey(Name = nameof(gameMode) + "$")] public string gameMode;
        
        public int sessionTime;
        public long modeTotalTime;

        // ReSharper disable once InconsistentNaming
        [FKey(RemoveIfNull = true)] public int? currentLevel;

        [Preserve]
        public FSessionLog()
        {
        }

        public FSessionLog(string gameMode, TimeSpan sessionTime, int? currentLevel = null)
        {
            LogParams(gameMode, sessionTime, currentLevel);
            this.gameMode = gameMode;
            this.sessionTime = (int)sessionTime.TotalSeconds;
            this.currentLevel = currentLevel;
            modeTotalTime = FDataPool.Instance.Compute<long>("Session_Total_" + gameMode, (hasKey, val) =>
            {
                if (!hasKey) val = 0;
                return val + (long)sessionTime.TotalSeconds;
            });
        }

        public override string Event => "f_sdk_session_data";
    }
}