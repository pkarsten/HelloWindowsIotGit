using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Search;

namespace HelloWindowsIot
{
    public class CommonFolderQueryObject
    {
        public string ReadableFilterGroupName { get; set; }
        public CommonFolderQuery CommonFolderEnum { get; set; }
    }

    public class VirtualFolderObject
    {
        public string VirtualFolderName { get; set; }
    }
}
