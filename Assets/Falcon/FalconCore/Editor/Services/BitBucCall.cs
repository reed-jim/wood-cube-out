using System.Collections.Generic;
using System.Net.Http;
using Falcon.FalconCore.Editor.Payloads;
using Falcon.FalconCore.Scripts.Utils;
using Falcon.FalconCore.Scripts.Utils.FActions.Variances.Starts;

namespace Falcon.FalconCore.Editor.Services
{
    public static class BitBucCall
    {
        public static List<BitBucObj> OfUrl(string url)
        {
            List<BitBucObj> result = new List<BitBucObj>();
            
            var currentUrl = url;

            while (!string.IsNullOrEmpty(currentUrl))
            {
                BitBucResponse response = JsonUtil.FromJson<BitBucResponse>(new HttpRequest
                {
                    RequestType = HttpMethod.Get,
                    URL = currentUrl
                }.InvokeAndGet());
                result.AddRange(response.Values);

                currentUrl = response.Next;
            }

            return result;
        }
    }
}