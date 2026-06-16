using System;
using Newtonsoft.Json;

namespace Falcon.FalconCore.Scripts.Utils
{
    public static class JsonUtil
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            DateFormatString = "yyyy-MM-dd HH:mm:sszzz"
        };

        public static T FromJson<T>(String jsonStr)
        {
            return JsonConvert.DeserializeObject<T>(jsonStr, Settings);
        }
        
        public static T FromJson<T>(String jsonStr, JsonSerializerSettings settings)
        {
            return JsonConvert.DeserializeObject<T>(jsonStr, settings);
        }

        public static string ToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj, Settings);
        }
        
        public static string ToJson(object obj, JsonSerializerSettings settings)
        {
            return JsonConvert.SerializeObject(obj, settings);
        }
    }
}