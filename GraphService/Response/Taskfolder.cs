using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGraph.Response
{
    // Task Folder: https://graph.microsoft.com/beta/me/outlook/taskFolders
    /// <summary>
    /// MS Graph Outlook TaskFolder 
    /// </summary>
    public class TaskFolder
    {
        [JsonProperty("id")]
        public string Id
        {
            get;
            set;
        }

        [JsonProperty("name")]
        public string Name
        {
            get;
            set;
        }
    }

    public class ParseTaskFolderResponse
    {
        public ObservableCollection<TaskFolder> Value
        {
            get;
            set;
        }
    }
}
