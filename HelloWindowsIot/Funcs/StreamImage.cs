﻿using AppSettings;
using MSGraph;
using MSGraph.Response;
using System;
using System.Threading;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UwpSqliteDal;
using Windows.UI.Xaml.Media.Imaging;

namespace HelloWindowsIot
{
    public class HelperFunc
    {
        public static async Task StreamImageFromOneDrive()
        {
            try
            {
                if (Dal.GetAllPictures().Count == 0)
                {
                    var s = await Dal.GetSetup();
                    await Dal.LoadImagesFromOneDriveInDBTable(s.OneDrivePictureFolder);
                }
                // Get Random ItemInfoResponse from Table 
                var item = Dal.GetRandomInfoItemResponse();

                BitmapImage bitmapimage = new BitmapImage();

                // Only load a detail view image for image items. Initialize the bitmap from the image content stream.
                Exception error = null;
                ItemInfoResponse foundFile = null;
                Stream contentStream = null;

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
                        System.Diagnostics.Debug.WriteLine("Found Image: " + item.Name + " Id: " + item.OneDriveId + item.DownloadUrl);

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
                Dal.SaveLogEntry(LogType.AppInfo, "Dashboard Picture Changed");
                item.Viewed = true;
                Dal.ResetIsCurrentWallpaper();
                item.IsCurrentWallPaper = true;
                Dal.SavePicture(item);
                Task.Delay(15000);
                Settings.DashBoardImage = bitmapimage;
            }
            catch (Exception ex)
            {
                Dal.SaveLogEntry(LogType.Error, "Exception  in DAL xxxxxxxxx () " + ex.Message);
            }
            finally
            {
                Dal.CheckForViewedPictures();
            }
        }
    }
}
