using System.Collections.Generic;
using Newtonsoft.Json;
using System.Collections;

namespace MSGraph.Response
{
    //[JsonObject]
    public class ParseChildrenResponse  
    {
        public IList<ItemInfoResponse> Value
        {
            get;
            set;
        }

        [JsonProperty("@odata.nextLink")]
        public string NextLink
        {
            get;
            set;
        }
    }
}
