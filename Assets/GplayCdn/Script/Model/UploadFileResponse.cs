using System;
using Newtonsoft.Json;

namespace GplayCdn.Model
{
    [Serializable]
    public class UploadFileResponse
    {
        [JsonProperty("fileId")] public string FileId { get; set; }
    }
}
