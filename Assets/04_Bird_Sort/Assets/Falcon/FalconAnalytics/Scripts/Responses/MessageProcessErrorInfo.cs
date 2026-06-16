using System;

namespace Falcon.FalconAnalytics.Scripts.Responses
{
    [Serializable]
    public class MessageProcessErrorInfo
    {
        public string exception;
        public string data;
    }
}