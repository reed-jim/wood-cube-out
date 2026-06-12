using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Falcon.FalconAnalytics.Scripts.Enum
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LevelStatus
    {
        Fail, Pass, ReplayPass, ReplayFail, Skip
    }
}