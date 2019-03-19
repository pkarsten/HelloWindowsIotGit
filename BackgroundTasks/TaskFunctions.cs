using AppSettings;
using MSGraph;
using MSGraph.Response;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UwpSqliteDal;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.System.UserProfile;
using Windows.UI.Xaml.Media.Imaging;

namespace RWPBGTasks
{
    public static class TaskFunctions
    {
        #region Change WallPaper
        //Needed for if we wantr call a function by BackGround Task or Manually from App GUI? 

        // Because WinRt can use, but can't return Task<T> h ttp://dotnetbyexample.blogspot.de/2014/11/returning-task-from-windows-runtime.html
        /// <summary>
        /// 
        /// </summary>
        /// <param name="CallFromBackgroundTask">Define if Function was called from Background Task or Startet by Manual Button Click in UI </param>
        /// <returns></returns>
        public static IAsyncOperation<bool> ChangeWallpaperAsync(bool CallFromBackgroundTask)
        {
            return InternalChangeWallpaperAsync(CallFromBackgroundTask).AsAsyncOperation();
        }
        private static async Task<bool> InternalChangeWallpaperAsync(bool CallFromBackgroundTask)
        {
            try
            {
              /*if (CallFromBackgroundTask)
                    Dal.SaveLogEntry(LogType.Info, "Try Change Wallpaper");
                else
                    Dal.SaveLogEntry(LogType.Info, "Try Change Wallpaper manually");

                FavoritePic fp = await Dal.GetRandomPicture();
                //FavoritePic fp = Dal.GetPictureById(243);
                StorageFile newFile = null;
                if (fp != null)
                {
                    StorageLibrary myPicturesLib = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Pictures);
                    IObservableVector<Windows.Storage.StorageFolder> myPictureLibFolders = myPicturesLib.Folders;

                    foreach (var fold in myPictureLibFolders)
                    {
                        if (fold.Path == fp.LibraryPath)
                        {
                            StorageFile stf = await GetFileAsync(fold, fp.RelativePath);
                            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                            Dal.SaveLogEntry(LogType.Info, "Selected Pic: " + fp.LibraryPath + "\\"+fp.RelativePath);
                            newFile = await stf.CopyAsync(ApplicationData.Current.LocalFolder, Configuration.PicFileNameInAppDataFolder + GetFileExtension(stf.Name), NameCollisionOption.ReplaceExisting);

                        }
                    }
                    if (await SetWallpaperAsync(newFile) == true)
                    {
                        Dal.SaveLogEntry(LogType.AppInfo, "Wallpaper Changed");
                        fp.Viewed = true;
                        Dal.ResetIsCurrentWallpaper();
                        fp.IsCurrentWallPaper = true;
                        Dal.SavePicture(fp);
                    }
                    else
                    {
                        Dal.SaveLogEntry(LogType.Error, "Something Wrong - SetWallpaperAsync returns false");
                    }
                }
                else
                {
                    Dal.SaveLogEntry(LogType.Error, "GetRandomPicture() is null in ChangeWallpaperAsync()");
                }
                */
            }
            catch (Exception ex)
            {
                Dal.SaveLogEntry(LogType.Error, "Exception  in InternalChangeWallpaperAsync() " + ex.Message);
            }
            finally
            {
                Dal.CheckForViewedPictures();
            }
            return true;
        }

        private static string GetFileExtension(string name)
        {
            string result = "";
            try
            {
                result = name.Substring(name.LastIndexOf('.')); //e.g. name="hallo.jpg" returns ".jpg", LastIndexOf+1 returns jpg without .
            }
            catch (Exception ex)
            {
                Dal.SaveLogEntry(LogType.Error, "Exception  in GetFileExtension()" + ex.Message);
            }
            return result;
        }

        private static async Task<StorageFile> GetFileAsync(StorageFolder folder, string filename)
        {
            StorageFile file = null;
            try
            {
                if (folder != null)
                {
                    file = await folder.GetFileAsync(filename);
                }
            }
            catch (Exception ex)
            {
                Dal.SaveLogEntry(LogType.Error, "Exception  in GetFileAsync()" + ex.Message);
            }
            return file;
        }

        /// <summary>
        /// Pass in a relative path to a file inside the local appdata folder 
        /// </summary>
        /// <param name="fileItem"></param>
        /// <returns></returns>
        private static async Task<bool> SetWallpaperAsync(StorageFile fileItem)
        {
            bool success = false;
            try
            {
                if (UserProfilePersonalizationSettings.IsSupported())
                {
                    UserProfilePersonalizationSettings profileSettings = UserProfilePersonalizationSettings.Current;
                    success = await profileSettings.TrySetWallpaperImageAsync(fileItem);
                }
                else
                {
                    Dal.SaveLogEntry(LogType.Error, "UserProfilePersonalizationSettings is NOT Supported ");
                }
            }
            catch (Exception ex)
            {
                Dal.SaveLogEntry(LogType.Error, "Exception  in SetWallpaperAsync()" + ex.Message);

            }
            return success;
        }
        #endregion

        #region Change Dashboard Desktop Pic
        // Because WinRt can use, but can't return Task<T> h ttp://dotnetbyexample.blogspot.de/2014/11/returning-task-from-windows-runtime.html
        /// <summary>
        /// 
        /// </summary>
        /// <param name="CallFromBackgroundTask">Define if Function was called from Background Task or Startet by Manual Button Click in UI </param>
        /// <returns></returns>
        public static IAsyncOperation<bool> ChangeDashBoardBackGroundAsync(bool CallFromBackgroundTask)
        {
            return InternalChangeDashBoardBackGroundAsync(CallFromBackgroundTask).AsAsyncOperation();
        }
        private static async Task<bool> InternalChangeDashBoardBackGroundAsync(bool CallFromBackgroundTask)
        {
            try
            {
                if (Dal.GetAllPictures().Count == 0)
                {
                    await Dal.LoadImagesFromOneDriveInDBTable();
                } 
                // Get Random ItemInfoResponse from Table 
                var item = Dal.GetRandomInfoItemResponse();

                BitmapImage bitmapimage = new BitmapImage();

                // Only load a detail view image for image items. Initialize the bitmap from the image content stream.
                Exception error = null;
                ItemInfoResponse foundFile = null;
                Stream contentStream = null;

                //ShowBusy(true);
                //// Initialize Graph client
                var accessToken = await GraphService.GetTokenForUserAsync();
                var graphService = new GraphService(accessToken);
                try
                {
                    foundFile = await graphService.GetItem(item.OneDriveId);

                    if (foundFile == null)
                    {
                        Dal.SaveLogEntry(LogType.Error, $"Image Not found Id: {item.OneDriveId}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Found Image: " + item.Name + "Id: " + item.OneDriveId + item.DownloadUrl);

                    }

                    // Get the file's content
                    contentStream = await graphService.RefreshAndDownloadContent(foundFile, false);

                    if (contentStream == null)
                    {
                        Dal.SaveLogEntry(LogType.Error, $"Content not found: {foundFile.Name}");
                    }
                }
                catch (Exception ex)
                {
                    error = ex;
                }

                if (error != null)
                {
                    Dal.SaveLogEntry(LogType.Error, error.Message);
                }

                // Save the retrieved stream 
                var memoryStream = contentStream as MemoryStream;

                if (memoryStream != null)
                {
                    //if (item.Image == null)
                    //{
                    //    System.Diagnostics.Debug.WriteLine("item.Image == null");
                    //}
                    System.Diagnostics.Debug.WriteLine("memoryStream != null");
                    //await item.Image.Bitmap.SetSourceAsync(memoryStream.AsRandomAccessStream());
                    await bitmapimage.SetSourceAsync(memoryStream.AsRandomAccessStream());
                    System.Diagnostics.Debug.WriteLine("awaited memory stream != null");

                }
                else
                {
                    using (memoryStream = new MemoryStream())
                    {
                        await contentStream.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;
                        System.Diagnostics.Debug.WriteLine("using (memoryStream = new MemoryStream()");
                        await bitmapimage.SetSourceAsync(memoryStream.AsRandomAccessStream());
                    }
                }
                System.Diagnostics.Debug.WriteLine("must set bgimage");

                //bitmapimage = new BitmapImage(new Uri(item.DownloadUrl)); -> Works too
                Dal.SaveLogEntry(LogType.AppInfo, "Wallpaper Changed");
                item.Viewed = true;
                Dal.ResetIsCurrentWallpaper();
                item.IsCurrentWallPaper = true;
                Dal.SavePicture(item);

                Settings.DashBoardImage = bitmapimage;
            }
            catch (Exception ex)
            {
                Dal.SaveLogEntry(LogType.Error, "Exception  in InternalChangeWallpaperAsync() " + ex.Message);
            }
            finally
            {
                Dal.CheckForViewedPictures();
            }
            return true;
        }

        #endregion
    }

}
