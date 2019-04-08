using AppSettings;
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
        public static async Task<BitmapImage> StreamImageFromOneDrive()
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
                        await Dal.SaveLogEntry(LogType.Error, $"Image Not found Id: {item.OneDriveId}");
                    }
                    else
                    {
                        //System.Diagnostics.Debug.WriteLine("Found Image: " + item.Name + " Id: " + item.OneDriveId + item.DownloadUrl);

                    }

                    // Get the file's content
                    contentStream = await graphService.RefreshAndDownloadContent(foundFile, false);

                    if (contentStream == null)
                    {
                        await Dal.SaveLogEntry(LogType.Error, $"Content Stream not found: {foundFile.Name}");
                    }
                }
                catch (Exception ex)
                {
                    error = ex;
                    await Dal.SaveLogEntry(LogType.Error, error.Message);
                    Dal.DeletePicture(item);
                    return null;
                }

                // Save the retrieved stream 
                var memoryStream = contentStream as MemoryStream;

                if (memoryStream != null)
                {
                    await bitmapimage.SetSourceAsync(memoryStream.AsRandomAccessStream());

                }
                else
                {
                    using (memoryStream = new MemoryStream())
                    {
                        await contentStream.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;
                        await bitmapimage.SetSourceAsync(memoryStream.AsRandomAccessStream());
                    }
                }
                
                item.Viewed = true;
                await Dal.SavePicture(item);
                return bitmapimage;
            }
            catch (Exception ex)
            {
                await Dal.SaveLogEntry(LogType.Error, "Exception  in StreamImageFromOneDrive(): " + ex.Message);
                return null;
            }
            finally
            {
                await Dal.SaveLogEntry(LogType.Info, "Dashboard Picture Changed at: " + DateTime.Now);
                Dal.CheckForViewedPictures();
            }
        }
    }
}
