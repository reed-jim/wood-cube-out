using System;
using System.Collections.Generic;
using System.Net;

namespace Falcon.FalconCore.Editor.Models
{
    [Serializable]
    public class FKeyValidateResponse
    {
        public HttpStatusCode code;
        public string message;
        public Dictionary<string, object> data;
    }
}