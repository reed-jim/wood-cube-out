using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Falcon.FalconAnalytics.Scripts.Enum
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum FlowType
    {
        Source, Sink
    }
}