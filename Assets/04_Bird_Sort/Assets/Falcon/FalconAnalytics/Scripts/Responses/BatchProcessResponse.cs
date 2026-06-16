using System;
using System.Collections.Generic;

namespace Falcon.FalconAnalytics.Scripts.Responses
{
    [Serializable]
    public class BatchProcessResponse
    {
        public int successCount;
        public int failCount;
        public List<MessageProcessErrorInfo> errors= new List<MessageProcessErrorInfo>();
    }
}