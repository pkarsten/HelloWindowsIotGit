using HelloWindowsIot.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media.Imaging;

namespace HelloWindowsIot
{
    public static class Settings
    {
        #region UI
        public const string APP_NAME = "Hello WIndows IOT";
        public const string ProductIdinStore = "";
        public const string SupportEmail = "pkarsten@live.de";
        public const string SupporterFirstName = "Peter";
        public static bool LoadPictureListManually { get; set; }
        #endregion

    }

}

