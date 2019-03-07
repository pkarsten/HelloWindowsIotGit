﻿
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml.Media.Imaging;

namespace MSGraph.Response
{
    public class ImageResponseInfo : INotifyPropertyChanged
    {
        private BitmapSource bitmap;
        public DriveItem DriveItem { get; private set; }


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

        public ImageResponseInfo(DriveItem item)
        {
            this.DriveItem = item;
        }

        public BitmapSource Bitmap
        {
            get
            {
                return this.bitmap;
            }
            set
            {
                this.bitmap = value;
                OnPropertyChanged("Bitmap");
            }
        }

        public string Id
        {
            get
            {
                return this.DriveItem == null ? null : this.DriveItem.Id;
            }
        }

        public string Name
        {
            get
            {
                return this.DriveItem.Name;
            }
        }

        //INotifyPropertyChanged members
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }


    }
}
