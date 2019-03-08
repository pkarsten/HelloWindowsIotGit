namespace MSGraph.Response
{
    public class ImageResponseInfo
    {
        //private BitmapSource bitmap;
        //public DriveItem DriveItem { get; private set; }


        public int Height
        {
            get;
            set;
        }

        public int Width
        {
            get;
            set;
        }

        //public ImageResponseInfo(DriveItem item)
        //{
        //    this.DriveItem = item;
        //}

        //public BitmapSource Bitmap
        //{
        //    get
        //    {
        //        return this.bitmap;
        //    }
        //    set
        //    {
        //        this.bitmap = value;
        //        OnPropertyChanged("Bitmap");
        //    }
        //}

        //public string Id
        //{
        //    get;
        //    set;
        //}

        //public string Name
        //{
        //    get;
        //    set;
        //}

        ////INotifyPropertyChanged members
        //public event PropertyChangedEventHandler PropertyChanged;

        //protected void OnPropertyChanged(string name)
        //{
        //    if (null != PropertyChanged)
        //    {
        //        PropertyChanged(this, new PropertyChangedEventArgs(name));
        //    }
        //}


    }
}
