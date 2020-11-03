using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGraph.Response
{
    // Task Response: https://docs.microsoft.com/en-us/graph/api/outlooktask-get?view=graph-rest-beta
    //e.g. 
    //Task Folders: https://graph.microsoft.com/beta/me/outlook/taskFolders
    //List Tasks in Folder: https://graph.microsoft.com/beta/me/outlook/taskFolders/AQMkADAwATM3ZmYAZS05NzcANS05NzE4LTAwAi0wMAoALgAAA9AbFx3CcYdHmhKEe93jcbkBAEzk4EU4PLJIn8ZZnZVUnYgAAAHppBIAAAA=/tasks
    //Get Task (Einkaufen) https://graph.microsoft.com/beta/me/outlook/tasks('AQMkADAwATM3ZmYAZS05NzcANS05NzE4LTAwAi0wMAoARgAAA9AbFx3CcYdHmhKEe93jcbkHAEzk4EU4PLJIn8ZZnZVUnYgAAAHppBIAAABM5OBFODyySJ-GWZ2VVJ2IAAGzZf5vAAAA')
    public class TaskResponse
    {
        [JsonProperty("subject")]
        public string Subject
        {
            get;
            set;
        }

        [JsonProperty("Id")]
        public string Id { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

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

    public class ParseTaskResponse
    {
        public ObservableCollection<TaskResponse> Value
        {
            get;
            set;
        }
    }
}
