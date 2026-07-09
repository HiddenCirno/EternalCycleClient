using Newtonsoft.Json;
using System.Collections.Generic;

namespace EternalCycleClient
{
    public class SyncResourceRequest
    {
        [JsonProperty("clientHashes")]
        public Dictionary<string, string> ClientHashes { get; set; } = new Dictionary<string, string>();
    }

    public class SyncResourceResponse
    {
        [JsonProperty("validFiles")]
        public List<string> ValidFiles { get; set; } = new List<string>();

        [JsonProperty("filesToUpdate")]
        public Dictionary<string, string> FilesToUpdate { get; set; } = new Dictionary<string, string>();
    }
}