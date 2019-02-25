using Newtonsoft.Json;

namespace MSGraph.Request
{
    public class RequestLinkInfo
    {
        [JsonProperty("type")]
        public string Type
        {
            get;
            set;
        }
    }
}
