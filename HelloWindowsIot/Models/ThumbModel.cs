using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.FileProperties;

namespace HelloWindowsIot
{
    public class ThumbModel
    {
        public StorageItemThumbnail Thumbnail { get; set; }
        public string Name { get; set; }
    }
}
