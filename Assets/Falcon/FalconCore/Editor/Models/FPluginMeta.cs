using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Serialization;

namespace Falcon.FalconCore.Editor.Models
{
    [Serializable]
    public class FPluginMeta
    {
        [JsonProperty(PropertyName = "require")]
        public Dictionary<string, string> Require = new Dictionary<string, string>();

        [FormerlySerializedAs("Version")]
        public string version;
    }
}