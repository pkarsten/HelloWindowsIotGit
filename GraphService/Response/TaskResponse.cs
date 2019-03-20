using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGraph.Response
{
    // Task Response: https://docs.microsoft.com/en-us/graph/api/outlooktask-get?view=graph-rest-beta
    public class TaskResponse
    {
        [JsonProperty("subject")]
        public string Subject
        {
            get;
            set;
        }

        [JsonProperty("body")]
        public TaskBodyResponse TaskBody
        {
            get;
            set;
        }
    }

    public class TaskBodyResponse
    {
        [JsonProperty("contentType")]
        public string ContentType
        {
            get;
            set;
        }

        [JsonProperty("content")]
        public string Content
        {
            get;
            set;
        }
    }
}
