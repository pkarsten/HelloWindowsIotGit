using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using RWPBGTasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Core;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.System;
using System.Windows.Input;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Search;
using Windows.UI.Xaml.Media;

namespace HelloWindowsIot
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class Dashboard : Page
    {
        //Prepare image to display on dashboard
        BitmapImage CurrBitmapImage = new BitmapImage();

        public Dashboard()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Dal.SaveLogEntry(LogType.Info, "Navigated To DashBoard");
            await GetThumbnailFromCurrentWallpaper();
            if (PicDBContainsPictures() == false) { }

        }

        #region functions
        private async Task GetThumbnailFromCurrentWallpaper()
        {
            try
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                var filenames = Directory.GetFiles(localFolder.Path).Select(f => Path.GetFileName(f)).Where(fn => fn.StartsWith("mybgpicture"));
                if (filenames != null)
                {
                    var mybgpicfile = filenames.FirstOrDefault();
                    if (mybgpicfile != null)
                    {
                        StorageFile mybgpicturefile = await localFolder.GetFileAsync(mybgpicfile.ToString());
                        Dal.SaveLogEntry(LogType.Info, mybgpicturefile.Path);

                        // Get image thumbnails!!
                        // Thumbnail = await mybgpicture.GetThumbnailAsync(ThumbnailMode.SingleItem);
                        using (StorageItemThumbnail thumbnail = await mybgpicturefile.GetThumbnailAsync(ThumbnailMode.SingleItem))
                        {
                            if (thumbnail != null)
                            {


                                CurrBitmapImage.SetSource(thumbnail);
                                dashBoardImage.Source = CurrBitmapImage;
                            }
                        }
                    }
                    else
                    {
                        //txtThumbInfo.Visibility = Visibility.Visible;
                        //txtThumbInfo.Text = AppcFuncs.GetLanguage("ImageNotSetByApp");
                    }
                }

            }
            catch (Exception ex)
            {
                Dal.SaveLogEntry(LogType.Exception, "Exception in GetThumbnailFromCurrentWallpaper in DashBoard" + ex.Message);
            }
        }

        private bool PicDBContainsPictures()
        {
            bool b = true;
            //Can't Launch ChangeWallpaper if :
            // Favorite Pics in DB are not 0 
            if (Dal.GetAllPictures().Count == 0)
            {
                return false;
            }

            return b;
        }

        #endregion
    }
}
