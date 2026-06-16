using System.Collections.Generic;
using Newtonsoft.Json;

namespace Falcon.FalconAnalytics.Scripts.Models.Messages.Interfaces
{
    public interface IDataLog
    {
        [JsonIgnore]
        long CreatedTime { get; }
        [JsonIgnore]
        string Event { get; }
        
        Dictionary<string, object> ToDictionary();
    }
}