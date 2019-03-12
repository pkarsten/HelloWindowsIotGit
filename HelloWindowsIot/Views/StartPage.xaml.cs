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

    public sealed partial class StartPage : Page
    {

        //Prepare thumbnail to display
        BitmapImage CurrBitmapImage = new BitmapImage();

        public StartPage()
        {
            this.InitializeComponent();
            PageTitle.Text = AppcFuncs.GetLanguage("TitleStartPage");
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Dal.SaveLogEntry(LogType.Info, "Navigated To StatusPage");
            UpdateUI();
            await GetThumbnailFromCurrentWallpaper();
            if (PicDBContainsPictures() == false)
                RefreshStatus(AppcFuncs.GetLanguage("emptyPicDatabase"), NotifyType.ErrorMessage);
        }

        #region UI
        /// <summary>
        /// Update the scenario UI.
        /// </summary>
        private async void UpdateUI()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
               SetFilterAndThumbInfos();
            });
        }
        /// <summary>
        /// refresh Status Block  
        /// </summary>
        /// <param name="strMessage"></param>
        /// <param name="type"></param>
        private async void RefreshStatus(string strMessage, NotifyType type)
        {
            switch (type)
            {
                case NotifyType.StatusMessage:
                    StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Green);
                    break;
                case NotifyType.ErrorMessage:
                    StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                    break;
            }
            StatusBlock.Text = strMessage;

            // Collapse the StatusBlock if it has no text to conserve real estate.
            StatusBorder.Visibility = (StatusBlock.Text != String.Empty) ? Visibility.Visible : Visibility.Collapsed;
            if (StatusBlock.Text != String.Empty)
            {
                StatusBorder.Visibility = Visibility.Visible;
            }
            else
            {
                StatusBorder.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Change Wallpaper eventhandler
        private async void RunChangeWPTask_Click(object sender, RoutedEventArgs e)
        {
            if (PicDBContainsPictures() == true)
                await LaunchChangeWallpaper();
            else
                RefreshStatus(AppcFuncs.GetLanguage("emptyPicDatabase"),NotifyType.ErrorMessage);
            UpdateUI();
        }

        private async Task LaunchChangeWallpaper()
        {
            await TaskFunctions.ChangeWallpaperAsync(false);
            await GetThumbnailFromCurrentWallpaper();
        }

        private async void GoToBGPicFolder_Click(object sender, RoutedEventArgs e)
        {
            FavoritePic p = Dal.GetCurrentBGPic();
            if (p != null)
            {
                //TODO: Open File in Folder 
                string sPath = p.RelativePath;
                StorageLibrary myPicturesLib = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Pictures);
                IObservableVector<Windows.Storage.StorageFolder> myPictureLibFolders = myPicturesLib.Folders;

                foreach (var fold in myPictureLibFolders)
                {
                    if (fold.Path == p.LibraryPath)
                    {
                        
                        StorageFile stf = await fold.GetFileAsync(p.RelativePath);
                        var directory = stf.Path.Replace("\\" + p.Name, "");
                        Dal.SaveLogEntry(LogType.Info, "Directory : " + directory);
                        StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(directory);
                        LauncherOptions o = new LauncherOptions();
                        FolderLauncherOptions fo = new FolderLauncherOptions();
                        fo.ItemsToSelect.Add(stf);
                        await Launcher.LaunchFolderAsync(folder, fo);
                    }
                }
            }

        }

        #endregion

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
                                bgThumbImage.Source = CurrBitmapImage;
                            }
                        }
                    }
                    else
                    {
                        txtThumbInfo.Visibility = Visibility.Visible;
                        txtThumbInfo.Text = AppcFuncs.GetLanguage("ImageNotSetByApp");
                    }
                }

            }
            catch(Exception ex)
            {
                Dal.SaveLogEntry(LogType.Exception, "Exception in GetThumbnailFromCurrentWallpaper " + ex.Message);
            }
        }

        private async Task SetFilterAndThumbInfos()
        {
            try
            {
                PicFilter p = Dal.GetPicFilter();
                txtPicFilterTitleInfo.Text = AppcFuncs.GetLanguage("Filterproperties");
                if (!String.IsNullOrEmpty(p.CommonFolderQuery))
                {
                    CommonFolderQuery cfq = (CommonFolderQuery)Enum.Parse(typeof(CommonFolderQuery), p.CommonFolderQuery);
                    txtFilterGroup.Text = cfq.ReadableName();
                }
                if (!String.IsNullOrEmpty(p.VirtualFolder))
                    txtFilteVirtualFolder.Text = AppcFuncs.GetLanguage("AdditionalAttribute") + ": " + p.VirtualFolder;
                txtCountPics.Text = AppcFuncs.GetLanguage("NumberOfPicsSaved") + ": " + Dal.GetAllPictures().Count;
                currentBGPic.Text = AppcFuncs.GetLanguage("CurrentWallPaperPic") + ":";
                if (CurrBitmapImage == null)
                {
                    txtThumbInfo.Visibility = Visibility.Visible;
                    txtThumbInfo.Text = AppcFuncs.GetLanguage("ImageNotSetByApp");
                }
                else
                {
                    txtThumbInfo.Visibility = Visibility.Collapsed;
                    txtThumbInfo.Text = "";
                }
                if (Dal.GetCurrentBGPic() != null)
                {
                    gotopictext.Visibility = Visibility.Visible;
                    PicTaskGoToBGPicFolder.Visibility = Visibility.Visible;
                } else
                {
                    gotopictext.Visibility = Visibility.Collapsed;
                    PicTaskGoToBGPicFolder.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Dal.SaveLogEntry(LogType.Error, "Exception  in SetFilterAndThumbInfos()" + ex.Message);
            }
        }

        private bool PicDBContainsPictures()
        {
            bool b = true;
            //Can Launch ChangeWallpaper if :
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
