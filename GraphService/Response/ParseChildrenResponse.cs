using System.Collections.Generic;
using Newtonsoft.Json;
using System.Collections;

namespace MSGraph.Response
{
    //[JsonObject]
    public class ParseChildrenResponse  //: IEnumerable<ItemInfoResponse>
    {
        public IList<ItemInfoResponse> Value
        {
            get;
            set;
        }

        //public IEnumerator<ItemInfoResponse> GetEnumerator()
        //{
        //    return Value.GetEnumerator();
        //}

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return GetEnumerator();
        //}

        [JsonProperty("@odata.nextLink")]
        public string NextLink
        {
            get;
            set;
        }
    }
}
