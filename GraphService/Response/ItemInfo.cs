using System;
using Newtonsoft.Json;

namespace MSGraph.Response
{
    // https://docs.microsoft.com/en-us/graph/api/resources/driveitem?view=graph-rest-1.0
    //DriveItem
    public abstract class DriveItem
    {
        public enum FileFolderKind
        {
            Folder = 0,
            File = 1,
        }

        public enum ItemKind
        {
            Other = 0,
            Image = 1,
            Photo = 2,
            Audio = 3,
            Video = 4,
            Location = 5,
        }

        public DateTime CreatedDateTime
        {
            get;
            set;
        }

        public string CTag
        {
            get;
            set;
        }

        public string ETag
        {
            get;
            set;
        }

        public string Id
        {
            get;
            set;
        }

        public FileFolderKind Kind
        {
            get;
            protected set;
        }

        public DateTime LastModifiedDateTime
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        [JsonIgnore]
        public string ParentId
        {
            get;
            set;
        }

        [JsonProperty("size")]
        public long SizeInBytes
        {
            get;
            set;
        }

        public Uri WebUri => string.IsNullOrEmpty(WebUrl) ? null : new Uri(WebUrl);

        public string WebUrl
        {
            get;
            set;
        }

        public virtual void Populate(ItemInfoResponse info)
        {
            if (!string.IsNullOrEmpty(Id)
                && Id != info.Id)
            {
                throw new ArgumentException("Invalid ID for item", nameof(info));
            }

            CreatedDateTime = info.CreatedDateTime;
            CTag = info.CTag;
            ETag = info.ETag;
            Id = info.Id;
            Kind = info.Kind;
            LastModifiedDateTime = info.LastModifiedDateTime;
            Name = info.Name;
            Description = info.Description;
            SizeInBytes = info.SizeInBytes;
            WebUrl = info.WebUrl;
            
        }
    }
}
