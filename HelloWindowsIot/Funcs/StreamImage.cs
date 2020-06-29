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
using HelloWindowsIot.Models;
using RWPBGTasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Core;

namespace HelloWindowsIot
{
    public class HelperFunc
    {
        public static BGTaskModel MyBgTask { get; set; }
        public static async Task<BitmapImage> StreamImageFromOneDrive()
        {
            try
            {
                BitmapImage bitmapimage = new BitmapImage();
                if (await DAL.AppDataBase.CountAllPictures() == 0)
                {
                    // Check if Backgroundtask is running
                    var ts = BGTasksSettings.ListBgTasks.Where(g => g.Name == BGTasksSettings.LoadImagesFromOneDriveTaskName).FirstOrDefault();
                    if (ts != null)
                    {
                        MyBgTask = ts;
                    }

                    if (MyBgTask != null)
                    {
                        await DAL.AppDataBase.DeleteAllPictures();
                        BackgroundTaskConfig.UnregisterBackgroundTaskByName(BGTasksSettings.LoadImagesFromOneDriveTaskName);

                        ApplicationTrigger trigger3 = new ApplicationTrigger();
                        System.Diagnostics.Debug.WriteLine("Call RegisterBackgroundTask on Setttings ViewModel LoadPictures");
                        var task = await BackgroundTaskConfig.RegisterBackgroundTask(MyBgTask.EntryPoint,
                                                                          BGTasksSettings.LoadImagesFromOneDriveTaskName,
                                                                          trigger3,
                                                                          null);

                        AttachLoadPictureListProgressAndCompletedHandlers(task);

                        // Reset the completion status
                        var settings = ApplicationData.Current.LocalSettings;
                        settings.Values.Remove(BGTasksSettings.LoadImagesFromOneDriveTaskName);

                        //Signal the ApplicationTrigger
                        var result = await trigger3.RequestAsync();
                        Settings.LoadPictureListManually = true;
                    }

                    //var s = await HelloWindowsIotDataBase.GetSetup();
                    //await HelloWindowsIotDataBase.LoadImagesFromOneDriveInDBTable(s.OneDrivePictureFolder);
                }
                else
                {
                    // Get Random ItemInfoResponse from Table 
                    var item = DAL.AppDataBase.GetRandomInfoItemResponse();

                    

                    // Only load a detail view image for image items. Initialize the bitmap from the image content stream.
                    Exception error = null;
                    ItemInfoResponse foundFile = null;
                    Stream contentStream = null;

                    //// Initialize Graph client
                    var accessToken = await GraphService.GetTokenForUserAsync();
                    var graphService = new GraphService(accessToken);
                    try
                    {
                        //TODO: Handle if item is null -> DB eror
                        foundFile = await graphService.GetItem(item.OneDriveId);

                        if (foundFile == null)
                        {
                            await DAL.AppDataBase.SaveLogEntry(LogType.Error, $"Image Not found Id: {item.OneDriveId}");
                        }
                        else
                        {
                            //System.Diagnostics.Debug.WriteLine("Found Image: " + item.Name + " Id: " + item.OneDriveId + item.DownloadUrl);

                        }

                        // Get the file's content
                        contentStream = await graphService.RefreshAndDownloadContent(foundFile, false);

                        if (contentStream == null)
                        {
                            await DAL.AppDataBase.SaveLogEntry(LogType.Error, $"Content Stream not found: {foundFile.Name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        error = ex;
                        await DAL.AppDataBase.SaveLogEntry(LogType.Error, error.Message);
                        DAL.AppDataBase.DeletePicture(item);
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
                    await DAL.AppDataBase.SavePicture(item);
                }
                return bitmapimage;
            }
            catch (Exception ex)
            {
                await DAL.AppDataBase.SaveLogEntry(LogType.Error, "Exception  in StreamImageFromOneDrive(): " + ex.Message);
                return null;
            }
            finally
            {
                await DAL.AppDataBase.SaveLogEntry(LogType.Info, "Dashboard Picture Changed at: " + DateTime.Now);
                DAL.AppDataBase.CheckForViewedPictures();
            } 

        }

        /// <summary>
        /// Attach progress and completed handers to a background task.
        /// </summary>
        /// <param name="task">The task to attach progress and completed handlers to.</param>
        private static void AttachLoadPictureListProgressAndCompletedHandlers(IBackgroundTaskRegistration task)
        {
            task.Progress += new BackgroundTaskProgressEventHandler(OnProgressLoadPictures);
            task.Completed += new BackgroundTaskCompletedEventHandler(OnCompletedLoadPictures);
        }

        /// <summary>
        /// Handle background task progress.
        /// </summary>
        /// <param name="task">The task that is reporting progress.</param>
        /// <param name="e">Arguments of the progress report.</param>
        private static async void OnProgressLoadPictures(IBackgroundTaskRegistration task, BackgroundTaskProgressEventArgs args)
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(
                () =>
                {
                    var progress = "Progress: " + args.Progress + "%";
                }
                , CoreDispatcherPriority.Normal);
        }
        /// <summary>
        /// Handle background task completion.
        /// </summary>
        /// <param name="task">The task that is reporting completion.</param>
        /// <param name="e">Arguments of the completion report.</param>
        private static async void OnCompletedLoadPictures(IBackgroundTaskRegistration task, BackgroundTaskCompletedEventArgs args)
        {
            if (Settings.LoadPictureListManually == true)
            {
                //Unregister App Trigger 
                BackgroundTaskConfig.UnregisterBackgroundTaskByName(BGTasksSettings.LoadImagesFromOneDriveTaskName);
                //Register Backgroundtask 
                var apptask = await BackgroundTaskConfig.RegisterBackgroundTask(MyBgTask.EntryPoint,
                                                                           BGTasksSettings.LoadImagesFromOneDriveTaskName,
                                                                            await DAL.AppDataBase.GetTimeIntervalForTask(BGTasksSettings.LoadImagesFromOneDriveTaskName),
                                                                           null);
            }
            System.Diagnostics.Debug.WriteLine("OnCompleted Picturesloaded");
            Settings.LoadPictureListManually = false;
        }
    }
}
